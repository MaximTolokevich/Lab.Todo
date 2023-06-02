using Lab.Todo.Api.Extensions;
using Lab.Todo.Api.Filters;
using Lab.Todo.Api.Services.UserServices;
using Lab.Todo.BLL.MappingProfiles;
using Lab.Todo.BLL.Services.AttachmentManagers;
using Lab.Todo.BLL.Services.AttachmentManagers.Enums;
using Lab.Todo.BLL.Services.SqlConnectionStringProviders;
using Lab.Todo.BLL.Services.SqlConnectionStringProviders.ConfigurationExtensions;
using Lab.Todo.BLL.Services.SqlConnectionStringProviders.Options;
using Lab.Todo.BLL.Services.ToDoItemManagers;
using Lab.Todo.BLL.Services.ToDoItemManagers.Builders;
using Lab.Todo.BLL.Services.ToDoItemManagers.Options;
using Lab.Todo.BLL.Services.TokenHandler.Options;
using Lab.Todo.BLL.Services.UniqueFileNameServices;
using Lab.Todo.BLL.Services.UserServices;
using Lab.Todo.DAL.Attachments.Services;
using Lab.Todo.DAL.Attachments.Services.Interfaces;
using Lab.Todo.DAL.Attachments.Services.Options;
using Lab.Todo.DAL.Repositories;
using Lab.Todo.DAL.Repositories.Interfaces;
using Lab.Todo.Web.Common.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Serilog;
using System;
using System.Text;

namespace Lab.Todo.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IStartupFilter, OptionsValidationStartupFilter>();

            services.Configure<SqlConnectionStringOptions>(options => Configuration.BuildSqlConnectionStringOptions(options));

            services.Configure<JwtOptions>(Configuration.GetSection(nameof(JwtOptions)));

            services.AddTransient<ISqlConnectionStringProvider, SqlConnectionStringProvider>();
            services.AddDbContext<ToDoUnitOfWork>((serviceProvider, options) =>
            {
                var connectionStringProvider = serviceProvider.GetService<ISqlConnectionStringProvider>();
                if (connectionStringProvider != null)
                {
                    options.UseSqlServer(connectionStringProvider.GetSqlDatabaseConnectionString());
                }
            });

            services.AddAutoMapper(
                typeof(MappingProfiles.AttachmentProfile),
                typeof(MappingProfiles.CustomFieldProfile),
                typeof(MappingProfiles.TagProfile),
                typeof(MappingProfiles.ToDoItemProfileApi),
                typeof(CustomFieldProfile),
                typeof(AttachmentProfile),
                typeof(ToDoItemDependencyProfile),
                typeof(TagProfile),
                typeof(ToDoItemProfile));

            services.AddLogging(builder => builder.AddSerilog(dispose: true));
            services.AddApplicationInsightsTelemetry();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddJwtSecurityTokenHandler();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUnitOfWork, ToDoUnitOfWork>();
            services.AddTransient<IAttachmentManager, AttachmentManager>();
            services.AddTransient<IUniqueFileNameService, UniqueFileNameService>();
            services.AddTransient<IFileTransactionService, FileTransactionService>();
            services.AddTransient<IFileStorageService, LocalStorageService>();
            services.AddTransient<IToDoItemQueryBuilder, ToDoItemQueryBuilder>();

            services.Configure<ToDoItemManagerOptions>(Configuration.GetSection(nameof(ToDoItemManagerOptions)));

            services.AddTransient<IToDoItemManager, ToDoItemManager>();

            services.AddControllers(options => options.Filters.Add<ExceptionFilter>())
                .AddNewtonsoftJson(options => options.SerializerSettings.Converters.Add(new StringEnumConverter()));
            
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(bearerOptions =>
            {
                var jwtOptions = Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
                
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretValue)),
                    ValidAlgorithms = jwtOptions.EncryptionAlgorithms
                };
            });

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lab.Todo.Api", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = $"JWT Authorization header using the {JwtBearerDefaults.AuthenticationScheme} scheme.{Environment.NewLine}{Environment.NewLine}" +
                                  $"Enter '{JwtBearerDefaults.AuthenticationScheme}' [space] and then your token in the text input below.{Environment.NewLine}{Environment.NewLine}" +
                                  $"Example: \"{JwtBearerDefaults.AuthenticationScheme} 1a2b3c4d5ef\""
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var storageType = Configuration.GetValue<StorageType>("AttachmentsStorage");

            if (storageType == StorageType.Azure)
            {
                services.Configure<AzureStorageOptions>(Configuration.GetSection("AzureStorageOptions"));
                services.AddTransient<IFileStorageService, AzureStorageService>();
            }
            else if (storageType == StorageType.Local)
            {
                services.Configure<LocalStorageOptions>(Configuration.GetSection("LocalStorageOptions"));
                services.AddTransient<IFileStorageService, LocalStorageService>();
            }

            services.AddSingleton(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lab.Todo.Api v1"));
            }

            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging();

            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
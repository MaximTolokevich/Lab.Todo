using Lab.Todo.BLL.MappingProfiles;
using Lab.Todo.BLL.Services.AttachmentManagers;
using Lab.Todo.BLL.Services.AttachmentManagers.Enums;
using Lab.Todo.BLL.Services.SqlConnectionStringProviders;
using Lab.Todo.BLL.Services.SqlConnectionStringProviders.ConfigurationExtensions;
using Lab.Todo.BLL.Services.SqlConnectionStringProviders.Options;
using Lab.Todo.BLL.Services.ToDoItemManagers;
using Lab.Todo.BLL.Services.ToDoItemManagers.Builders;
using Lab.Todo.BLL.Services.ToDoItemManagers.Options;
using Lab.Todo.BLL.Services.UniqueFileNameServices;
using Lab.Todo.BLL.Services.UserServices;
using Lab.Todo.DAL.Attachments.Services;
using Lab.Todo.DAL.Attachments.Services.Interfaces;
using Lab.Todo.DAL.Attachments.Services.Options;
using Lab.Todo.DAL.Repositories;
using Lab.Todo.DAL.Repositories.Interfaces;
using Lab.Todo.Web.Common.Filters;
using Lab.Todo.Web.Common.Services;
using Lab.Todo.Web.Filters;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Serilog;
using System;
using System.Text.Json.Serialization;

namespace Lab.Todo.Web
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

            services.AddTransient<ISqlConnectionStringProvider, SqlConnectionStringProvider>();
            services.AddDbContext<ToDoUnitOfWork>((serviceProvider, options) =>
            {
                var connectionStringProvider = serviceProvider.GetService<ISqlConnectionStringProvider>();
                if (connectionStringProvider is null)
                {
                    throw new ArgumentNullException(null, $"{nameof(connectionStringProvider)} can't be null");
                }

                options.UseSqlServer(connectionStringProvider.GetSqlDatabaseConnectionString());
            });

            services.AddAutoMapper(
                typeof(MappingProfiles.AttachmentProfile),
                typeof(MappingProfiles.CustomFieldProfile),
                typeof(MappingProfiles.TagProfile),
                typeof(MappingProfiles.ToDoItemProfile),
                typeof(CustomFieldProfile),
                typeof(AttachmentProfile),
                typeof(ToDoItemDependencyProfile),
                typeof(TagProfile),
                typeof(ToDoItemProfile));

            services.AddLogging(builder => builder.AddSerilog(dispose: true));
            services.AddApplicationInsightsTelemetry();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUnitOfWork, ToDoUnitOfWork>();
            services.AddTransient<IToDoItemQueryBuilder, ToDoItemQueryBuilder>();

            services.Configure<ToDoItemManagerOptions>(Configuration.GetSection("ToDoItemManagerOptions"));

            services.AddTransient<IToDoItemManager, ToDoItemManager>();
            services.AddTransient<IAttachmentManager, AttachmentManager>();
            services.AddTransient<IUniqueFileNameService, UniqueFileNameService>();
            services.AddTransient<IFileTransactionService, FileTransactionService>();

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

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<ExceptionFilter>();
                options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build()));
                options.Filters.Add<ModelStateValidationActionFilterAttribute>();
            });
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection(Constants.Configuration.AzureActiveDirectory));
            services.AddAuthorization();

            services.AddRazorPages()
                .AddMvcOptions(options => options.Filters.Add<ExceptionFilter>())
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddSingleton(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
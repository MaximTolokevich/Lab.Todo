using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Lab.Todo.Web.Common.Filters
{
    public class OptionsValidationStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> builderAction) =>
            builderAction + (applicationBuilder =>
            {
                var services = (IServiceCollection)applicationBuilder.ApplicationServices.GetService(typeof(IServiceCollection))
                               ?? throw new ApplicationException($"{nameof(applicationBuilder.ApplicationServices)} does not contain {nameof(IServiceCollection)} service.");

                var optionsInstances = services
                    .Select(serviceDescriptor => serviceDescriptor.ServiceType)
                    .Where(serviceType => IsAssignableToGenericType(givenType: serviceType, genericType: typeof(IConfigureOptions<>)))
                    .Select(optionsType => optionsType.GetGenericArguments()[0])
                    .Where(optionType => optionType.Assembly.FullName is not null && optionType.Assembly.FullName.StartsWith("Lab.Todo"))
                    .Select(optionsTypeArgument => applicationBuilder.ApplicationServices.GetService(typeof(IOptions<>).MakeGenericType(optionsTypeArgument)))
                    .Select(option => option!.GetType()
                        .GetProperty(nameof(IOptions<object>.Value))!
                        .GetValue(option))
                    .Where(option => option is not null);

                var validationResults = new List<ValidationResult>();

                foreach (var options in optionsInstances)
                {
                    if (!Validator.TryValidateObject(options, new ValidationContext(options), validationResults, validateAllProperties: true))
                    {
                        var optionsType = options.GetType();

                        throw new OptionsValidationException(optionsType.Name, optionsType, validationResults.Select(result => result.ErrorMessage));
                    }
                }
            });

        private static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            if (givenType.GetInterfaces().Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
            {
                return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            var baseType = givenType.BaseType;

            return baseType is not null && IsAssignableToGenericType(baseType, genericType);
        }
    }
}
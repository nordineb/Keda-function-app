// [assembly: Microsoft.Azure.Functions.Extensions.DependencyInjection.FunctionsStartup(typeof(AzFuncApp.Startup.FunctionsHostBuilder))]
namespace AzFuncApp.Startup
{

    using System;
    using AzFuncApp.invocationfilters;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;

    public class FunctionsHostBuilder : IFunctionsHostBuilder
    {
        public FunctionsHostBuilder(IServiceCollection services)
        {
            var serviceCollection = services;

            services = serviceCollection ?? throw new ArgumentNullException(nameof(services));

#pragma warning disable 612, 618
            services.AddSingleton<IFunctionFilter, TestFilter>();
#pragma warning restore 612, 618
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "My Api", Version = "v1" });

                options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Scheme = "bearer"
                });

                //options.OperationFilter<AuthenticationRequirementsOperationFilter>();
            }
            );
        }
        public IServiceCollection Services { get; }

        IServiceCollection IFunctionsHostBuilder.Services => throw new NotImplementedException();
    }
}

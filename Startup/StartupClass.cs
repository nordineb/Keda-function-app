using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(AzFuncApp.Startup.StartupClass))]
namespace AzFuncApp.Startup
{
    using System.Reflection;
    using AzureFunctions.Extensions.Swashbuckle;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Hosting;

    public class StartupClass : IWebJobsStartup
    {

        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
            Configure(new FunctionsHostBuilder(builder.Services));
        }
        private static void Configure(IFunctionsHostBuilder builder)
        { 
            // configure services here 
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace MyCRM
{
    public class HelloWorld : IPlugin
    {
        public string UnSecureConfig { get; set; }
        public string SecureConfig { get; set; }

        // Constructor can retrieve the secure & unSecure configuration data
        // Please config in the plugin registeration tool
        public HelloWorld(string unSecureConfig, string secureConfig)
        {
            this.UnSecureConfig = unSecureConfig;
            this.SecureConfig = secureConfig;
        }
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.  
            // If you are not registering the plug-in in the sandbox, then you do  
            // not have to add any tracing service related code.  
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the organization service reference which you will need for  
            // web service calls.  
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);



            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"];


                try
                {
                    // Plug-in business logic goes here.  
                    var firstName = string.Empty;
                    var lastName = string.Empty;

                    // Read from atttribute values
                    if (entity.Attributes.Contains("firstname"))
                    {
                        // Return first name
                        firstName = entity.Attributes["firstname"].ToString();
                    }

                    // Retrun last name
                    lastName = entity.Attributes["lastname"].ToString();

                    // Assign data to attributes
                    var welcomeText = $"Hello World {firstName} {lastName}";
                    entity.Attributes.Add("description", welcomeText);
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in MyPlug-in.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("MyPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}

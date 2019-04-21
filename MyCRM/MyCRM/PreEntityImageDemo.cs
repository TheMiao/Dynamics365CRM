using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using System.ServiceModel;

namespace MyPlugins
{
    public class PreEntityImageDemo : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.  
            // If you are not registering the plug-in in the sandbox, then you do  
            // not have to add any tracing service related code.  
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the organization service reference which you will need for  
            // web service calls.  
            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);



            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"];


                try
                {
                    // Plug-in business logic goes here.  


                    //Pre and post entity images
                    // Snapshots of the primary entity's attributes from database before (pre) and after (post) the core platfomr operation.

                    // This is modified business phone number
                    string modifiedBusinessPhone = entity.Attributes["telephone1"].ToString();

                    // Retrieve preEntity Images
                    Entity preImage = (Entity)context.PreEntityImages["PreImage"];

                    // Normally we don't retrieve post entity images with following reason
                    // 1. After the postOperation, we are on the current situation modifed data
                    // 2. When retrieve the postEntityImages, due to it is not created, the entity will be null.
                    // Entity postImage = (Entity)context.PostEntityImages["PostImage"];

                    // Retrieve original business phone number
                    string originalBusinessPhone = preImage.Attributes["telephone1"].ToString();

                    // Display the information in the error log
                    throw new InvalidPluginExecutionException("Phone number is changed from" + originalBusinessPhone + " to " + modifiedBusinessPhone);
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

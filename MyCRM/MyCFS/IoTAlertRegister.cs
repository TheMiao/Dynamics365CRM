using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using System.ServiceModel;
using System.Net.Http;
using Microsoft.Xrm.Sdk.Messages;

namespace MyCFS
{
    public class IoTAlertRegister : IPlugin
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
                    // var deviceId = entity.Attributes["msdyn_deviceid"].ToString();
                    var alertId =  entity.Attributes["msdyn_iotalertid"].ToString();

                    Entity newCommand = new Entity("msdyn_iotdevicecommand");
                    newCommand.Attributes.Add("msdyn_iotdevicecommandid", Guid.NewGuid().ToString());
                    //newCommand.Attributes.Add("_msdyn_parentalert_value", deviceId);
                    // newCommand.Attributes.Add("_msdyn_parentalert_value", alertId);
                    newCommand.Attributes.Add("msdyn_name", "Hello World from Plugin");
                    // newCommand.Attributes.Add("_createdby_value	", "7dcd4bcc-e37a-e911-a83a-000d3a07fbb4");
                    newCommand.Attributes.Add("statecode", 0);
                    newCommand.Attributes.Add("statuscode", 1);
                    newCommand.Attributes.Add("msdyn_deviceid", "Ava-Device-05");
                    newCommand.Attributes.Add("versionnumber", 1234567);
                    newCommand.Attributes.Add("createdon", DateTime.Now);
                    newCommand.Attributes.Add("modifiedon", DateTime.Now);
                    newCommand.Attributes.Add("msdyn_message", "{\"CommandName\":\"Reset Thermostat\",\"Parameters\":\"\"}");
                    newCommand.Attributes.Add("msdyn_commandstatus", 192350000);
                    newCommand.Attributes.Add("msdyn_sendtoallconnecteddevices", 0);
                    // newCommand.Attributes.Add("_owningbusinessunit_value", "33248697-8a79-e911-a9a5-000d3aa37759");
                    // newCommand.Attributes.Add("_modifiedonbehalfby_value", "7dcd4bcc-e37a-e911-a83a-000d3a07fbb4");
                    //newCommand.Attributes.Add("_msdyn_command_value", "f5a0898e-018c-e911-a841-000d3a07f3d7");
                    //newCommand.Attributes.Add("_ownerid_value", "7dcd4bcc-e37a-e911-a83a-000d3a07fbb4");
                    //newCommand.Attributes.Add("_owninguser_value", "7dcd4bcc-e37a-e911-a83a-000d3a07fbb4");
                    // newCommand.Attributes.Add("_modifiedby_value", "7dcd4bcc-e37a-e911-a83a-000d3a07fbb4");


                    //service.Create(newCommand);
                    CreateRequest createRequest = new CreateRequest();
                    createRequest.Target = newCommand;
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in MyPlug-in.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("MyPlugin: {0}", ex.ToString());
                }
            }
        }
    }
}

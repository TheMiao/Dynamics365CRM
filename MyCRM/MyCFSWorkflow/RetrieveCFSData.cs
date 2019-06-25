using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCFSWorkflow
{
    public class RetrieveCFSData : CodeActivity
    {
        [Input("Key")]
        public InArgument<string> Key { get; set; }

        [ReferenceTarget("msdyn_iotalert")]
        [Output("IoTAlertId")]
        public OutArgument<EntityReference> IoTAlertId { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            //Create the tracing service
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            //Create the context
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity iotAlert = (Entity)context.InputParameters["Target"];
                var alertId = iotAlert.Attributes["msdyn_iotalertid"].ToString();

                var key = Key.Get(executionContext);

                // Get data from Configuraton Entity
                // Call organization web service


                // retrieve the configuration in the cfsdemo_cfs_configuration list
                // *********************************** //
                // Input custom entity logical name
                //QueryByAttribute query = new QueryByAttribute("cfsdemo_cfs_configuration");
                //// Query Condition, input value logical name
                //query.ColumnSet = new ColumnSet(new string[] { "cfsdemo_avacfsiotdevicecommandid" });
                //query.AddAttributeValue("cfsdemo_name", key);

                //EntityCollection collection = service.RetrieveMultiple(query);

                //if (collection.Entities.Count != 1)
                //{
                //    tracingService.Trace("Something is wrong with configuration");
                //}

                //Entity config = collection.Entities.FirstOrDefault();
                //tracingService.Trace(config.Attributes["cfsdemo_avacfsiotdevicecommandid"].ToString());

                // *********************************** //


                //IoTAlertId.Set(executionContext, alertId);
                //Update Record by using Custom Assembly output parameter
                var iotAlertRef = new EntityReference("msdyn_iotalert", new Guid(alertId));
                iotAlertRef.Name = "Hello World From Workflow";
                IoTAlertId.Set(executionContext, iotAlertRef);
            }
            
        }
    }
}

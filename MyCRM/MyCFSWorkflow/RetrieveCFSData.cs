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

        [Output("CustomerAssetsId")]
        public OutArgument<string> CustomerAssetsId { get; set; }

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

                //Update Record by using Custom Assembly output parameter
                var alertId = iotAlert.Attributes["msdyn_iotalertid"].ToString();
                var iotAlertRef = new EntityReference("msdyn_iotalert", new Guid(alertId));
                iotAlertRef.Name = "Hello World From Workflow";
                IoTAlertId.Set(executionContext, iotAlertRef);
                tracingService.Trace(iotAlertRef.Name, iotAlertRef);


                // Retrieve CustomerAssetsId
                var customerAssetsId = iotAlert.Attributes["msdyn_CustomerAsset"].ToString();
                CustomerAssetsId.Set(executionContext, customerAssetsId);
            }
        }
    }
}

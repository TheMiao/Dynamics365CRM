using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace MyCRM
{
    public class ChangeIoTAlertBpfStage : IPlugin
    {
        public Entity RetrievedProcessInstance { get; set; }
        public Entity IotAlert { get; set; }

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
                IotAlert = (Entity)context.InputParameters["Target"];

                try
                {
                    ChangeStage(service);
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

        public void ChangeStage(IOrganizationService service)
        {
            // Get Process Instances
            var processInstanceRequest = new RetrieveProcessInstancesRequest
            {

                EntityId = new Guid(IotAlert.Id.ToString()),
                EntityLogicalName = IotAlert.LogicalName.ToString()
            };

            var processInstanceResponse = (RetrieveProcessInstancesResponse)service.Execute(processInstanceRequest);

            // Declare variables to store values returned in response
            int processCount = processInstanceResponse.Processes.Entities.Count;
            Entity activeProcessInstance = processInstanceResponse.Processes.Entities[0]; // First record is the active process instance
            Guid activeProcessInstanceID = activeProcessInstance.Id; // Id of the active process instance, which will be used later to retrieve the active path of the process instance

            // Retrieve the active stage ID of in the active process instance
            Guid activeStageID = new Guid(activeProcessInstance.Attributes["processstageid"].ToString());

            // Retrieve the process stages in the active path of the current process instance
            RetrieveActivePathRequest pathReq = new RetrieveActivePathRequest
            {
                ProcessInstanceId = activeProcessInstanceID
            };
            RetrieveActivePathResponse pathResp = (RetrieveActivePathResponse)service.Execute(pathReq);

            string activeStageName = "";
            int activeStagePosition = -1;

            Console.WriteLine("\nRetrieved stages in the active path of the process instance:");
            for (int i = 0; i < pathResp.ProcessStages.Entities.Count; i++)
            {
                // Retrieve the active stage name and active stage position based on the activeStageId for the process instance
                if (pathResp.ProcessStages.Entities[i].Attributes["processstageid"].ToString() == activeStageID.ToString())
                {
                    activeStageName = pathResp.ProcessStages.Entities[i].Attributes["stagename"].ToString();
                    activeStagePosition = i;
                }
            }

            // Retrieve the stage ID of the next stage that you want to set as active
            if (activeStagePosition < pathResp.ProcessStages.Entities.Count)
            {
                activeStageID = (Guid)pathResp.ProcessStages.Entities[activeStagePosition + 1].Attributes["processstageid"];
            }
            else
            {
                Console.WriteLine("You are at latest stage");
                return;
            }

            // Retrieve IoT alert Id to match the specific bpf
            var query = new QueryExpression
            {
                EntityName = "msdyn_bpf_477c16f59170487b8b4dc895c5dcd09b",
                ColumnSet = new ColumnSet("bpf_name", "bpf_msdyn_iotalertid", "activestageid")
            };
            // query.Criteria.AddCondition("processid", ConditionOperator.Equal, activeProcessInstanceID);
            var retrievedProcessInstanceList = service.RetrieveMultiple(query);
            foreach (var entity in retrievedProcessInstanceList.Entities)
            {
                var ioTAlertER = (EntityReference)entity.Attributes["bpf_msdyn_iotalertid"];
                if (IotAlert.Id == ioTAlertER.Id)
                {
                    RetrievedProcessInstance = entity;
                    break;
                }
            }


            // Retrieve the process instance record to update its active stage

            if (activeStagePosition == 0 && activeStageName == "Created")
            {
                // create a case here assign the case to the "Create Case" stage
                var nextStageId = new EntityReference()
                {
                    Id = new Guid("ae10c50d-3980-0e80-1293-64672a1e1301"),
                    Name = "Create Case",
                    LogicalName = "processstage"
                };

                // Create the Case here
                var newIncidents = new Entity("incident");
                var newGuid = Guid.NewGuid();
                newIncidents.Id = newGuid;
                newIncidents.Attributes["incidentid"] = newGuid;
                newIncidents.Attributes.Add("title", IotAlert.Attributes["msdyn_description"].ToString());

                // Retrieve Customer information from IoT Alert Customer Assets
                var customerAsset = (EntityReference)IotAlert.Attributes["msdyn_customerasset"]; ;
                var customerAssetIdQuery = new QueryExpression
                {
                    EntityName = "msdyn_customerasset",
                    ColumnSet = new ColumnSet("msdyn_account", "msdyn_name", "msdyn_customerassetid"),
                    Criteria = new FilterExpression()
                };
                customerAssetIdQuery.Criteria.AddCondition("msdyn_customerassetid", ConditionOperator.Equal, customerAsset.Id);
                var customerAssetIdCollection = service.RetrieveMultiple(customerAssetIdQuery);
                if (customerAssetIdCollection.Entities.Count <= 1)
                {
                    var accountER = (EntityReference)customerAssetIdCollection[0].Attributes["msdyn_account"];

                    //TODO: need to confirm it is account or contact
                    newIncidents.Attributes.Add("customerid", accountER);
                }
                else
                {
                    //more than one value, need developer to do investigation
                    return;
                }

                // Retrieve IoT Alert EntityReference
                var iotAlertER = new EntityReference
                {
                    Id = IotAlert.Id,
                    LogicalName = IotAlert.LogicalName,
                    Name = IotAlert.Attributes["msdyn_description"].ToString()
                };
                newIncidents.Attributes.Add("msdyn_iotalert", iotAlertER);


                //var createRequest = new CreateRequest();
                //createRequest.Target = newIncidents;
                service.Create(newIncidents);

                RetrievedProcessInstance.Attributes["activestageid"] = nextStageId;
                // Link the case with

            }
            else if (activeStagePosition == 1 && activeStageName == "Create Work Order")
            {
                // create a Work Order here assign the case to the "Create Work Order" stage
                var nextStageId = new EntityReference()
                {
                    Id = new Guid("be272128-a40a-edeb-0769-6683ea3a3857"),
                    Name = "Create Work Order",
                    LogicalName = "processstage"
                };
                RetrievedProcessInstance.Attributes["activestageid"] = nextStageId;
            }
            else if (activeStagePosition == 2 && activeStageName == "Schedule Work Order")
            {
                // create a Work Order here assign the case to the "Schedule Work Order" Stage
                var nextStageId = new EntityReference()
                {
                    Id = new Guid("41b3d5c3-b292-cb62-369b-6a70a92cb7b8"),
                    Name = "Schedule Work Order",
                    LogicalName = "processstage"
                };
                RetrievedProcessInstance.Attributes["activestageid"] = nextStageId;
            }

            // Set the next stage as the active stage
            // service.Update(RetrievedProcessInstance);
        }

    }
}

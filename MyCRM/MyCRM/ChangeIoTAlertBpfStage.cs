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
        public EntityReference PrimaryAssetER { get; set; } = new EntityReference();
        public EntityReference IoTAlertER { get; set; } = new EntityReference();
        public Entity NewWorkOrder { get; set; }
        public EntityReference AccountER { get; set; }

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

                // Retrieve IoT Alert EntityReference
                IoTAlertER.Id = IotAlert.Id;
                IoTAlertER.LogicalName = IotAlert.LogicalName;
                IoTAlertER.Name = IotAlert.Attributes["msdyn_description"].ToString();

                try
                {
                    var sendCommandResult = SendCommand(service);
                    if (sendCommandResult)
                    {
                        CloseCurrentIotAlert(service);
                    }
                    //ChangeStage(service);
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

            var activeStageName = "";
            var activeStagePosition = -1;

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
                var ioTAlertBpfER = (EntityReference)entity.Attributes["bpf_msdyn_iotalertid"];
                if (IotAlert.Id == ioTAlertBpfER.Id)
                {
                    RetrievedProcessInstance = entity;
                    break;
                }
            }

            //Retrieve Asset's account id
            var customerAsset = (EntityReference)IotAlert.Attributes["msdyn_customerasset"]; ;
            var customerAssetIdQuery = new QueryExpression
            {
                EntityName = "msdyn_customerasset",
                ColumnSet = new ColumnSet("msdyn_account", "msdyn_name", "msdyn_customerassetid"),
                Criteria = new FilterExpression()
            };
            customerAssetIdQuery.Criteria.AddCondition("msdyn_customerassetid", ConditionOperator.Equal, customerAsset.Id);
            var customerAssetIdCollection = service.RetrieveMultiple(customerAssetIdQuery);
            AccountER = new EntityReference();
            if (customerAssetIdCollection.Entities.Count <= 1)
            {
                //TODO: need to confirm it is account or contact

                AccountER = (EntityReference)customerAssetIdCollection[0].Attributes["msdyn_account"];

                PrimaryAssetER.Id = customerAssetIdCollection[0].Id;
                PrimaryAssetER.LogicalName = customerAssetIdCollection[0].LogicalName;
                PrimaryAssetER.Name = customerAssetIdCollection[0].Attributes["msdyn_name"].ToString();
            }
            else
            {
                //more than one value, need developer to do investigation
                return;
            }

            // Retrieve the process instance record to update its active stage
            // activeStagePosition == 0 && activeStageName == "Created"
            CreateCase(service);
            // activeStagePosition == 1 && activeStageName == "Create Work Order"
            CreateWorkOrder(service);
            // activeStagePosition == 2 && activeStageName == "Schedule Work Order"
            ScheduleWorkOrder(service);

            // Set the next stage as the active stage
            service.Update(RetrievedProcessInstance);
        }



        public void CreateCase(IOrganizationService service)
        {
            // Create the Case here
            var newIncidents = new Entity("incident");
            var newGuid = Guid.NewGuid();
            newIncidents.Id = newGuid;
            newIncidents.Attributes["incidentid"] = newGuid;
            newIncidents.Attributes.Add("title", IotAlert.Attributes["msdyn_description"].ToString());

            // Retrieve Customer information from IoT Alert Customer Assets
            newIncidents.Attributes.Add("customerid", AccountER);



            newIncidents.Attributes.Add("msdyn_iotalert", IoTAlertER);


            //var createRequest = new CreateRequest();
            //createRequest.Target = newIncidents;
            service.Create(newIncidents);


            // create a case here assign the case to the "Create Case" stage
            var nextStageId = new EntityReference()
            {
                Id = new Guid("ae10c50d-3980-0e80-1293-64672a1e1301"),
                Name = "Create Case",
                LogicalName = "processstage"
            };
            RetrievedProcessInstance.Attributes["activestageid"] = nextStageId;
            // Link the case with

        }
        public void CreateWorkOrder(IOrganizationService service)
        {
            // Create work order
            NewWorkOrder = new Entity("msdyn_workorder");

            //var workOrderQuery = new QueryExpression
            //{
            //    EntityName = "msdyn_workorder",
            //    ColumnSet = new ColumnSet("msdyn_name"),
            //    //Criteria = new FilterExpression()
            //};
            //// workOrderQuery.Criteria.AddCondition("msdyn_customerassetid", ConditionOperator.Equal, customerAsset.Id);
            //var workOrderCollection = service.RetrieveMultiple(workOrderQuery);

            var newWorkOrderGuid = Guid.NewGuid();
            NewWorkOrder.Id = newWorkOrderGuid;
            NewWorkOrder.Attributes.Add("msdyn_workorderid", newWorkOrderGuid);

            NewWorkOrder.Attributes.Add("msdyn_name", DateTime.Now.ToString());
            NewWorkOrder.Attributes.Add("msdyn_systemstatus", new OptionSetValue(690970000));
            NewWorkOrder.Attributes.Add("msdyn_serviceaccount", AccountER);

            // Retrieve Primary Incident details
            // TODO: Will hard code here for demo purpose, we could search out the incident type by current IoT Alert type.
            // Primary Incident Type
            var incidentTypeQuery = new QueryExpression
            {
                EntityName = "msdyn_incidenttype",
                ColumnSet = new ColumnSet("msdyn_name", "msdyn_estimatedduration"),
                Criteria = new FilterExpression()
            };
            incidentTypeQuery.Criteria.AddCondition("msdyn_name", ConditionOperator.Equal, "Unit Overheating");
            var incidentTypeCollection = service.RetrieveMultiple(incidentTypeQuery);
            var incidentTypeER = new EntityReference();
            var estimatedDuration = 0;
            if (incidentTypeCollection.Entities.Count <= 1)
            {
                incidentTypeER.Id = incidentTypeCollection.Entities[0].Id;
                incidentTypeER.LogicalName = incidentTypeCollection.Entities[0].LogicalName;
                incidentTypeER.Name = incidentTypeCollection.Entities[0].Attributes["msdyn_name"].ToString();
                int.TryParse(incidentTypeCollection.Entities[0].Attributes["msdyn_estimatedduration"].ToString(), out estimatedDuration);
            }
            NewWorkOrder.Attributes.Add("msdyn_primaryincidenttype", incidentTypeER);
            // Primary Incident Estimated Duration
            NewWorkOrder.Attributes.Add("msdyn_primaryincidentestimatedduration", estimatedDuration);
            // IoT Alert
            NewWorkOrder.Attributes.Add("msdyn_iotalert", IoTAlertER);
            // Primary Incident Customer Asset
            NewWorkOrder.Attributes.Add("msdyn_customerasset", PrimaryAssetER);


            // Retrieve workorder type
            var workorderTypeQuery = new QueryExpression
            {
                EntityName = "msdyn_workordertype",
                ColumnSet = new ColumnSet("msdyn_name"),
            };
            var wokrorderTypeCollection = service.RetrieveMultiple(workorderTypeQuery);
            var workOrderTypeER = new EntityReference
            {
                Id = wokrorderTypeCollection[0].Id,
                LogicalName = wokrorderTypeCollection[0].LogicalName,
                Name = wokrorderTypeCollection[0].Attributes["msdyn_name"].ToString()
            };
            NewWorkOrder.Attributes.Add("msdyn_workordertype", workOrderTypeER);

            // Retrieve price list
            var priceListQuery = new QueryExpression
            {
                EntityName = "pricelevel",
                ColumnSet = new ColumnSet("pricelevelid", "name")
            };
            var priceListCollection = service.RetrieveMultiple(priceListQuery);

            var priceListER = new EntityReference();
            priceListER.Id = priceListCollection[0].Id;
            priceListER.LogicalName = priceListCollection[0].LogicalName;
            priceListER.Name = priceListCollection[0].Attributes["name"].ToString();
            NewWorkOrder.Attributes.Add("msdyn_pricelist", priceListER);


            service.Create(NewWorkOrder);
            // create a Work Order here assign the case to the "Create Work Order" stage
            var nextStageId = new EntityReference()
            {
                Id = new Guid("be272128-a40a-edeb-0769-6683ea3a3857"),
                Name = "Create Work Order",
                LogicalName = "processstage"
            };
            RetrievedProcessInstance.Attributes["activestageid"] = nextStageId;
        }
        public void ScheduleWorkOrder(IOrganizationService service)
        {
            // Create Bookings
            var newBooking = new Entity("bookableresourcebooking");

            // TODO: We can have better schedule the Start and End Time depending on the location and the time schdule of Assigned Resource (Engineer)
            // Current schedule is hard coded
            var newBookingGuid = Guid.NewGuid();
            newBooking.Attributes.Add("bookableresourcebookingid", newBookingGuid);
            newBooking.Id = newBookingGuid;
            newBooking.Attributes.Add("name", "IoT Alert" + DateTimeOffset.UtcNow);
            newBooking.Attributes.Add("starttime", DateTimeOffset.Now.AddDays(1).UtcDateTime);
            newBooking.Attributes.Add("endtime", DateTimeOffset.Now.AddDays(1).AddHours(2).UtcDateTime);
            // Duration
            var incidentTypeQuery = new QueryExpression
            {
                EntityName = "msdyn_incidenttype",
                ColumnSet = new ColumnSet("msdyn_name", "msdyn_estimatedduration"),
                Criteria = new FilterExpression()
            };
            incidentTypeQuery.Criteria.AddCondition("msdyn_name", ConditionOperator.Equal, "Unit Overheating");
            var incidentTypeCollection = service.RetrieveMultiple(incidentTypeQuery);
            var estimatedDuration = 0;
            if (incidentTypeCollection.Entities.Count <= 1)
            {
                int.TryParse(incidentTypeCollection.Entities[0].Attributes["msdyn_estimatedduration"].ToString(), out estimatedDuration);
            }
            newBooking.Attributes.Add("duration", estimatedDuration);

            // Resource
            var bookableresourceQuery = new QueryExpression
            {
                EntityName = "bookableresource",
                ColumnSet = new ColumnSet("name"),
                Criteria = new FilterExpression()
            };
            bookableresourceQuery.Criteria.AddCondition("bookableresourceid", ConditionOperator.Equal, "19f73bd9-17f3-e611-8112-e0071b66bf01");
            var bookableResourceCollection = service.RetrieveMultiple(bookableresourceQuery);
            var bookableResourceER = new EntityReference();
            bookableResourceER.Id = bookableResourceCollection.Entities[0].Id;
            bookableResourceER.LogicalName = bookableResourceCollection.Entities[0].LogicalName;
            bookableResourceER.Name = bookableResourceCollection.Entities[0].Attributes["name"].ToString();
            newBooking.Attributes.Add("resource", bookableResourceER);

            // Booking Status
            var bookingStatusQuery = new QueryExpression
            {
                EntityName = "bookingstatus",
                ColumnSet = new ColumnSet("name"),
                Criteria = new FilterExpression()
            };
            bookingStatusQuery.Criteria.AddCondition("bookingstatusid", ConditionOperator.Equal, "f16d80d1-fd07-4237-8b69-187a11eb75f9");
            var bookingStatusCollection = service.RetrieveMultiple(bookingStatusQuery);
            var bookingStatusER = new EntityReference();
            bookingStatusER.Id = bookingStatusCollection.Entities[0].Id;
            bookingStatusER.LogicalName = bookingStatusCollection.Entities[0].LogicalName;
            bookingStatusER.Name = bookingStatusCollection.Entities[0].Attributes["name"].ToString();
            newBooking.Attributes.Add("bookingstatus", bookingStatusER);

            newBooking.Attributes.Add("bookingtype", new OptionSetValue(2));

            // Resource Requirement. It is msdyn_resourcerequirement not msdyn_workorder
            var resourceRequirementQuery = new QueryExpression
            {
                EntityName = "msdyn_resourcerequirement",
                ColumnSet = new ColumnSet("msdyn_name"),
                Criteria = new FilterExpression()
            };
            resourceRequirementQuery.Criteria.AddCondition("msdyn_workorder", ConditionOperator.Equal, "48feaa78-424b-4519-80ff-4ac2212c5e79");
            var resourceRequiremenCollection = service.RetrieveMultiple(resourceRequirementQuery);
            var resourceRequirementER = new EntityReference();
            resourceRequirementER.Id = resourceRequiremenCollection.Entities[0].Id;
            resourceRequirementER.LogicalName = resourceRequiremenCollection.Entities[0].LogicalName;
            resourceRequirementER.Name = resourceRequiremenCollection.Entities[0].Attributes["msdyn_name"].ToString();
            newBooking.Attributes.Add("msdyn_resourcerequirement", resourceRequirementER);

            service.Create(newBooking);
            // create a Work Order here assign the case to the "Schedule Work Order" Stage
            var nextStageId = new EntityReference()
            {
                Id = new Guid("41b3d5c3-b292-cb62-369b-6a70a92cb7b8"),
                Name = "Schedule Work Order",
                LogicalName = "processstage"
            };
            RetrievedProcessInstance.Attributes["activestageid"] = nextStageId;
        }

        public bool SendCommand(IOrganizationService service)
        {
            /* Method Function
             * 
             * The SendCommand method will include the following functions
             * 1. When the high temperature IoT Alert created, send reboot command
             * 2. If the high temperature IoT Alert created again in 5 minutes, start the ChangeStage function
             * 
             */

            // Retrieve IoT Alert
            var iotAlertQuery = new QueryExpression
            {
                EntityName = "msdyn_iotalert",
                ColumnSet = new ColumnSet("msdyn_description", "createdon", "msdyn_customerasset")
            };
            iotAlertQuery.Criteria.AddCondition("msdyn_iotalertid", ConditionOperator.Equal, IotAlert.Id);
            var iotAlertCollection = service.RetrieveMultiple(iotAlertQuery);

            foreach (var item in iotAlertCollection.Entities)
            {
                // condition, matching the situation now, first iot alert or more than once.
                var itemIotAlertCustomerAsset = item.Attributes["msdyn_customerasset"].ToString().ToLower();
                var currentIotAlertCustomerAsset = IotAlert.Attributes["msdyn_customerasset"].ToString().ToLower();
                var itemIotAlertAssetCreateTime = (DateTime)item.Attributes["createdon"];
                var currentIotAlertCreateTime = (DateTime)item.Attributes["createdon"];

                if (DateTimeOffset.Compare(currentIotAlertCreateTime, itemIotAlertAssetCreateTime) > 5)
                {
                    CreateCommand(service);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public void CreateCommand(IOrganizationService service)
        {
            Entity newCommand = new Entity("msdyn_iotdevicecommand");
            var newCommandGuid = Guid.NewGuid();
            newCommand.Id = newCommandGuid;
            newCommand.Attributes.Add("msdyn_iotdevicecommandid", newCommandGuid);
            newCommand.Attributes.Add("msdyn_name", "Reset to Default from Plugin");
            newCommand.Attributes.Add("statecode", 0);
            newCommand.Attributes.Add("statuscode", 1);
            newCommand.Attributes.Add("msdyn_deviceid", IotAlert.Attributes["msdyn_deviceid"].ToString());
            newCommand.Attributes.Add("versionnumber", 1234567);
            newCommand.Attributes.Add("createdon", DateTime.UtcNow);
            newCommand.Attributes.Add("msdyn_message", "{\"CommandName\":\"Reset Thermostat\",\"Parameters\":\"\"}");
            newCommand.Attributes.Add("msdyn_commandstatus", new OptionSetValue(192350000));
            newCommand.Attributes.Add("msdyn_sendtoallconnecteddevices", false);
            newCommand.Attributes.Add("msdyn_customerasset", (EntityReference)IotAlert.Attributes["msdyn_customerasset"]);
            newCommand.Attributes.Add("msdyn_device", (EntityReference)IotAlert.Attributes["msdyn_device"]);
            newCommand.Attributes.Add("msdyn_parentalert", IoTAlertER);

            service.Create(newCommand);
        }

        public void CloseCurrentIotAlert(IOrganizationService service)
        {
            IotAlert.Attributes["statecode"] = 2;
            service.Update(IotAlert);
        }
    }
}

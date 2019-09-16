function ContactLoad() {
    alert("This is Onload event of From");
}

function ContactSave() {
    alert("This is Save event of Form");
}

function EmailOnChange() {
    alert("This is OnChange event of email attribute");
}

function DisplayHelloWorld(executionContext) {
    var formContext = executionContext.getFormContext();

    // var firstName = formContext.data.entity.attributes.get("firstname").getValue();

    // this is shortcut
    var firstName = formContext.getAttribute("firstname").getValue();

    alert("Hello World " + firstName);
}

// Converting functions to Namespace Notation
var Sdk = window.Sdk || {};
(
    function () {
        this.formOnLoad = function (executionContext) {
            var formContext = executionContext.getFormContext();
            //var firstName = formContext.getAttribute("firstname").getValue();
            //alert("Hello World " + firstName);

            var lookupAccountArray = formContext.getAttribute("parentcustomerid").getValue();

            if (lookupAccountArray !== null && lookupAccountArray[0] !== null) {
                // you will get entity type, name and id
                var accountGuid = lookupAccountArray[0].id;
                var accountName = lookupAccountArray[0].name;
                var accountType = lookupAccountArray[0].entityType;

                formContext.ui.setFormNotification("Guid of the Account: " + accountGuid, "Info", "notification1");
                formContext.ui.setFormNotification("Name of the Account: " + accountName, "Info", "notification2");
                formContext.ui.setFormNotification("Type of the Account: " + accountType, "Info", "notification3");
            }
        };


        this.shippingMethodOnChange = function (executionContext) {
            var formContext = executionContext.getFormContext();
            if (formContext.getAttribute("address1_shippingmethodcode").getText() === "FedEx") {
                formContext.getControl("address1_freighttermscode").setDisabled(true);
            } else {
                formContext.getControl("address1_freighttermscode").setDisabled(false);
            }
        };
    }
).call(Sdk);

function RetrieveLookUpValue() {
    var lookupObj = Xrm.Page.getAttribute("xm_productname"); //Check for Lookup Object
    if (lookupObj !== null) {
        var lookupObjValue = lookupObj.getValue();//Check for Lookup Value
        if (lookupObjValue !== null) {
            console.log("lookupEntityType" + lookupObjValue[0].xm_productcoding);
            console.log("lookupRecordGuid" + lookupObjValue[0].xm_productmodel);
            console.log("lookupRecordName" + lookupObjValue[0].name);
        }
    }
}
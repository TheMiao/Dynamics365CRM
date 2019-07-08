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

    var firstName = formContext.getAttribute("firstname").getValue();

    alert("Hello World " + firstName);
}

// Converting functions to Namespace Notation
var Sdk = window.Sdk || {};
(
    function () {
        this.formOnLoad = function (executionContext) {
            var formContext = executionContext.getFormContext();
            var firstName = formContext.getAttribute("firstname").getValue();
            alert("Hello World " + firstName);
        };
    }
).call(Sdk);

function RetrieveLookUpValue() {
    var lookupObj = Xrm.Page.getAttribute("xm_productname"); //Check for Lookup Object
    if (lookupObj != null) {
        var lookupObjValue = lookupObj.getValue();//Check for Lookup Value
        if (lookupObjValue != null) {
            console.log("lookupEntityType" + lookupObjValue[0].xm_productcoding);
            console.log("lookupRecordGuid" + lookupObjValue[0].xm_productmodel);
            console.log("lookupRecordName" + lookupObjValue[0].name);
        }
    }
}
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

// Create an entity in CRM
function CreateEntity(clientURL, entityType, entityData) {
    var req = new XMLHttpRequest();
    req.open("POST", encodeURI(clientURL + "/api/data/v9.0/" + entityType), true);
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");

    req.onreadystatechange = function () {
        if (this.readyState === 4 /* complete */) {
            req.onreadystatechange = null; // avoids memory leaks
            if (this.status === 204) {
                var entityUri = this.getResponseHeader("ODta=EntityId");
                console.log("Created " + entityType + " with URI: " + entityUri);
            }
            else {
                var error = JSON.parse(this.response).error;
                console.log(error.message);
            }
        }
    };
    req.send(JSON.stringify(entityData));
}

// Basic Create an account entity using Web API
function BasicCreateAccount() {
    var entityType = "accounts";
    var clientURL = Xrm.Page.context.getClientUrl();

    var account = {};
    account["name"] = "Basic Create Sample Account";
    account["accountnumber"] = "xxxxx";
    account["fax"] = "xxxxx";

    // Set the exsiting contact id to the account primary contact id to associate the contact entity.
    account["xxxx@xxxx.com"] = "/contacts(GUID)";

    CreateEntity(clientURL, entityType, account);
}
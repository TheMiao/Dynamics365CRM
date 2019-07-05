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
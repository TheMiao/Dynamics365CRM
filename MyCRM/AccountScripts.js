// Converting functions to Namespace Notation
var Sdk = window.Sdk || {};
(
    function () {
        this.formOnLoad = function (executionContext) {
        };

        this.MailPhoneOnChange = function (executionContext) {
            var formContext = executionContext.getFormContext();
            var phoneNumber = formContext.getAttribute("telephone1").getValue();

            var expression = new RegExp("/^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$/im");
            if (!expression.test(phoneNumber)) {
                alert("The phone number entered should use correct format");
            }
        };
    }
).call(Sdk); 
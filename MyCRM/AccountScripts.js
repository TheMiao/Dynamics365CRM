// Converting functions to Namespace Notation
var Sdk = window.Sdk || {};
(
    function () {
        this.formOnLoad = function (executionContext) {
        };

        this.MailPhoneOnChange = function (executionContext) {
            var formContext = executionContext.getFormContext();
            var phoneNumber = formContext.getAttribute("telephone1").getValue();

            var expression = /^(\()?\d{3}(\))?(-|\s)?\d{3}(-|\s)\d{4}$/;
            if (!expression.test(phoneNumber)) {
                formContext.getControl("telephone1").setNotification("The phone number entered should use correct format","telephonemsg");
                formContext.ui.setFormNotification("Info message", "INFO", "formoti1");

            } else {
                formContext.getControl("telephone1").clearNotification("telephonemsg");
                formContext.ui.clearFormNotification("formoti1");
            }
        };
    }
).call(Sdk); 
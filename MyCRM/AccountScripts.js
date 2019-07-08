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
                // ��ʾfield �� form�� notification
                formContext.getControl("telephone1").setNotification("The phone number entered should use correct format", "telephonemsg");
                // Form message ���ݣ� ��ʾicon�� form message�� id
                formContext.ui.setFormNotification("Info message", "INFO", "formoti1");

            } else {
                // ����field �� from ��notification
                formContext.getControl("telephone1").clearNotification("telephonemsg");
                formContext.ui.clearFormNotification("formoti1");
            }
        };
    }
).call(Sdk); 
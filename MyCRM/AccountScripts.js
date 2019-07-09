// Converting functions to Namespace Notation
var Sdk = window.Sdk || {};
(
    function () {
        this.formOnLoad = function (executionContext) {
            var formContext = executionContext.getFormContext();

            var formType = formContext.ui.getFormType();

            if (formType == 1) {
                formContext.ui.setFormNotification("User is creating account ", "Info", "notification1");

            } else if (formType == 2) {
                formContext.ui.setFormNotification("User is opening account", "Info", "notification1");

            } else if (formType == 3) {
                formContext.ui.setFormNotification("User doesn't have permission to edit the record", "Info", "notification1");
            }
        };

        this.formOnSave = function (executionContext) {
            var eventArgs = executionContext.getEventArgs();
            if (eventArgs.getSaveMode() == 70 || eventArgs.getSaveMode() == 2) {
                eventArgs.preventDefault();
            }
        };


        this.MailPhoneOnChange = function (executionContext) {
            var formContext = executionContext.getFormContext();
            var phoneNumber = formContext.getAttribute("telephone1").getValue();

            var expression = /^(\()?\d{3}(\))?(-|\s)?\d{3}(-|\s)\d{4}$/;
            if (!expression.test(phoneNumber)) {
                // 显示field 和 form的 notification
                formContext.getControl("telephone1").setNotification("The phone number entered should use correct format", "telephonemsg");
                // Form message 内容， 显示icon， form message的 id
                formContext.ui.setFormNotification("Info message", "INFO", "formoti1");

            } else {
                // 清理field 和 from 的notification
                formContext.getControl("telephone1").clearNotification("telephonemsg");
                formContext.ui.clearFormNotification("formoti1");
            }
        };
    }
).call(Sdk); 
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
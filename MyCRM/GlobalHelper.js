// JavaScript source code
var Helper = window.Helper || {};
(
    function () {
        this.formOnLoad = function (executionContext) {
            var formContext = executionContext.getFormContext();
        };

        this.formOnSave = function (executionContext) {
            var formContext = executionContext.getFormContext();
        };

        this.DoSomething = function (executionContext) {
            alert("Hello World from GlobalHelper");
        };
    }
).call(Helper); 
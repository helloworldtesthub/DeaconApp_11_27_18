/* Reads values in hidden inputs on page to show a status notification
 to the user. For example, if an email was sent successfully, the
 user should get a notification pop up with that status.*/

$(function () {

    var hasStatusNotification = $("#hasStatusNotification").attr("value");

    var iconSuccess = "glyphicon glyphicon-thumbs-up";
    var iconFailure = "glyphicon glyphicon-thumbs-down";
    var iconInfo = "glyphicon glyphicon-info-sign";
    var icons = [iconSuccess, iconFailure, iconInfo];

    var typeSuccess = "success";
    var typeFailure = "error";
    var typeInfo = "info";
    var types = [typeSuccess, typeFailure, typeInfo];

    if (hasStatusNotification === "True") {
        var title = $("#statusNotificationTitle").attr("value");
        var message = $("#statusNotificationMessage").attr("value");
        var notifyTypeInt = $("#notifyTypeInt").attr("value");

        // notifyId: 2 === success; 3 === failure (corresponds with enum)
        var icon;
        var type;
        if (notifyTypeInt === '2') {
            icon = icons[0];
            type = types[0];
        }
        else if (notifyTypeInt === '3') {
            icon = icons[1];
            type = types[1];
        }
        else {
            icon = icons[2];
            type = types[2];
        }

        var url = "";
        var linkText = "";
        notifyId = ""; // no id since this is never stored

        var autoHideDelay = 15000;

        notifyJSService.showNotification(icon,
            type, title, message, url, linkText, notifyId, autoHideDelay);
    }
});
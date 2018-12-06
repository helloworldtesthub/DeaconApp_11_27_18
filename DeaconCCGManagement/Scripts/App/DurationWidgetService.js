/* Sets the options for the duration widget and sets hidden input field
 with duration value when the form is submitted */

$(document).ready(function () {
    var options = {
        hour: {
            value: 0,
            min: 0,
            max: 24,
            step: 1,
            symbol: "hrs"
        },
        minute: {
            value: 0,
            min: 0,
            max: 60,
            step: 1,
            symbol: "mins"
        },
        direction: "increment", // increment or decrement
        inputHourTextbox: null, // hour textbox
        inputMinuteTextbox: null, // minutes textbox
        postfixText: "", // text to display after the input fields
        numberPaddingChar: '0' // number left padding character ex: 00052
    };

    $(".durationWidget").timesetter(options).setMinute(15);

    // For edit view. If hidden input for hour and minute, set values.
    if ($('#hour').length) {
        var hour = $('#hour').val();        
        $(".durationWidget").timesetter(options).setHour(hour);
    } 
    if ($('#minutes').length) {
        var minute = $('#minutes').val();     
        $(".durationWidget").timesetter(options).setMinute(minute);
    } 

    $("#submit").on("click", function () {
        var totalMinutes = $(".durationWidget").timesetter().getTotalMinutes();
        $("#durationInput").val(totalMinutes);   
    });


});
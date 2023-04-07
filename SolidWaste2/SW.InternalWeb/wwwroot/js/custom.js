$(function () {
    $("body").on("click", "[data-confirm]", function (event) {
        if (!confirm($(this).attr("data-confirm"))) {
            return false;
        }
    });
    $("body").on("click", '[data-form-action]', function () {
        var formAction = $(this).attr('data-form-action');
        $(this).closest('form').attr('action', formAction);
    });
    //table = $('[data-table]').dataTable();
});

function setHeartbeat() {
    setTimeout("heartbeat()", 900000);
}

function heartbeat() {
    $.ajax({
        type: 'GET',
        dataType: 'json',
        url: 'https://sncowebdev/SOWA/Home/ResetSessionTimeout',
        data: null,
        success: setHeartbeat(),
        cache: false
    });
}

$(function () {
    setHeartbeat();
});

$(function () {
    $(':input:enabled:visible:not([readonly]):not(.datepicker-input):first').focus();
});

//jQuery(function ($) {
//    $("#date").mask("99/99/9999", { placeholder: "mm/dd/yyyy" });
//    $("#phone").mask("(999) 999-9999");
//    $("#tin").mask("99-9999999");
//    $("#ssn").mask("999-99-9999");
//});

$('#slimtest1').slimScroll({
    height: '200px'
});

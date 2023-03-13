$(function () {
    $('[data-confirm]').on('click', function (event) {
        if (!confirm($(this).attr("data-confirm"))) {
            return false;
        }
    });
    $('[data-form-action][type="submit"]').on('click', function () {
        var formAction = $(this).attr('data-form-action');
        $(this).closest('form').attr('action', formAction);
    });
    $('[redirect][type="button"]').on('click', function () {
        var redirect = $(this).attr('redirect');
        window.location.href = redirect;
    });
});

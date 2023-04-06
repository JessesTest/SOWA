function updateNotificationsList(data) {
    var list = $('#notify-list');
    list.empty();
    var notificationsLength = data.notifications.length;

    for (var i = 0; i < notificationsLength; i++) {
        var timeIndex = data.notifications[i].AddDateTime.indexOf('T');

        var anchor = $(document.createElement('a'));
        /*anchor.attr('href', '@(envUrl)' + 'Notify/Detail?notificationID=' + data.notifications[i].NotificationID);*/
        anchor.attr('href', getPath('~/Notify/Detail?notificationID=' + data.notifications[i].NotificationID));

        var rowDiv = $(document.createElement('div'));
        rowDiv.attr('class', 'row notify-element');

        if (data.notifications[i].Read) {
            rowDiv.addClass('notify-element-read');
        }
        else {
            rowDiv.addClass('notify-element-unread');
        }

        var colDiv = $(document.createElement('div'));
        colDiv.attr('class', 'col-md-8');

        var fromDiv = $(document.createElement('div'));
        fromDiv.attr('class', 'font-bold text-sm text-ellipsis');
        fromDiv.text(data.notifications[i].From)

        colDiv.append(fromDiv);
        rowDiv.append(colDiv);

        var colDiv = $(document.createElement('div'));
        colDiv.attr('class', 'col-md-4');

        var timeDiv = $(document.createElement('div'));
        timeDiv.attr('class', 'text-right text-sm');
        timeDiv.text(data.notifications[i].AddDateTime.substring(0, timeIndex));

        colDiv.append(timeDiv);
        rowDiv.append(colDiv);

        var colDiv = $(document.createElement('div'));
        colDiv.attr('class', 'col-md-12');

        var subjectDiv = $(document.createElement('div'));
        subjectDiv.attr('class', 'text-sm text-ellipsis');
        subjectDiv.text(data.notifications[i].Subject);

        colDiv.append(subjectDiv);
        rowDiv.append(colDiv);

        anchor.append(rowDiv);

        list.append(anchor);
    }

    var anchor = $(document.createElement('a'));
    /*anchor.attr('href', '@(envUrl)' + 'Notify/List');*/
    anchor.attr('href', getPath('~/Notify/List'));

    var rowDiv = $(document.createElement('div'));
    rowDiv.attr('class', 'row notify-element');
    rowDiv.attr('id', 'notify-footer');

    var colDiv = $(document.createElement('div'));
    colDiv.attr('class', 'col-md-12');

    var footerDiv = $(document.createElement('div'));
    footerDiv.attr('class', 'font-bold text-sm text-center');
    footerDiv.text('View All Notifications');

    colDiv.append(footerDiv);
    rowDiv.append(colDiv);
    anchor.append(rowDiv);

    list.append(anchor);
}

function updateNotificationsCount(data) {
    var icon = $('#notify-icon');
    var iconTxt = $('#notify-icon-txt');

    if (data.count == 0) {
        icon.removeClass('fa-color-gold');
        icon.addClass('fa-color-grey');
        iconTxt.removeClass();
        iconTxt.attr('hidden', true);
        iconTxt.text('');
    }
    else {
        icon.removeClass('fa-color-grey');
        icon.addClass('fa-color-gold');
        iconTxt.addClass('notify-icon-txt');
        iconTxt.attr('hidden', false);
        iconTxt.text(data.count);
    }
}

function ajaxNotificationsList($event) {
    var data;
    $.ajax({
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json',
        /*url: '@(envUrl + "api/NotifyAPI/NotificationsListJson")',*/
        url: getPath('~/api/NotifyAPI/NotificationsListJson'),
        data: JSON.stringify(data),
        cache: false,
        success: updateNotificationsList,
        error: function (jqXHR, textStatus, errorThrown) {
            console.log(textStatus, errorThrown);
        }
    });
}

function ajaxNotificationsCount($event) {
    var data;
    $.ajax({
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json',
        /*url: '@(envUrl + "api/NotifyAPI/NotificationsCountJson")',*/
        url: getPath('~/api/NotifyAPI/NotificationsCountJson'),
        data: JSON.stringify(data),
        cache: false,
        success: updateNotificationsCount,
        error: function (jqXHR, textStatus, errorThrown) {
            console.log(textStatus, errorThrown);
        }
    });
}

$('#notify-div').on('click', function () {
    $('#notify-list').show();
    ajaxNotificationsList();
    return false;
});

$(document).on('click', function () {
    $('#notify-list').hide();
});

$('#notify-list').on('click', function (e) {
    e.stopPropagation();
});

$(function () {
    ajaxNotificationsCount();
});

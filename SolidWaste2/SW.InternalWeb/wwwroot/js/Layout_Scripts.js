function updateNotificationsList(data) {
    let list = $('#notify-list');
    list.empty();
    let notificationsLength = data.notifications.length;
    
    for (let i = 0; i < notificationsLength; i++) {
        let timeIndex = data.notifications[i].addDateTime.indexOf('T');
        
        let anchor = $(document.createElement('a'));
        anchor.attr('href', getPath('~/Notify/Detail?notificationID=' + data.notifications[i].notificationID));

        let rowDiv = $(document.createElement('div'));
        rowDiv.attr('class', 'row notify-element');

        if (data.notifications[i].read) {
            rowDiv.addClass('notify-element-read');
        }
        else {
            rowDiv.addClass('notify-element-unread');
        }

        let colDiv = $(document.createElement('div'));
        colDiv.attr('class', 'col-md-8');

        let fromDiv = $(document.createElement('div'));
        fromDiv.attr('class', 'font-bold text-sm text-ellipsis');
        fromDiv.text(data.notifications[i].from)

        colDiv.append(fromDiv);
        rowDiv.append(colDiv);

        let colDiv2 = $(document.createElement('div'));
        colDiv2.attr('class', 'col-md-4');

        let timeDiv = $(document.createElement('div'));
        timeDiv.attr('class', 'text-right text-sm');
        timeDiv.text(data.notifications[i].addDateTime.substring(0, timeIndex));

        colDiv2.append(timeDiv);
        rowDiv.append(colDiv2);

        let colDiv3 = $(document.createElement('div'));
        colDiv3.attr('class', 'col-md-12');

        let subjectDiv = $(document.createElement('div'));
        subjectDiv.attr('class', 'text-sm text-ellipsis');
        subjectDiv.text(data.notifications[i].subject);

        colDiv3.append(subjectDiv);
        rowDiv.append(colDiv3);

        anchor.append(rowDiv);

        list.append(anchor);
    }

    let footerAnchor = $(document.createElement('a'));
    footerAnchor.attr('href', getPath('~/Notify/List'));

    let footerRowDiv = $(document.createElement('div'));
    footerRowDiv.attr('class', 'row notify-element');
    footerRowDiv.attr('id', 'notify-footer');

    let footerColDiv = $(document.createElement('div'));
    footerColDiv.attr('class', 'col-md-12');

    let footerDiv = $(document.createElement('div'));
    footerDiv.attr('class', 'font-bold text-sm text-center');
    footerDiv.text('View All Notifications');

    footerColDiv.append(footerDiv);
    footerRowDiv.append(footerColDiv);
    footerAnchor.append(footerRowDiv);

    list.append(footerAnchor);
}

function updateNotificationsCount(data) {
    let icon = $('#notify-icon');
    let iconTxt = $('#notify-icon-txt');

    if (data.count == 0) {
        icon.removeClass('fa-color-gold');
        icon.addClass('fa-color-grey');
        iconTxt.css('display', 'none');
        iconTxt.text('');
    }
    else {
        icon.removeClass('fa-color-grey');
        icon.addClass('fa-color-gold');
        iconTxt.css('display', 'block');
        iconTxt.text(data.count);
    }
}

function ajaxNotificationsList($event) {
    let data;
    $.ajax({
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json',
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
    let data;
    $.ajax({
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json',
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

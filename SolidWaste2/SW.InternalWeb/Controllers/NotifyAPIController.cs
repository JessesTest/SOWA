using Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Notify.BL.Services;
using SW.InternalWeb.Models.NotifyAPI;

namespace SW.InternalWeb.Controllers;

[ApiController]
[Route("api/NotifyAPI")]
public class NotifyApiController : ControllerBase
{
    private readonly INotifyService notifyService;

    public NotifyApiController(INotifyService notifyService)
    {
        this.notifyService = notifyService;
    }

    [Route("NotificationsCountJson")]
    public async Task<object> NotificationsCountJson()
    {
        var email = User.GetEmail();
        var count = await notifyService.GetUnreadCountByTo(email);
        return new { count };
    }

    [Route("NotificationsListJson")]
    public async Task<object> NotificationsListJson()
    {
        var email = User.GetEmail();
        var list = await notifyService.GetUnreadByTo(email, 5);
        return new { notifications = list };
    }

    [Route("NotificationsListAllJson")]
    public async Task<object> NotificationsListAllJson()
    {
        var email = User.GetEmail(); // ?? "DEANNA.STARKEBAUM@SNCO.US"
        var list = await notifyService.GetByTo(email);
        var value = list
            .Select(n => new ItemViewModel
            {
                AddDateTime = n.AddDateTime?.ToString("yyyy/MM/dd hh:mm:ss tt"),
                From = n.From,
                NotificationID = n.NotificationID.ToString(),
                Subject = n.Subject
            })
            .ToList();

        return new { Notifications = value };
    }

    [Route("GetGroupJson")]
    public object GetGroupJson()
    {
        string groupName = "role.admin";

        // not possible without graph api access...
        //try
        //{
        //    var dtos = await notifyService.GetApprovalUsers();  // role.admin
        //    return new
        //    {
        //        Members = dtos.Select(d => new
        //        {
        //            d.DisplayName,
        //            d.EmailAddress
        //        })
        //    };
        //}
        //catch(Exception e)
        //{
        //    return new { message = e.Message };
        //}

        return new 
        {
            Members = new[]
            {
                new{ DisplayName = "FINLEY, KATHRYN (6061)", EmailAddress = "KATHRYN.FINLEY@SNCO.US" },
                new{ DisplayName = "ORESTER, ANGIE (6062)", EmailAddress = "ANGIE.ORESTER@SNCO.US" },
                new{ DisplayName = "STARKEBAUM, DEANNA (6056)", EmailAddress = "DEANNA.STARKEBAUM@SNCO.US" },
            }
        };
    }
}

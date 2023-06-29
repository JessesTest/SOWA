using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using Notify.BL.Services;
using SW.InternalWeb.Models.Notify;

namespace SW.InternalWeb.Controllers;

public class NotifyController : Controller
{
    public readonly INotifyService notifyService;

    public NotifyController(INotifyService notifyService)
    {
        this.notifyService = notifyService;
    }

    public IActionResult List()
    {
        return View();
    }

    public async Task<IActionResult> Detail(int notificationId)
    {
        var notification = await notifyService.GetById(notificationId);
        if (notification == null)
            return RedirectToAction(nameof(List))
                .WithDanger("Invalid notification id", "");
        if (notification.Deleted)
            return RedirectToAction(nameof(List)).WithDanger("Notification id previously deleted", "");

        var email = User.GetEmail(); // ?? "DEANNA.STARKEBAUM@SNCO.US"
        if (email != notification.To)
            return RedirectToAction(nameof(List)).WithDanger("Notification does not belong to current user", "");

        await notifyService.ToggleReadById(notification.NotificationID);

        DetailViewModel model = new()
        {
            AddDateTime = notification.AddDateTime?.ToString("M/d/yyyy h:mm:ss tt"),
            Body = notification.Body,
            From = notification.From,
            NotificationID = notification.NotificationID.ToString(),
            Read = notification.Read.ToString(),
            Subject = notification.Subject,
            System = notification.System,
            To = notification.To,
            Url = notification.Url
        };
        return View(model);
    }
}

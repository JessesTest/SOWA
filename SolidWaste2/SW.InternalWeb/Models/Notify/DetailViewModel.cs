using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.Notify;

public class DetailViewModel
{
    [Display(Name = "Date")]
    public string AddDateTime { get; set; }

    public string Body { get; set; }

    public string From { get; set; }

    public string NotificationID { get; set; }

    public string Read { get; set; }

    public string Subject { get; set; }

    public string System { get; set; }

    public string To { get; set; }

    public string Url { get; set; }
}

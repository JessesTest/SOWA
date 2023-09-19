using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notify.DM;

public class Notification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int NotificationID { get; set; }

    [Required]
    [MaxLength(32)]
    public string System { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string From { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string To { get; set; }

    [MaxLength(256)]
    public string Subject { get; set; }

    [MaxLength(1024)]
    public string Body { get; set; }

    [Url]
    [MaxLength(256)]
    public string Url { get; set; }

    public bool Read { get; set; }

    public bool Deleted { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime? AddDateTime { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? ChgDateTime { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? DelDateTime { get; set; }
}

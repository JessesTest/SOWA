using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.CustomerServiceAddress;

public class ServiceAddressNoteViewModel
{
    public int Id { get; set; }

    [StringLength(1024)]
    public string Note { get; set; }

    public string AddDateTime { get; set; }

    public string AddToi { get; set; }
}

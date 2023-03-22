using PE.DM;
using SW.DM;

namespace SW.ExternalWeb.Models.Cancel;

public class CancelEmailViewModel
{
    public PersonEntity Person { get; set; }
    public Phone Phone { get; set; }
    public Email Email { get; set; }
    public Address Address { get; set; }
    public Customer Customer { get; set; }
    public ServiceAddress ServiceAddress { get; set; }
    public ICollection<Container> Containers { get; set; }
}

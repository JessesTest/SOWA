using Common.Services.AddressValidation;

namespace SW.InternalWeb.Models.CustomerServiceAddress;

public class ServiceAddressMasterViewModel
{
    public ICollection<ValidAddress> ServiceAddressList { get; set; }
    public int? ServiceAddressListIndex { get; set; }

    public ServiceAddressViewModel ServiceAddress { get; set; } = new();
    public int AddressIndex { get; set; }
    public int AddressCount { get; set; }

    public ContainerViewModel Container { get; set; } = new();
    public int ContainerIndex { get; set; }
    public int ContainerCount { get; set; }

    public ServiceAddressNoteViewModel Note { get; set; } = new();
    public int NoteIndex { get; set; }
    public int NoteCount { get; set; }

    public string FullName { get; set; }
    public DateTime? CustomerCancelDate { get; set; } 
}

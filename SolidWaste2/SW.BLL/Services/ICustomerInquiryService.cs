using PE.DM;
using SW.DM;

namespace SW.BLL.Services;

public interface ICustomerInquiryService
{
    Task<ICollection<CustomerInquiryResult>> Search(int? customerNumber,
            string customerName,
            string customerAddress,
            string locationName,
            string PIN,
            string locationAddress,
            bool include);
}

public class CustomerInquiryResult
{
    public Customer Customer { get; set; }
    public PersonEntity PersonEntity { get; set; }
    public Address BillingAddress { get; set; }
    public ICollection<ServiceAddress> ServiceAddresses { get; set; }
}

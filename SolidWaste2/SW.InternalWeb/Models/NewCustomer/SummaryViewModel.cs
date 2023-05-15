namespace SW.InternalWeb.Models.NewCustomer;

public class SummaryViewModel
{
    public CustomerViewModel Customer { get; set; }
    public BillingAddressViewModel BillingAddress { get; set; }
    public PhoneNumberViewModel Phone { get; set; }
    public EmailViewModel Email { get; set; }
    public ServiceAddressList ServiceAddresses { get; set; }

    public Dictionary<Guid, decimal> Container2Rate { get; set; }
    public string GetBillingAmount(ContainerViewModel container)
    {
        return Container2Rate[container.Id]
            .ToString("c");
    }
    public string GetBillingAmount(ServiceAddressViewModel address)
    {
        return address.Containers
            .Sum(c => Container2Rate[c.Id])
            .ToString("c");
    }
    public string GetBillingAmount()
    {
        if(decimal.TryParse(Customer.ContractCharge, out decimal contractCharage))
        {
            return contractCharage
                .ToString("c");
        }

        return Container2Rate
            .Sum(p => p.Value)
            .ToString("c");
    }
}

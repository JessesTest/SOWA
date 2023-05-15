namespace SW.InternalWeb.Models.NewCustomer;

public class ServiceAddressList : List<ServiceAddressViewModel>
{
    public bool IsValid
    {
        get
        {
            return Count > 0;
        }
    }
}

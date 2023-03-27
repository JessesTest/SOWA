namespace SW.ExternalWeb.Models.Manage
{
    public class AddressesViewModel
    {
        public List<AddressListViewModel> Addresses { get; set; }
    }

    public class AddressListViewModel
    {
        public int AddressID { get; set; }
        public bool IsDefault { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
    }
}

namespace SW.ExternalWeb.Models.Manage
{
    public class PhonesViewModel
    {
        public List<PhoneListViewModel> Phones { get; set; }
    }

    public class PhoneListViewModel
    {
        public int PhoneID { get; set; }
        public string TypeDescription { get; set; }
        public string Number { get; set; }
        public bool IsDefault { get; set; }
    }
}

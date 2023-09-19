namespace SW.ExternalWeb.Models.Manage
{
    public class EmailsViewModel
    {
        public List<EmailListViewModel> Emails { get; set; }
    }

    public class EmailListViewModel
    {
        public int EmailID { get; set; }
        public string TypeDescription { get; set; }
        public string EmailAddress { get; set; }
        public bool IsDefault { get; set; }
    }
}

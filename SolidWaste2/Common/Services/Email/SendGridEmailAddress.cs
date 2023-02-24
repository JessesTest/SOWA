namespace Common.Services.Email;

public class SendGridEmailAddress
{
    public SendGridEmailAddress() { }

    public SendGridEmailAddress(string address, string name = null)
    {
        Address = address;
        Name = name;
    }

    public string Address { get; set; }
    public string Name { get; set; }
}

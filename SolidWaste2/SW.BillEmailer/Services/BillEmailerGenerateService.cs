using Common.Services.Email;
using Common.Services.TelerikReporting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PE.DAL.Contexts;
using PE.DM;
using SW.DAL.Contexts;
using SW.DM;
using System.Text;

namespace SW.BillEmailer.Services;

/// <summary>
/// Runs every 1st of the month (6:30 AM) after SW.Billing and SW.BillGenerate and SW.BillBlobs
/// </summary>

public class BillEmailerGenerateService
{
    private readonly ISendGridService sendGridService;
    private readonly IConfiguration _configuration;

    private readonly IDbContextFactory<SwDbContext> swdbFactory;
    private readonly IDbContextFactory<PeDbContext> pedbFactory;

    private readonly IReportingService _reportingService;
    private readonly ReportingServiceOptions _options;

    private SwDbContext swdb;
    private PeDbContext pedb;

    private BillEmailerContext context;

    public BillEmailerGenerateService(IDbContextFactory<SwDbContext> swdbFactory, IDbContextFactory<PeDbContext> pedbFactory, IReportingService reportingService, IOptions<ReportingServiceOptions> options, ISendGridService sendGridService, IConfiguration configuration)
    {
        this.swdbFactory = swdbFactory;
        this.pedbFactory = pedbFactory;

        this._reportingService = reportingService;
        _options = options.Value;

        this.sendGridService = sendGridService;
        _configuration = configuration;
    }

    public async Task Handle(BillEmailerContext context)
    {
        using var swdbContext = swdbFactory.CreateDbContext();
        swdb = swdbContext;

        using var pedbContext = pedbFactory.CreateDbContext();
        pedb = pedbContext;

        this.context = context;

        await Process(swdbContext);
    }

    public async Task Process(SwDbContext swdbContext)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var account_login_external_url = _configuration[$"{env}-External-Domain"];

        Console.WriteLine("Getting list of Customers");

        List<Customer> customer_records = await swdb.GetAllCustomers();

        var customer_person_list = new List<CustomerPerson>();

        foreach (var customer in customer_records) 
        {
            var bill_blob = await swdb.GetBillBlobByAddDateTimeRange(customer.CustomerId, context.Process_Date_Beg, context.Process_Date_End);
        
            if (bill_blob != null) 
            {
                customer_person_list.Add(new CustomerPerson() { Customer = customer, BillBlob = bill_blob });
            }
        }

        Console.WriteLine("Joining Customers with PersonEntity data");
        
        foreach (var customer_person in customer_person_list)
        {
            var personEntity = await pedb.GetPersonEntityById(customer_person.Customer.Pe);

            if (personEntity == null)
                throw new InvalidOperationException($"Could not find PersonEntity record for Customer {customer_person.Customer.CustomerId}");

            customer_person.PersonEntity = personEntity;
            customer_person.PrimaryEmail = await pedb.Emails.SingleOrDefaultAsync(m => !m.Delete && m.PersonEntityID == customer_person.Customer.Pe && m.IsDefault);

            if (customer_person.PersonEntity.PaperLess == false || customer_person.PrimaryEmail == null) 
            {
                continue;
            }
            else 
            {
                SendEmailDto email = new()
                {
                    Subject = "Your Shawnee County Solid Waste eBill is ready",
                    HtmlContent = GenerateEmailBody(customer_person.Customer.CustomerId, account_login_external_url)
                };

                email.AddTo(customer_person.PrimaryEmail.Email1);
                // use default from

                var message = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")} : Sending email to {customer_person.PrimaryEmail.Email1} | ";

                Console.WriteLine(message);
                context.BillEmailerSummaryWriter.WriteLine(message);

                await sendGridService.SendSingleEmail(email);
            }
        }

        Console.WriteLine("Successful Job Completion");
    }

    private static string GenerateEmailBody(int customer_id, string url)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<div style=\"background-color:#909EB8\">");
        sb.AppendLine("     <div align=\"center\">");
        sb.AppendLine("         <div style=\"background-color:#344479; min-height:100px; padding-left: 20px; padding-right: 20px; padding-top: 28px; height: 100px; min-width: 100%; display:table; overflow:hidden;\">");
        sb.AppendLine("             <div style=\"display: table-cell; vertical-align: middle; font-size: 40px; color:white; font-family: Verdana;\">");
        sb.AppendLine("                 <div align=\"center\">");
        sb.AppendLine("                     Shawnee County Solid Waste");
        sb.AppendLine("                 </div>");
        sb.AppendLine("             </div>");
        sb.AppendLine("         </div>");
        sb.AppendLine("     </div>");
        sb.AppendLine("     <div align=\"center\">");
        sb.AppendLine("        <div style=\"padding: 20px 20px 20px 20px; background-color: #D4D4D4; width: 50%;\">");
        sb.AppendLine("            <div style=\"padding: 20px 20px 20px 20px; background-color: #FFFFFF;\">");
        //sb.AppendLine("                <div style=\"display: block; margin-bottom: 20px;\">");
        //sb.AppendLine("                    We apologize for the inconvenience; we mistakenly sent the previous email with an incorrect link to our web portal. The error has been resolved; your Shawnee County Solid Waste eBill is now available and ready to be viewed online. To view your eBill, please log in to your account by clicking the 'View eBill' link below. If the problem persists please call Shawnee County IT Support at (785) 251-4535.");
        //sb.AppendLine("                </div>");
        sb.AppendLine("                <div style=\"display: block; margin-bottom: 20px;\">");
        sb.AppendLine("                    Your Shawnee County Solid Waste eBill is now available and ready to be viewed online. Your eBill contains information that is pertinent to your account and should be viewed immediately. To view your eBill, please log in to your account");
        sb.AppendLine("                </div>");
        sb.AppendLine("                <div style=\"display: block; margin-bottom: 20px; text-align: center;\">");
        sb.AppendLine("                     <div align=\"center\">");
        sb.AppendLine($"                        <a style=\"padding: 8px 15px 8px 15px; text-align:center; background-color: #6EBF4A; color: #FFFFFF; font-weight: bold; text-decoration: none;\" href=\"{url}\">View eBill</a>");
        sb.AppendLine("                    </div>");
        sb.AppendLine("                 </div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("         <div align=\"center\" style=\"padding-top:20px\">");
        sb.AppendLine("             <div style=\"font-size:12px;\">");
        sb.AppendLine("                 DO NOT REPLY TO THIS EMAIL. If you've received this email in error, please notify us by telephone at 785.233.4774 or by email at solidwaste@snco.us.");
        sb.AppendLine("         </div>");
        sb.AppendLine("     </div>");
        sb.AppendLine("<hr>");
        sb.AppendLine("     <div width=\"50%\">");
        sb.AppendLine("         Visit the Shawnee County Website at http://www.snco.us");
        sb.AppendLine("     </div>");
        sb.AppendLine("     <div width=\"50%\">");
        sb.AppendLine("         Questions? Contact Solid Waste at 785.233.4774 (voice) 785.291.4928(fax)");
        sb.AppendLine("     </div>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");
        return sb.ToString();
    }
}

public class CustomerPerson
{
    public SW.DM.Customer Customer { get; set; }
    public PE.DM.PersonEntity PersonEntity { get; set; }
    public SW.DM.BillBlobs BillBlob { get; set; }
    public PE.DM.Email PrimaryEmail { get; set; }
}

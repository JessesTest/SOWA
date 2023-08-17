using System.Runtime.InteropServices;

namespace SW.Billing.Services;

public class BillingContext
{
    public DateTime Process_Date { get; init; }
    public DateTime Mthly_Bill_Beg_DateTime { get; init; }
    public DateTime Mthly_Bill_End_DateTime { get; init; }

    public string Mthly_Bill_Past_Due_Date { get; init; }
    public string Mthly_Bill_Curr_Due_Date { get; init; }

    public FileInfo BillingSummaryFile { get; init; } // SW_Billing_Summary_Rpt.txt
    public FileInfo BillingExceptionFile { get; init; } // SW_Billing_Error_Rpt.txt

    private TextWriter _billingSummaryWriter;
    public TextWriter BillingSummaryWriter => _billingSummaryWriter ??= BillingSummaryFile.AppendText();

    private TextWriter _billingExceptionWriter;
    public TextWriter BillingExceptionWriter => _billingExceptionWriter ??= BillingExceptionFile.AppendText();

    public BillingContext()
    {
        BillingSummaryFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
        BillingExceptionFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
    }
    public void CloseFiles()
    {
        var writers = new[] { _billingSummaryWriter, _billingExceptionWriter };
        foreach (var writer in writers)
        {
            if (writer != null)
            {
                writer.Flush();
                writer.Close();
            }
        }
        
        _billingSummaryWriter = null;
        _billingExceptionWriter = null;
    }
    public void DeleteFiles()
    {
        CloseFiles();

        var files = new[] { BillingSummaryFile, BillingExceptionFile };
        foreach (var file in files)
        {
            if (file.Exists)
            {
                file?.Delete();
            }
        }
    }
}

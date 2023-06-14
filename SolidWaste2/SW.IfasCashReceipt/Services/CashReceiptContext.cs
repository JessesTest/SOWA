namespace SW.IfasCashReceipt.Services;

public class CashReceiptContext
{

    public FileInfo ErrorFile { get; init; }  // Error.txt
    public FileInfo GoodFile { get; init; }   // Good.txt
    public FileInfo ReportFile { get; init; } // SW_Cash_Receipt_Rpt.txt
    public FileInfo ErrRptFile { get; init; } // SW_Cash_Receipt_Err_Rpt.txt // temp for exception handling

    public DateTime CashReceiptBeginDatetime { get; init; }
    public DateTime CashReceiptForDate { get; init; }

    public TextWriter ErrorWriter { get; init; }
    public TextWriter GoodWriter { get; init; }
    public TextWriter ReportWriter { get; init; }
    public TextWriter ExceptionWriter { get; set; }

    public CashReceiptContext()
    {
        ErrorFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
        GoodFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
        ReportFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
        ErrRptFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());

        ErrorWriter = ErrorFile.AppendText();
        ExceptionWriter = ErrRptFile.AppendText();
        GoodWriter = GoodFile.AppendText();
        ReportWriter = ReportFile.AppendText();
    }

    public void CloseFiles()
    {
        var writers = new[] { ErrorWriter, GoodWriter, ReportWriter, ExceptionWriter };
        foreach (var writer in writers)
        {
            if (writer != null)
            {
                writer.Flush();
                writer.Close();
            }
        }
    }
}

namespace SW.IfasCashReceipt.Services;

public class CashReceiptContext
{

    public FileInfo ErrorFile { get; init; }  // Error.txt
    public FileInfo GoodFile { get; init; }   // Good.txt
    public FileInfo ReportFile { get; init; } // SW_Cash_Receipt_Rpt.txt
    public FileInfo ExceptionFile { get; init; } // SW_Cash_Receipt_Err_Rpt.txt

    public int TotalPaymentsFound { get; set; }

    public DateTime CashReceiptBeginDatetime { get; init; }
    public DateTime CashReceiptForDate { get; init; }

    private TextWriter _errorWriter;
    public TextWriter ErrorWriter => _errorWriter ??= ErrorFile.AppendText();

    private TextWriter _goodWriter;
    public TextWriter GoodWriter => _goodWriter ??= GoodFile.AppendText();

    private TextWriter _reportWriter;
    public TextWriter ReportWriter => _reportWriter ??= ReportFile.AppendText();

    private TextWriter _exceptionWriter;
    public TextWriter ExceptionWriter => _exceptionWriter ??= ExceptionFile.AppendText();

    public CashReceiptContext()
    {
        GoodFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
        ErrorFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
        ReportFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
        ExceptionFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
    }
    public void CloseFiles()
    {
        var writers = new[] { _errorWriter, _goodWriter, _reportWriter, _exceptionWriter };
        foreach (var writer in writers)
        {
            if (writer != null)
            {
                writer.Flush();
                writer.Close();
            }
        }
        _errorWriter = null;
        _goodWriter = null;
        _reportWriter = null;
        _exceptionWriter = null;
    }
    public void DeleteFiles()
    {
        CloseFiles();

        var files = new[] { GoodFile, ErrorFile, ReportFile, ExceptionFile };
        foreach (var file in files)
        {
            if (file.Exists)
            {
                file.Delete();
            }
        }
    }
}

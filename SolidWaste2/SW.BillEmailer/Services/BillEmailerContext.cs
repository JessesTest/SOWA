using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW.BillEmailer.Services;

public class BillEmailerContext
{
    public DateTime Process_Date_Beg { get; init; }
    public DateTime Process_Date_End { get; init; }

    public FileInfo BillEmailerSummaryFile { get; init; } // SW_Bill_Emailer_Summary_Rpt.txt
    public FileInfo BillEmailerExceptionFile { get; init; } // SW_Bill_Emailer_Error_Rpt.txt

    private TextWriter _billEmailerSummaryWriter;
    public TextWriter BillEmailerSummaryWriter => _billEmailerSummaryWriter ??= BillEmailerSummaryFile.AppendText();

    private TextWriter _billEmailerExceptionWriter;
    public TextWriter BillEmailerExceptionWriter => _billEmailerExceptionWriter ??= BillEmailerExceptionFile.AppendText();

    public BillEmailerContext()
    {
        BillEmailerSummaryFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
        BillEmailerExceptionFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
    }
    public void CloseFiles()
    {
        var writers = new[] { _billEmailerSummaryWriter, _billEmailerExceptionWriter };
        foreach (var writer in writers)
        {
            if (writer != null)
            {
                writer.Flush();
                writer.Close();
            }
        }

        _billEmailerSummaryWriter = null;
        _billEmailerExceptionWriter = null;
    }
    public void DeleteFiles()
    {
        CloseFiles();

        var files = new[] { BillEmailerSummaryFile, BillEmailerExceptionFile };
        foreach (var file in files)
        {
            if (file.Exists)
            {
                file?.Delete();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW.BillBlobs.Services;

public class BillBlobContext
{
    public DateTime Process_Date_Beg { get; init; }
    public DateTime Process_Date_End { get; init; }

    public FileInfo BillBlobSummaryFile { get; init; } // SW_Bill_Blob_Summary_Rpt.txt
    public FileInfo BillBlobExceptionFile { get; init; } // SW_Bill_Blob_Error_Rpt.txt

    private TextWriter _billBlobSummaryWriter;
    public TextWriter BillBlobSummaryWriter => _billBlobSummaryWriter ??= BillBlobSummaryFile.AppendText();

    private TextWriter _billBlobExceptionWriter;
    public TextWriter BillBlobExceptionWriter => _billBlobExceptionWriter ??= BillBlobExceptionFile.AppendText();

    public DateTime Mthly_Bill_Blob_Beg_DateTime;
    public DateTime Mthly_Bill_Blob_End_DateTime;

    public BillBlobContext()
    {
        BillBlobSummaryFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
        BillBlobExceptionFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
    }
    public void CloseFiles()
    {
        var writers = new[] { _billBlobSummaryWriter, _billBlobExceptionWriter };
        foreach (var writer in writers)
        {
            if (writer != null)
            {
                writer.Flush();
                writer.Close();
            }
        }

        _billBlobSummaryWriter = null;
        _billBlobExceptionWriter = null;
    }
    public void DeleteFiles()
    {
        CloseFiles();

        var files = new[] { BillBlobSummaryFile, BillBlobExceptionFile };
        foreach (var file in files)
        {
            if (file.Exists)
            {
                file?.Delete();
            }
        }
    }
}

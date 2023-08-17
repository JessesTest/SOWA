using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SW.DAL.Contexts;

namespace SW.BillGenerate.Services;
public class BillGenerateContext
{
    public DateTime Process_Date { get; init; }

    public FileInfo BillGenerateBatchPdfFile { get; init; } //SW Bills Batch PDF
    public FileInfo BillGenerateExceptionFile { get; init; } //SW_Bill_Generate_Error_Rpt.txt

    //private TextWriter _billGenerateBatchPdfWriter;
    //public TextWriter BillGenerateBatchPdfWriter => _billGenerateBatchPdfWriter ??= BillGenerateBatchPdfFile.AppendText();

    private FileStream _billGenerateBatchPdfWriter;
    public FileStream BillGenerateBatchPdfWriter => _billGenerateBatchPdfWriter ??= BillGenerateBatchPdfFile.OpenWrite();

    private TextWriter _billGenerateExceptionWriter;
    public TextWriter BillGenerateExceptionWriter => _billGenerateExceptionWriter ??= BillGenerateExceptionFile.AppendText();

    public DateTime Mthly_Bill_Generate_Beg_DateTime;
    public DateTime Mthly_Bill_Generate_End_DateTime;

    public BillGenerateContext()
    {
        BillGenerateBatchPdfFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
        BillGenerateExceptionFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
    }

    public void CloseFiles()
    {
        //var writers = new[] { _billGenerateBatchPdfWriter, _billGenerateExceptionWriter };
        var writers = new[] { _billGenerateExceptionWriter };
        foreach (var writer in writers)
        {
            if (writer != null)
            {
                writer.Flush();
                writer.Close();
            }
        }

        //SOWA-117
        if (_billGenerateBatchPdfWriter != null) 
        {
            _billGenerateBatchPdfWriter.Close(); 
        }

        _billGenerateBatchPdfWriter = null;
        _billGenerateExceptionWriter = null;
    }

    public void DeleteFiles()
    {
        CloseFiles();

        var files = new[] { BillGenerateBatchPdfFile, BillGenerateExceptionFile };
        foreach (var file in files)
        {
            if (file.Exists)
            {
                file?.Delete();
            }
        }
    }
}
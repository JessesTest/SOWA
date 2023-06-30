using System.Runtime.InteropServices;

namespace SW.WeeklyRental.Services;

public class WeeklyRentalContext
{
    public FileInfo ExceptionFile { get; init; } // SW_Weekly_Rental_Err_Rpt.txt
    public FileInfo GoodFile { get; init; }     // Good.txt
    //public FileInfo ErrorFile { get; init; }    // Error.txt
    //public FileInfo ReportFile { get; init; }   // .txt

    private TextWriter _exceptionWriter;
    public TextWriter ExceptionWriter => _exceptionWriter ??= ExceptionFile.AppendText();

    //private TextWriter _errorWriter;
    //public TextWriter ErrorWriter => _errorWriter ??= ErrorFile.AppendText();

    private TextWriter _goodWriter;
    public TextWriter GoodWriter => _goodWriter ??= GoodFile.AppendText();

    //private TextWriter _reportWriter;
    //public TextWriter ReportWriter => _reportWriter ??= ReportFile.AppendText();


    /// <summary>
    /// current_date
    /// </summary>
    public DateTime CurrentTime { get; init; }
    /// <summary>
    /// current_date
    /// </summary>
    public DateTime CurrentDate { get; init; }
    /// <summary>
    /// mthly_bill_beg_datetime
    /// </summary>
    public DateTime BeginTime { get; init; }
    /// <summary>
    /// mthly_bill_end_datetime
    /// </summary>
    //public DateTime EndTime { get; init; }

    public int TransactionsCreated { get; set; }


    public WeeklyRentalContext()
    {
        ExceptionFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
        //ErrorFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
        GoodFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
        //ReportFile = new FileInfo(Path.GetTempPath() + Path.GetRandomFileName());
    }

    public void CloseFiles()
    {
        //var streams = new[] { _exceptionWriter, _errorWriter, _goodWriter, _reportWriter };
        var streams = new[] { _exceptionWriter, _goodWriter };
        foreach (var stream in streams)
        {
            if(stream != null)
            {
                stream.Flush();
                stream.Close();
            }
        }
        _exceptionWriter = null;
        //_errorWriter = null;
        _goodWriter = null;
        //_reportWriter = null;
    }

    public void DeleteFiles()
    {
        CloseFiles();

        //var files = new[] { ExceptionFile, ErrorFile, GoodFile, ReportFile };
        var files = new[] { ExceptionFile, GoodFile };
        foreach (var file in files)
        {
            file?.Delete();
        }
    }
}


using Common.Services.Blob;

namespace SW.WeeklyRental.Services;

public class WeeklyRentalBlobService
{
    private readonly IBlobStorageService storageService;
    private WeeklyRentalContext context;

    public WeeklyRentalBlobService(IBlobStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task Handle(WeeklyRentalContext context)
    {
        this.context = context;
        //...

        var list = await storageService.List();
        foreach(var item in list)
        {
            Console.WriteLine(item);
        }

        this.context = null;
    }

    private async Task SendFilesToStorage()
    {
        context.CloseFiles();
        await SendFileToStorage(context.GoodFile, "Good.txt", true);
        await SendFileToStorage(context.ExceptionFile, "SW_Weekly_Rental_Err_Rpt.txt", false);
    }

    private async Task SendFileToStorage(FileInfo file, string fileName, bool sendIfEmpty)
    {
        var time = context.BeginTime;
        var name = $"{time:yyyy}/{time:MM}/{time:dd}/{fileName}";
        if (file.Exists)
        {
            using var readStream = file.OpenRead();
            await storageService.Add(readStream, name, "text/plain");
        }
        else if (sendIfEmpty)
        {
            var bytes = new byte[0];
            using var stream = new MemoryStream(bytes);
            await storageService.Add(stream, name, "text/plain");
        }
    }
}

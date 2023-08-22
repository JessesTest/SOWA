using Common.Services.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW.BillBlobs.Services;

public class BillBlobStorageService
{
    private readonly IBlobStorageService storageService;
    private BillBlobContext context;

    public BillBlobStorageService(IBlobStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task Handle(BillBlobContext context)
    {
        this.context = context;
        //...

        var list = await storageService.List();
        foreach (var item in list)
        {
            Console.WriteLine(item);
        }

        this.context = null;
    }

    private async Task SendFilesToStorage()
    {
        context.CloseFiles();
        await SendFileToStorage(context.BillBlobSummaryFile, "SW_Bill_Blob_Summary_Rpt.txt", true);
        await SendFileToStorage(context.BillBlobExceptionFile, "SW_Bill_Blob_Error_Rpt.txt", false);
    }

    private async Task SendFileToStorage(FileInfo file, string fileName, bool sendIfEmpty)
    {
        var time = DateTime.Now;
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

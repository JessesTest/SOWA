using Common.Services.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW.Billing.Services;

public class BillingBlobService
{
    private readonly IBlobStorageService storageService;
    private BillingContext context;

    public BillingBlobService(IBlobStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task Handle(BillingContext context) 
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
        await SendFileToStorage(context.BillingSummaryFile, "SW_Billing_Summary_Rpt.txt", true);
        await SendFileToStorage(context.BillingExceptionFile, "SW_Billing_Error_Rpt.txt", false);
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

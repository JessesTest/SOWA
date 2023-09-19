using Common.Services.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW.BillGenerate.Services;

public class BillGenerateBlobService
{
    private readonly IBlobStorageService storageService;
    private BillGenerateContext context;

    public BillGenerateBlobService(IBlobStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task Handle(BillGenerateContext context)
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
        await SendFileToStorage(context.BillGenerateBatchPdfFile, "SW_Bills_" + DateTime.Now.ToString("yyyy-MM-dd") + ".pdf", true);
        await SendFileToStorage(context.BillGenerateExceptionFile, "SW_Bill_Generate_Error_Rpt.txt", false);
    }

    private async Task SendFileToStorage(FileInfo file, string fileName, bool sendIfEmpty)
    {
        var time = DateTime.Now;
        var name = $"{time:yyyy}/{time:MM}/{time:dd}/{fileName}";

        var content_type = "text/plain";
        if (fileName != "SW_Bill_Generate_Error_Rpt.txt")
        {
            content_type = "application/pdf";
        }

        if (file.Exists)
        {
            using var readStream = file.OpenRead();
            await storageService.Add(readStream, name, content_type);
        }
        else if (sendIfEmpty)
        {
            var bytes = new byte[0];
            using var stream = new MemoryStream(bytes);
            await storageService.Add(stream, name, content_type);
        }
    }
}

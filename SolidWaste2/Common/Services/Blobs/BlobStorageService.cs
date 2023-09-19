using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using System.Web;

namespace Common.Services.Blob;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient containerClient;
    private readonly string containerName;

    public BlobStorageService(IOptions<BlobStorageServiceSettings> options, BlobServiceClient serviceClient)
    {
        _ = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
        _ = options ?? throw new ArgumentNullException(nameof(options));
        var settings = options.Value ?? throw new ArgumentException("Invalid settings");

        if (string.IsNullOrWhiteSpace(settings.Folder) || settings.Folder == "/")
            throw new ArgumentException("Storage folder is required");

        containerName = settings.Folder;
        if(containerName.EndsWith('/'))
            containerName = containerName.Substring(0, containerName.Length - 1);

        containerClient = serviceClient.GetBlobContainerClient(settings.Container);
    }

    #region List

    public async Task<ICollection<string>> List(BlobContainerClient containerClient, CancellationToken cancellationToken = default)
    {
        _ = containerClient ?? throw new ArgumentNullException(nameof(containerClient));

        var value = new List<string>();
        var items = containerClient.GetBlobsAsync(prefix: containerName, cancellationToken: cancellationToken);
        await foreach (var item in items)
        {
            if (item.Deleted)
                continue;

            value.Add(item.Name.Substring(containerName.Length));
        }
        return value;
    }

    public Task<ICollection<string>> List()
    {
        return List(containerClient);
    }

    #endregion

    #region Get

    public async Task<BlobFile> Get(BlobContainerClient containerClient, string fileName, CancellationToken cancellationToken = default)
    {
        _ = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
        _ = fileName ?? throw new ArgumentNullException(nameof(fileName));
        if (fileName.Contains(".."))
            throw new ArgumentException("Invalid file name", nameof(fileName));

        var path = $"{containerName}/{fileName}";
        var blobClient = containerClient.GetBlobClient(path);
        var existsResult = await blobClient.ExistsAsync(cancellationToken);
        if (!existsResult.Value)
            return new BlobFile
            {
                ContentType = null,
                Data = null,
                Exists = false,
                Name = fileName,
                Uri = blobClient.Uri
            };

        var contentType = (await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken)).Value.ContentType;
        var response = await blobClient.DownloadContentAsync(cancellationToken);
        var content = response.Value.Content;
        var bytes = content.ToArray();

        return new BlobFile
        {
            ContentType = contentType,
            Data = bytes,
            Exists = true,
            Name = HttpUtility.UrlDecode(blobClient.Uri.Segments.Last()),
            Uri = blobClient.Uri
        };
    }

    public Task<BlobFile> Get(string fileName)
    {
        return Get(containerClient, fileName);
    }

    public Uri GetSasUri(string fileName)
    {
        return GetSasUri(containerClient, fileName, DateTime.UtcNow.AddMinutes(5));
    }

    public Uri GetSasUri(string fileName, DateTime expiresAt)
    {
        return GetSasUri(containerClient, fileName, expiresAt);
    }

    public Uri GetSasUri(BlobContainerClient containerClient, string fileName, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        _ = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
        _ = fileName ?? throw new ArgumentNullException(nameof(fileName));
        if (fileName.Contains(".."))
            throw new ArgumentException("Invalid file name", nameof(fileName));

        var path = $"{containerName}/{fileName}";
        var blobClient = containerClient.GetBlobClient(path);

        var sasBuilder = new BlobSasBuilder
        {
            ExpiresOn = expiresAt,
            ContentDisposition = "attachment"
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        return blobClient.GenerateSasUri(sasBuilder);
    }

    #endregion

    #region Add

    public async Task<Uri> Add(
        BlobContainerClient containerClient,
        Stream stream,
        string fileName,
        string fileType,
        IDictionary<string, string> metadata = null,
        CancellationToken cancellationToken = default)
    {
        _ = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
        _ = stream ?? throw new ArgumentNullException(nameof(stream));
        _ = fileName ?? throw new ArgumentNullException(nameof(fileName));
        _ = fileType ?? throw new ArgumentNullException(nameof(fileType));
        if (fileName.Contains(".."))
            throw new ArgumentException("Invalid file name", nameof(fileName));

        var headers = new BlobHttpHeaders { ContentType = fileType };
        var buo = new BlobUploadOptions { HttpHeaders = headers, Metadata = metadata };
        var path = $"{containerName}/{fileName}";
        var blobClient = containerClient.GetBlobClient(path);

        await blobClient.UploadAsync(stream, buo, cancellationToken);

        return blobClient.Uri;
    }

    public async Task<Uri> Add(Stream stream, string fileName, string fileType)
    {
        return await Add(containerClient, stream, fileName, fileType);
    }


    #endregion

    #region Delete

    public async Task Delete(BlobContainerClient containerClient, string fileName, CancellationToken cancellationToken = default)
    {
        _ = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
        _ = fileName ?? throw new ArgumentNullException(nameof(fileName));
        if (fileName.Contains(".."))
            throw new ArgumentException("Invalid file name", nameof(fileName));

        var path = $"{containerName}/{fileName}";
        await containerClient.DeleteBlobAsync(path, cancellationToken : cancellationToken);
    }

    public Task Delete(string fileName)
    {
        return Delete(containerClient, fileName);
    }

    #endregion
}

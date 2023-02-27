namespace Common.Services.Blob;

public interface IBlobStorageService
{
    Task<ICollection<string>> List();
    Task<Uri> Add(Stream stream, string fileName, string fileType);
    Task<BlobFile> Get(string fileName);
    Task Delete(string fileName);
    Uri GetSasUri(string fileName);
    Uri GetSasUri(string fileName, DateTime expiresAt);
}

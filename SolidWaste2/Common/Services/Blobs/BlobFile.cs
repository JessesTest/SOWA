namespace Common.Services.Blob;

public class BlobFile
{
    public Uri Uri { get; set; }
    public bool Exists { get; set; }
    public string Name { get; set; }
    public string ContentType { get; set; }
    public byte[] Data { get; set; }
}

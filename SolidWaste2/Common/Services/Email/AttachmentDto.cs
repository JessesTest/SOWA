namespace Common.Services.Email;

public class AttachmentDto
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    /// <summary>
    /// base64 encoded
    /// </summary>
    /// <see cref="Common.Extensions.ByteArrayExtensions.ToBase64String(byte[])"/>
    public string Content { get; set; }

    public bool DispositionInline { get; set; }
    public string ContentId { get; set; }
}

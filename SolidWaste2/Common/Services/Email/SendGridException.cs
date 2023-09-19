using System.Runtime.Serialization;

namespace Common.Services.Email;

[Serializable]
public class SendGridException : Exception
{
    public SendGridException() { }
    public SendGridException(string message) : base(message) { }
    public SendGridException(string message, Exception innerException) : base(message, innerException) { }
    protected SendGridException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
    }
}

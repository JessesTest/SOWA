using Twilio.Rest.Api.V2010.Account;

namespace Common.Services.Sms;

public interface ITwilioService
{
    Task<MessageResource> SendSmsAsync(string to, string message, string from = null, string callbackUrl = null);
}

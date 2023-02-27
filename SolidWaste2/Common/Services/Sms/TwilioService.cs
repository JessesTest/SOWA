using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Common.Services.Sms;

public class TwilioService : ITwilioService
{
    private readonly TwilioSettings _settings;

    public TwilioService(IOptions<TwilioSettings> twilioSettings)
    {
        _ = twilioSettings ?? throw new ArgumentNullException(nameof(twilioSettings));

        _settings = twilioSettings.Value;
        TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
    }

    public async Task<MessageResource> SendSmsAsync(string to, string message, string from = null, string callbackUrl = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(from))
            {
                from = _settings.From;
            }

            if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(message))
                return null;

            Uri statusCallback = null;
            if (!string.IsNullOrWhiteSpace(callbackUrl))
            {
                statusCallback = new Uri(callbackUrl);
            }
            else if (!string.IsNullOrWhiteSpace(_settings?.CallbackUrl))
            {
                statusCallback = new Uri(_settings.CallbackUrl);
            }

            return await MessageResource.CreateAsync(
                new PhoneNumber(to),
                from: new PhoneNumber(from),
                body: message,
                statusCallback: statusCallback);
        }
        catch (Exception e)
        {
            var ex = new ApplicationException("Error sending text", e);
            ex.Data.Add(nameof(to), to);
            ex.Data.Add(nameof(from), from);
            ex.Data.Add(nameof(message), message);
            ex.Data.Add(nameof(callbackUrl), callbackUrl);
            throw ex;
        }
    }
}

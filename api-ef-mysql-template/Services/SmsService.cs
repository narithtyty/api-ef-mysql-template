using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class SmsService
{
    private readonly string accountSid;
    private readonly string authToken;
    private readonly string twilioPhoneNumber;

    public SmsService(string accountSid, string authToken, string twilioPhoneNumber)
    {
        this.accountSid = accountSid;
        this.authToken = authToken;
        this.twilioPhoneNumber = twilioPhoneNumber;

        // Initialize Twilio
        TwilioClient.Init(accountSid, authToken);
    }

    public void SendSms(string toPhoneNumber, string message)
    {
        try
        {
            // Create a new message
            var twilioMessage = MessageResource.Create(
                body: message,
                from: new PhoneNumber(twilioPhoneNumber),
                to: new PhoneNumber(toPhoneNumber)
            );
        }
        catch (Exception ex)
        {
        }
    }
}
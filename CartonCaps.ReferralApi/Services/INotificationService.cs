namespace CartonCaps.ReferralApi.Services
{
	public interface INotificationService
	{
		bool SendSms(string phoneNumber, string message);
		bool SendEmail(string emailAddress, string subject, string body);
	}
}

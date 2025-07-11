namespace CartonCaps.ReferralApi.Services
{
	public class NotificationService : INotificationService
	{
		//Not adding logging here, because this is a mock service.
		public bool SendSms(string phoneNumber, string message)
		{
			Console.WriteLine($"[MOCK SMS] To: {phoneNumber} | Message: {message}");
			//The code to make the sms call would be here and will be inside a try catch
			// block to handle any exceptions that may occur during the API call. And if it fails it will return false.
			return true; // Simulate success
		}

		public bool SendEmail(string emailAddress, string subject, string body)
		{
			Console.WriteLine($"[MOCK EMAIL] To: {emailAddress} | Subject: {subject} | Body: {body}");
			return true; // Simulate success
		}
	}
}

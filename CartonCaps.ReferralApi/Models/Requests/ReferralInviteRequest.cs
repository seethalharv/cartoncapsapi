using System.ComponentModel.DataAnnotations;

namespace CartonCaps.ReferralApi.Models.Requests
{
	public class ReferralInviteRequest
	{
		[Required(ErrorMessage = "ReferrerUserId is required.")]
		[Range(1, int.MaxValue, ErrorMessage = "ReferrerUserId must be a positive number.")]
		public int ReferrerUserId { get; set; }

		[Required(ErrorMessage = "EmailOrPhone is required.")]
		public string EmailOrPhone { get; set; } = string.Empty;

		[Required(ErrorMessage = "Channel is required.")]
		[RegularExpression("^(sms|email)$", ErrorMessage = "Channel must be either 'sms' or 'email'.")]
		public string Channel { get; set; } = string.Empty;
	}
}

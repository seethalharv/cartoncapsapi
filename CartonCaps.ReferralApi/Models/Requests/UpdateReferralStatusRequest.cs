using System.ComponentModel.DataAnnotations;

namespace CartonCaps.ReferralApi.Models.Requests
{
	public class UpdateReferralStatusRequest
	{
		
		[Required(ErrorMessage = "ReferralCode is required.")]
		public string ReferralCode { get; set; }


		[Required(ErrorMessage = "RedeemingForUserId is required.")]
		[Range(1, int.MaxValue, ErrorMessage = "RedeemingForUserId must be a positive number.")]
		public int userId { get; set; }
	}
}

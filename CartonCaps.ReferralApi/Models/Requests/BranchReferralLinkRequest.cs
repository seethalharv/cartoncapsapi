namespace CartonCaps.ReferralApi.Models.Requests
{
	public class BranchReferralLinkRequest
	{
		public string branch_key { get; set; } = string.Empty;
		public string channel { get; set; } = "sms";
		public string feature { get; set; } = "referral";
		public string campaign { get; set; } = "referral_campaign";
		public Dictionary<string, object> data { get; set; } = new();
	}
}

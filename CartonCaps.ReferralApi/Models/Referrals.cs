namespace CartonCaps.ReferralApi.Models
{
	public class Referrals
	{
		public int Id { get; set; }
		public int ReferrerUserId { get; set; }
		public string ReferralCode { get; set; }
		public string? EmailOrPhone { get; set; }
		public DateTime ReferredDate { get; set; }
		public int ReferralStatusId { get; set; } 
		public int? ReferredToUser { get; set; } // Nullable to allow for cases where the referral is not yet accepted
}
}

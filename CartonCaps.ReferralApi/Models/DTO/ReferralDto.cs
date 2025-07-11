namespace CartonCaps.ReferralApi.Models.DTO
{
	/// <summary>
	/// This is DTO that is exposed to the outside world, it is used to transfer data between layers. Ideally we only want to 
	/// expose the necessary information to the client and not the entire model.
	/// </summary>
	public class ReferralDto
	{
		public string ReferralCode { get; set; }
		public string ReferredEmailOrPhone { get; set; }
		public string Status { get; set; }    // Status name only
		public DateTime CreatedAt { get; set; }
	}
}

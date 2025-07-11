namespace CartonCaps.ReferralApi.Models.Responses
{
	public class ReferralOperationResult
	{
		public bool Success { get; set; }
		public string? Message { get; set; }
		public int? ReferralId { get; set; }
	}
}

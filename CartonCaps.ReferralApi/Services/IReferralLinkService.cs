namespace CartonCaps.ReferralApi.Services
{
	public interface IReferralLinkService
	{
		Task<string> GenerateReferralLinkAsync(string referralCode, string channel);
	}
}

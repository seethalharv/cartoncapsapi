namespace CartonCaps.ReferralApi.Services
{
	public interface IUserService
	{
		Task<string> GetReferralCode(int userId);
	}
}

using CartonCaps.ReferralApi.Models;

namespace CartonCaps.ReferralApi.Repositories
{
	public interface IUserRepository
	{
		Task<string> GetReferralCodeByUserId(int userId);
		Task<UserReferralProfile?> GetUserByReferralCodeAsync(string referralCode);
		Task<User> GetUserById(int userId);
	}
}

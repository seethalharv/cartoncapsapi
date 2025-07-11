using CartonCaps.ReferralApi.Models.DTO;
using CartonCaps.ReferralApi.Models.Responses;

namespace CartonCaps.ReferralApi.Services
{
	public interface IReferralService
	{
		Task<List<ReferralDto>> GetUserReferralsAsync(int userId);
		Task<ReferralOperationResult> CreateReferralInvite(int referrerId, string emailOrPhone, string channel, string referralCode);
		Task<ReferralOperationResult> UpdateSuccessfulReferralToRedeemedAsync(int newlyAddedUserId, string referralCode);
		Task<string> GetReferralLinkAsync(int userId, string channel);
	}
}

using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Models.Responses;

namespace CartonCaps.ReferralApi.Repositories
{
	public interface IReferralRepository
	{
		Task<List<Referrals>> GetReferralsByUserIdAsync(int userId);
		Task<ReferralOperationResult> AddReferralInvite(Referrals referral);
		void UpdateReferralStatus(int referralId, int statusId);
		Task<ReferralOperationResult> UpdateSuccessfulReferralToRedeemedAsync(int newlyAddedUserId, string referralCode);
	}
}

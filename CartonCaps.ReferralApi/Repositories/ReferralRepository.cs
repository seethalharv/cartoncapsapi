using CartonCaps.ReferralApi.DB;
using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Models.Enums;
using CartonCaps.ReferralApi.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace CartonCaps.ReferralApi.Repositories
{
	public class ReferralRepository : IReferralRepository
	{
		private readonly AppDbContext _context;
		private readonly ILogger<ReferralRepository> _logger;

		public ReferralRepository(AppDbContext context, ILogger<ReferralRepository> logger)
		{
			_context = context;
			_logger = logger;
		}
		private readonly List<Referrals> _referrals = new();

		/// <summary>
		/// Retrieves a list of referrals associated with the specified user ID.
		/// </summary>
		/// <remarks>This method queries the database for referrals where the specified user is the referrer. If no
		/// referrals are found, a warning is logged and an empty list is returned.</remarks>
		/// <param name="userId">The unique identifier of the user whose referrals are to be retrieved.</param>
		/// <returns>A list of <see cref="Referrals"/> objects representing the referrals made by the specified user. Returns an
		/// empty list if no referrals are found.</returns>
		public async Task<List<Referrals>> GetReferralsByUserIdAsync(int userId)
		{
			var result = await _context.Referrals
				.Where(r => r.ReferrerUserId == userId)
				.ToListAsync();
			if (result == null || result.Count == 0)
			{
				_logger.LogWarning("No referrals found for user ID: {UserId}", userId);
				return new List<Referrals>();
			}
			return result;
		}

		/// <summary>
		/// Adds a new referral invite to the system.
		/// </summary>
		/// <remarks>This method assigns a unique ID to the referral and stores it in the system. The ID generation is
		/// simulated and may not reflect a production database implementation.</remarks>
		/// <param name="referral">The referral model containing the details of the invite to be added. This parameter must not be null and should
		/// include all required fields.</param>
		/// <returns>A <see cref="ReferralOperationResult"/> indicating the success or failure of the operation. If successful, the
		/// result includes the generated referral ID and a success message.</returns>
		public async Task<ReferralOperationResult> AddReferralInvite(Referrals referral)
		{
			try
			{
				// In real time this would be primary done by the database. Since we are not 
				//really using a database here, we will just simulate the ID generation.
				referral.Id = _referrals.Count + 1;
				_context.Referrals.Add(referral);
				// Simulate saving to the database , keeping this commented out since we are not using a real database.
				//await _context.SaveChangesAsync();
				return new ReferralOperationResult
				{
					Success = true,
					ReferralId = referral.Id,
					Message = "Referral successfully added."
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error adding referral invite for user ID: {UserId}", referral.ReferrerUserId);
				return new ReferralOperationResult
				{
					Success = false,
					ReferralId = null,
					Message = "An error occurred while adding the referral invite."
				};
			}
			
		}


		public void UpdateReferralStatus(int referralId, int statusId)
		{
			var referral = _context.Referrals.FirstOrDefault(r => r.Id == referralId);
			if (referral != null)
			{
				referral.ReferralStatusId = statusId;
			}
			//Not using real time database here so we will not save changes.
			//_context.SaveChanges();
			// Log the update operation, log if updated or failed to find the referral.
		}

		public async Task<ReferralOperationResult> UpdateSuccessfulReferralToRedeemedAsync(int newlyAddedUserId, string referralCode)
		{
			var referral = await _context.Referrals.FirstOrDefaultAsync(r => r.ReferralCode == referralCode && r.ReferredToUser == newlyAddedUserId);		    
			_logger.LogInformation("Updating referral with code {ReferralCode} for user ID {NewlyAddedUserId}", referralCode, newlyAddedUserId);

			if (referral == null)
				return new ReferralOperationResult
				{
					Success = false,
					ReferralId = null,
					Message = "Referral record not found for this user and code."
				};

			if (referral.ReferralStatusId == (int)ReferralStatus.Redeemed)
				return new ReferralOperationResult
				{
					Success = false,
					ReferralId = null,
					Message = "Referral already redeemed."
				};

			referral.ReferralStatusId = (int)ReferralStatus.Redeemed;
			// Update the referral status to redeemed, but commenting here to not actually save to a real database.
			//await _context.SaveChangesAsync();

			return new ReferralOperationResult
				{
					Success = true,
					ReferralId = referral.Id,
					Message = "Referral successfully redeemed and updated."
				};
		}

	}
	

	

}


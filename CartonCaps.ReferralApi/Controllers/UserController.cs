using CartonCaps.ReferralApi.Models.Requests;
using CartonCaps.ReferralApi.Repositories;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CartonCaps.ReferralApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserRepository _userRepository;
		private readonly IReferralRepository _referralRepository;
		private readonly ILogger<UserController> _logger;

		public UserController(IUserRepository userRepository, IReferralRepository referralRepository, ILogger<UserController> logger)
		{
			_userRepository = userRepository;
			_referralRepository = referralRepository;
			_logger = logger;
		}

		/// <summary>
		/// I would call this function after the a new user was created and the workflow for redemption was completed.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost("referral/update-redeemed")]
		public async Task<IActionResult> UpdateReferralToRedeemedAsync([FromBody] UpdateReferralStatusRequest request)
		{
			if (string.IsNullOrEmpty(request.ReferralCode))
				return BadRequest("Referral code is required.");

			var referrer = await _userRepository.GetUserByReferralCodeAsync(request.ReferralCode);
			if (referrer == null)
			{
				_logger.LogError("No user found with the given referral code: {ReferralCode}", request.ReferralCode);
				return NotFound("No user found with the given referral code.");
			}
			

			//Once a new user creates an account, we have their userId and they also have access to the referral code of the user who referred them.
			//and we assume they were passed in from UI.
			var updated = await _referralRepository.UpdateSuccessfulReferralToRedeemedAsync(request.userId, request.ReferralCode);
		    var message = $"Updated {updated} referral(s) to redeemed for your referred person who used the code. {referrer.ReferralCode}";
			_logger.LogInformation(message);
			return Ok(new
			{
				message = message
			});
		}

	}
}

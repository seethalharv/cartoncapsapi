using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Models.Requests;
using CartonCaps.ReferralApi.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CartonCaps.ReferralApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ReferralsController : ControllerBase
	{
		private readonly IReferralService _service;
		private readonly IUserService _userRepository;
		private readonly IReferralLinkService _referralLinkService;
		private readonly ILogger<ReferralsController> _logger;


		public ReferralsController(IReferralService referralService, IUserService userservice, IReferralLinkService referralLinkServie,
			  ILogger<ReferralsController> logger)
		{
			_service = referralService;
			_userRepository = userservice;
			_referralLinkService = referralLinkServie;
			_logger = logger;
		}

		/// <summary>
		/// This endpoint retrieves a list of referrals for a specific user. I am assuming one user has same referal code that they 
		/// can share with others and is unique to that user.
		/// </summary>
		/// <param name="userId"></param> // for testing use - userId = 100 in Swagger UI
		/// <returns></returns>
		[HttpPost("referalslist")]
		public async Task<IActionResult> GetReferrals([FromQuery] int userId)
		{
			// I would assume this validation was also done on the UI side but this is just to be sure.
			//If there was no userId , it does not even need to make this call. 
			if (userId <= 0)
			{
				return BadRequest("Invalid userId. It must be a positive integer.");
			}

			_logger.LogInformation("Fetching referrals for user ID: {UserId}", userId);
			var referrals = await _service.GetUserReferralsAsync(userId);

			if (referrals == null || referrals.Count == 0)
			{
				_logger.LogError($"No referrals found for user ID: {userId}");
				return NotFound($"No referrals found for user ID: {userId}");
			}
				

			return Ok(referrals);
		}

		/// <summary>
		/// Use , user Id , 100, and channel, "email" or "sms" to test this endpoint in Swagger UI.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>

		[HttpPost("invite")]
		public async Task<IActionResult> InviteFriend([FromBody] ReferralInviteRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			_logger.LogInformation("Processing referral invite for user ID: {UserId}, Email/Phone: {EmailOrPhone}, Channel: {Channel}",
				request.ReferrerUserId, request.EmailOrPhone, request.Channel);
			var referralCode = await _userRepository.GetReferralCode(request.ReferrerUserId);
			//The code to create invite URL and chose the specific notificaion channel (email or sms) is in the service layer.
			var result = await _service.CreateReferralInvite(request.ReferrerUserId, request.EmailOrPhone, request.Channel, referralCode);
			if (!result.Success)
			{
				_logger.LogError("An error occured while creating or inviting an referree " + result.Message);
				return StatusCode(500, result.Message);
			}
				


			return Ok(new { status =  $" invitation to {request.EmailOrPhone} was successfully sent."});
		}

		[HttpGet("get-referral-link")]
		public async Task<IActionResult> GetReferralLink([FromQuery] int userId, [FromQuery] string channel)
		{
			_logger.LogInformation("Retrieving referral link for user ID: {UserId}, Channel: {Channel}", userId, channel);
			if (userId <= 0 || string.IsNullOrWhiteSpace(channel))
				return BadRequest("UserId and channel are required.");

			try
			{
				var link = await _service.GetReferralLinkAsync(userId, channel);
				return Ok(new { referralLink = link });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving referral link for user {UserId}", userId);
				return StatusCode(500, "Failed to retrieve referral link.");
			}
		}
	}
}

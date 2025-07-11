using CartonCaps.ReferralApi.Repositories;

namespace CartonCaps.ReferralApi.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;
		private readonly ILogger<UserService> _logger;
		public UserService(IUserRepository userRepository, ILogger<UserService> logger)
		{
			_userRepository = userRepository;
			_logger = logger;
		}

		public Task<string> GetReferralCode(int userId)
		{
			_logger.LogInformation("Fetching referral code for user ID: {UserId}", userId);
			return _userRepository.GetReferralCodeByUserId(userId);
		}
	}
}

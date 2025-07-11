using CartonCaps.ReferralApi.Models.Requests;
using CartonCaps.ReferralApi.Models.Responses;

namespace CartonCaps.ReferralApi.Services
{	
	public class ReferralLinkService : IReferralLinkService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _config;
		private readonly ILogger<ReferralLinkService> _logger;
		public ReferralLinkService(HttpClient httpClient, IConfiguration config, ILogger<ReferralLinkService> logger)
		{
			_httpClient = httpClient;
			_config = config;
			_logger = logger;
		}

		public async Task<string> GenerateReferralLinkAsync(string referralCode, string channel)
		{
			_logger.LogInformation("Generating referral link for code: {ReferralCode} on channel: {Channel}", referralCode, channel);
			var branchKey = _config["Branch:Key"];
			bool useMock = bool.Parse(_config["Branch:UseMock"]);
			if (useMock)
			{
				await Task.Delay(50); // Just for fun, to get the expereince that its making a real call. 
				return $"https://cartoncaps.app.link/mock-{referralCode.ToLower()}-{channel}";
			}

			// I would have used Branch.Io for this but since it wont let me generate a link without adding my credit card
			//I am mocking this response for now. Also integrating and testing this would be extremely time consuming for this
			//effort , so I will just simulate the API call and response.
			var request = new BranchReferralLinkRequest
			{
				branch_key = branchKey,
				channel = channel,
				data = new Dictionary<string, object>
			{
				{ "$desktop_url", "https://cartoncaps.com" },
				{ "$ios_url", "cartoncaps://referral" },
				{ "$android_url", "cartoncaps://referral" },
				{ "referral_code", referralCode }
			}
			};

			_logger.LogInformation("Sending request to Branch API with data: {Data}", request.data);
			var response = await _httpClient.PostAsJsonAsync("https://api2.branch.io/v1/url", request);
			response.EnsureSuccessStatusCode();
			_logger.LogInformation("Received response from Branch API: {StatusCode}", response.StatusCode);

			var json = await response.Content.ReadFromJsonAsync<BranchLinkResponse>();
			return json?.url ?? throw new Exception("Failed to generate Branch link.");
		}
	}

}

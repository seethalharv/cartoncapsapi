using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CartonCaps.ReferralApi.Models
{
	public class User
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string EmailOrPhone { get; set; }
		public int Age { get; set; }
		public int AddressId { get; set; }
	}
}

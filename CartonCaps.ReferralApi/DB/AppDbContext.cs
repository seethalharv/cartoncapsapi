using CartonCaps.ReferralApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CartonCaps.ReferralApi.DB
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Referrals> Referrals { get; set; }
		public DbSet<UserReferralProfile> UserRefProfiles { get; set; }
		public DbSet<User> Users { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{			
			modelBuilder.Entity<Referrals>()
	       .Property(r => r.ReferralStatusId)
	       .HasConversion<int>();
		}
	}
}

namespace CartonCaps.ReferralApi.Repositories.Tests;
using CartonCaps.ReferralApi.DB;
using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Repositories;
using Microsoft.EntityFrameworkCore;

[TestClass]
public class UserRepositoryTests
{
	private AppDbContext _context;
	private UserRepository _repository;

	[TestInitialize]
	public void Setup()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;
		

		_context = new AppDbContext(options);

		_context.UserRefProfiles.AddRange(new List<UserReferralProfile>
		{
			new UserReferralProfile { Id = 1, ReferralCode = "REF101ABC" },
			new UserReferralProfile { Id = 2, ReferralCode = "REF102XYZ" }
		});

		_context.SaveChanges();

		_repository = new UserRepository(_context);
	}

	[TestCleanup]
	public void Cleanup()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();
	}

	[TestMethod]
	public async Task GetReferralCodeByUserId_ValidUserId_ReturnsCode()
	{
		var result = await _repository.GetReferralCodeByUserId(101);
		Assert.AreEqual("REF101ABC", result);
	}

	[TestMethod]
	[ExpectedException(typeof(System.Exception))]
	public async Task GetReferralCodeByUserId_InvalidUserId_Throws()
	{
		await _repository.GetReferralCodeByUserId(999);
	}

	[TestMethod]
	public async Task GetUserByReferralCodeAsync_ValidCode_ReturnsUser()
	{
		var result = await _repository.GetUserByReferralCodeAsync("ref101abc");
		Assert.IsNotNull(result);
		Assert.AreEqual("REF101ABC", result.ReferralCode);
	}

	[TestMethod]
	public async Task GetUserByReferralCodeAsync_InvalidCode_ReturnsNull()
	{
		var result = await _repository.GetUserByReferralCodeAsync("invalid");
		Assert.IsNull(result);
	}
}

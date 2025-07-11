namespace CartonCaps.ReferralApi.Repositories.Tests;

using CartonCaps.ReferralApi.DB;
using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[TestClass]
public class ReferralRepositoryTests
{
	private Mock<AppDbContext> _mockContext;
	private Mock<ILogger<ReferralRepository>> _mockLogger;
	private ReferralRepository _repository;

	[TestInitialize]
	public void Setup()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: "ReferralTestDb")
			.Options;

		var context = new AppDbContext(options);
		_mockContext = new Mock<AppDbContext>(options);
		_mockLogger = new Mock<ILogger<ReferralRepository>>();
		_repository = new ReferralRepository(context, _mockLogger.Object);

	}

	[TestMethod]
	public async Task GetReferralsByUserIdAsync_ReturnsList_WhenReferralsExist()
	{
		// Arrange
		var context = GetInMemoryContextWithData();
		var repository = new ReferralRepository(context, _mockLogger.Object);

		// Act
		var result = await repository.GetReferralsByUserIdAsync(1);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count);
	}

	[TestMethod]
	public async Task GetReferralsByUserIdAsync_ReturnsEmptyList_WhenNoReferralsFound()
	{
		// Arrange
		var context = GetInMemoryContextWithData();
		var repository = new ReferralRepository(context, _mockLogger.Object);

		// Act
		var result = await repository.GetReferralsByUserIdAsync(999); // No match

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Count);
		_mockLogger.Verify(l => l.Log(
			LogLevel.Warning,
			It.IsAny<EventId>(),
			It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No referrals found")),
			null,
			It.IsAny<Func<It.IsAnyType, Exception, string>>()),
			Times.Once);
	}

	[TestMethod]
	public async Task AddReferralInvite_ReturnsSuccess_WhenValidReferralProvided()
	{
		var context = GetInMemoryContextWithData();
		// Arrange
		var newReferral = context.Referrals.FirstOrDefault();

		// Act
		var result = await _repository.AddReferralInvite(newReferral);

		// Assert
		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.ReferralId);
		Assert.AreEqual("Referral successfully added.", result.Message);
	}

	[TestMethod]
	public async Task AddReferralInvite_ReturnsFailure_WhenContextIsNull()
	{
		// Arrange
		var brokenRepository = new ReferralRepository(null!, _mockLogger.Object);
		var referral = new Referrals
		{
			ReferrerUserId = 2,
			EmailOrPhone = "fail@example.com",
			ReferralCode = "FAIL123",
			ReferralStatusId = 1
		};

		// Act
		var result = await brokenRepository.AddReferralInvite(referral);

		// Assert
		Assert.IsFalse(result.Success);
		Assert.IsNull(result.ReferralId);
		_mockLogger.Verify(
			l => l.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error adding referral invite")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()
			),
			Times.Once);
	}

	private AppDbContext GetInMemoryContextWithData()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
			.Options;

		var context = new AppDbContext(options);

		context.Referrals.AddRange(new List<Referrals>
		{
			new Referrals { Id = 1, ReferrerUserId = 1, ReferralCode = "100-ABC" },
			new Referrals { Id = 2, ReferrerUserId = 1, ReferralCode = "200-TYU" },
			new Referrals { Id = 3, ReferrerUserId = 2, ReferralCode = "300-XYZ"},
		});

		context.SaveChanges();
		return context;
	}
	[TestMethod]
	public void UpdateReferralStatus_UpdatesStatus_WhenReferralExists()
	{
		var _context = GetInMemoryContextWithData();
		// Arrange
		var referral = new Referrals
		{
			Id = 4,
			ReferrerUserId = 4,
			ReferralCode = "ABC123",
			ReferralStatusId = 1
		};
		_context.Referrals.Add(referral);
		_context.SaveChanges();
		var _goodrepository = new ReferralRepository(_context, _mockLogger.Object);
		// Act
		_goodrepository.UpdateReferralStatus(1, 2);

		// Assert
		var updated = _context.Referrals.FirstOrDefault(r => r.Id == 1);
		Assert.IsNotNull(updated);
		Assert.AreEqual(2, updated.ReferralStatusId);
	}

	[TestMethod]
	public void UpdateReferralStatus_DoesNothing_WhenReferralDoesNotExist()
	{
		var _context = GetInMemoryContextWithData();
		var _goodrepository = new ReferralRepository(_context, _mockLogger.Object);
		_goodrepository.UpdateReferralStatus(999, 3); // non-existent ID

		// Assert
		Assert.AreEqual(3, _context.Referrals.Count()); // still empty
	}

}

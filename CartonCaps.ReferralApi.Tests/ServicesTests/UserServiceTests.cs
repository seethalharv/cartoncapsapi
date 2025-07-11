using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using CartonCaps.ReferralApi.Repositories;
using CartonCaps.ReferralApi.Services;
using Microsoft.Extensions.Logging;

namespace CartonCaps.ReferralApi.Services.Tests

{
	[TestClass]
	public class UserServiceTests
	{
		private Mock<IUserRepository> _userRepositoryMock;
		private UserService _userService;
		private Mock<ILogger<UserService>> _loggerMock;

		[TestInitialize]
		public void Setup()
		{
			_userRepositoryMock = new Mock<IUserRepository>();
			_loggerMock = new Mock<ILogger<UserService>>();
			_userService = new UserService(_userRepositoryMock.Object, _loggerMock.Object);
		}

		[TestMethod]
		public async Task GetReferralCode_ValidUserId_ReturnsCode()
		{
			// Arrange
			int userId = 101;
			string expectedCode = "REF101ABC";
			_userRepositoryMock.Setup(r => r.GetReferralCodeByUserId(userId))
							   .ReturnsAsync(expectedCode);

			// Act
			var result = await _userService.GetReferralCode(userId);

			// Assert
			Assert.AreEqual(expectedCode, result);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception))]
		public async Task GetReferralCode_InvalidUserId_ThrowsException()
		{
			// Arrange
			int invalidUserId = 999;
			_userRepositoryMock.Setup(r => r.GetReferralCodeByUserId(invalidUserId))
							   .ThrowsAsync(new Exception("Referral code not found"));

			// Act
			await _userService.GetReferralCode(invalidUserId);
		}
	}
}

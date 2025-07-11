using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CartonCaps.ReferralApi.Controllers;
using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Models.Responses;
using CartonCaps.ReferralApi.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace CartonCaps.ReferralApi.Services.Tests
{
	[TestClass]
	public class ReferralServiceTests
	{
		private Mock<IReferralRepository> _referralRepoMock;
		private Mock<IUserRepository> _userRepoMock;
		private Mock<IReferralLinkService> _linkServiceMock;
		private Mock<INotificationService> _notificationServiceMock;
		private ReferralService _service;
		private Mock<ILogger<ReferralService>> _mockLogger;


		[TestInitialize]
		public void Setup()
		{
			_referralRepoMock = new Mock<IReferralRepository>();
			_userRepoMock = new Mock<IUserRepository>();
			_linkServiceMock = new Mock<IReferralLinkService>();
			_notificationServiceMock = new Mock<INotificationService>();
			_mockLogger = new Mock<ILogger<ReferralService>>();


			_service = new ReferralService(
				_referralRepoMock.Object,
				_userRepoMock.Object,
				_linkServiceMock.Object,
				_notificationServiceMock.Object,
				_mockLogger.Object);
		}


		[TestMethod]
		public async Task GetUserReferralsAsync_ReturnsMappedDtos()
		{
			var referrals = new List<Referrals>
	    {
		new Referrals { ReferralCode = "ABC123", EmailOrPhone = "user@example.com", ReferredDate = DateTime.UtcNow, ReferralStatusId = 1 }
	    };

			_referralRepoMock.Setup(r => r.GetReferralsByUserIdAsync(1)).ReturnsAsync(referrals);

			var result = await _service.GetUserReferralsAsync(1);

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("ABC123", result[0].ReferralCode);
			Assert.AreEqual("Pending", result[0].Status); 
		}

		[TestMethod]
		public async Task UpdateSuccessfulReferralToRedeemedAsync_Success_ReturnsResult()
		{
			_referralRepoMock.Setup(r => r.UpdateSuccessfulReferralToRedeemedAsync(2, "REF123"))
				.ReturnsAsync(new ReferralOperationResult { Success = true });

			var result = await _service.UpdateSuccessfulReferralToRedeemedAsync(2, "REF123");

			Assert.IsTrue(result.Success);
		}

		[TestMethod]
		public async Task UpdateSuccessfulReferralToRedeemedAsync_Exception_ReturnsFailure()
		{
			_referralRepoMock.Setup(r => r.UpdateSuccessfulReferralToRedeemedAsync(It.IsAny<int>(), It.IsAny<string>()))
				.ThrowsAsync(new Exception("DB failure"));

			var result = await _service.UpdateSuccessfulReferralToRedeemedAsync(2, "REF123");

			Assert.IsFalse(result.Success);
			Assert.IsTrue(result.Message.Contains("DB failure"));
		}

		[TestMethod]
		public async Task CreateReferralInvite_SmsChannel_SuccessfulFlow()
		{
			_referralRepoMock.Setup(r => r.AddReferralInvite(It.IsAny<Referrals>()))
				.ReturnsAsync(new ReferralOperationResult { Success = true, ReferralId = 10 });

			_linkServiceMock.Setup(l => l.GenerateReferralLinkAsync("REF123", "sms"))
				.ReturnsAsync("https://mock.link");

			_notificationServiceMock.Setup(n => n.SendSms("1234567890", It.IsAny<string>())).Returns(true);
			_userRepoMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(new User { Id = 1, EmailOrPhone = 
				"test@gmail.com" });


			var result = await _service.CreateReferralInvite(1, "1234567890", "sms", "REF123");

			Assert.IsTrue(result.Success);
			Assert.AreEqual(10, result.ReferralId);
		}
		[TestMethod]
		public async Task CreateReferralInvite_NotificationFails_ReturnsFailureAndUpdatesStatus()
		{
			_referralRepoMock.Setup(r => r.AddReferralInvite(It.IsAny<Referrals>()))
				.ReturnsAsync(new ReferralOperationResult { Success = true, ReferralId = 99 });

			_linkServiceMock.Setup(l => l.GenerateReferralLinkAsync("REF123", "email"))
				.ReturnsAsync("https://mock.link");

			_notificationServiceMock.Setup(n => n.SendEmail("someone@example.com", It.IsAny<string>(), It.IsAny<string>()))
				.Returns(false);
			_userRepoMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(new User { Id = 1, EmailOrPhone = "test@gmail.com" });

			ReferralOperationResult result = await _service.CreateReferralInvite(1, "someone@example.com", "email", "REF123");

			Assert.IsFalse(result.Success);
			Assert.AreEqual("Referral saved, but notification failed.", result.Message);
		}

		[TestMethod]
		public async Task CreateReferralInvite_WhenExceptionThrown_ReturnsFailure()
		{
			_referralRepoMock.Setup(r => r.AddReferralInvite(It.IsAny<Referrals>()))
				.Throws(new Exception("Simulated exception"));
			_userRepoMock.Setup(u => u.GetUserById(It.IsAny<int>())).ReturnsAsync(new User { Id = 1,
				EmailOrPhone = "test@gmail.com" });


			var result = await _service.CreateReferralInvite(1, "email@example.com", "email", "REF123");

			Assert.IsFalse(result.Success);
			Assert.IsTrue(result.Message.Contains("Simulated exception"));
		}

		[TestMethod]
		public async Task CanSendReferralInviteAsync_Should_WhenInviteLimitNotExceeded()
		{
			// Arrange
			int userId = 1;
			string contact = "test@example.com";

			var timestamps = new List<DateTime>();
			for (int i = 0; i < 3; i++)
				timestamps.Add(DateTime.UtcNow.AddMinutes(-5));

			var field = typeof(ReferralService).GetField("_inviteTimestamps", BindingFlags.Static | BindingFlags.NonPublic);

			// Remove the readonly flag (I am not proud of this)
			var isInitOnly = typeof(FieldInfo).GetProperty("IsInitOnly", BindingFlags.NonPublic | BindingFlags.Instance);
			var attributesField = typeof(FieldInfo).GetField("m_fieldAttributes", BindingFlags.NonPublic | BindingFlags.Instance);
			if (attributesField != null && field != null)
			{
				var attrs = (FieldAttributes)attributesField.GetValue(field);
				attrs &= ~FieldAttributes.InitOnly;
				attributesField.SetValue(field, attrs);

				field.SetValue(null, timestamps);
			}

			_userRepoMock.Setup(x => x.GetUserById(userId))
						 .ReturnsAsync(new User { EmailOrPhone = "other@example.com" });

			// Act
		
			var result = await _service.CanSendReferralInviteAsync(userId, contact);

			// Assert
			Assert.IsTrue(result.flowControl);
		}

		[TestMethod]
		public async Task CanSendReferralInviteAsync_ShouldFail_WhenSelfReferral()
		{
			// Arrange
			int userId = 2;
			string contact = "self@example.com";

			_userRepoMock.Setup(x => x.GetUserById(userId))
						 .ReturnsAsync(new User { EmailOrPhone = "self@example.com" });

			// Act
			var result = await _service.CanSendReferralInviteAsync(userId, contact);

			// Assert
			Assert.IsFalse(result.flowControl);
			Assert.AreEqual("Cannot invite yourself.", result.value.Message);
		}

		[TestMethod]
		public async Task CanSendReferralInviteAsync_ShouldSucceed_WhenValid()
		{
			// Arrange
			int userId = 1;
			string contact = "other@example.com";

			_userRepoMock.Setup(x => x.GetUserById(userId))
						 .ReturnsAsync(new User { EmailOrPhone = "referrer@example.com" });

			// Act
			var result = await _service.CanSendReferralInviteAsync(userId, contact);

			// Assert
			Assert.IsTrue(result.flowControl);
			Assert.IsNull(result.value);
		}

		[TestMethod]
		public async Task GetReferralLinkAsync_ReturnsLink_WhenDataIsValid()
		{
			// Arrange
			int userId = 100;
			string channel = "email";
			string referralCode = "REF123";
			string expectedLink = "https://mocked.link";

			_userRepoMock.Setup(x => x.GetReferralCodeByUserId(userId))
				.ReturnsAsync(referralCode);

			_linkServiceMock.Setup(x => x.GenerateReferralLinkAsync(referralCode, channel))
				.ReturnsAsync(expectedLink);

			// Act
			var result = await _service.GetReferralLinkAsync(userId, channel);

			// Assert
			Assert.AreEqual(expectedLink, result);
		}

		[TestMethod]
		[ExpectedException(typeof(ApplicationException))]
		public async Task GetReferralLinkAsync_Throws_WhenUserRepositoryFails()
		{
			// Arrange
			int userId = 101;
			string channel = "sms";

			_userRepoMock.Setup(x => x.GetReferralCodeByUserId(userId))
				.ThrowsAsync(new Exception("DB failure"));

			// Act
			await _service.GetReferralLinkAsync(userId, channel);
		}

		[TestMethod]
		[ExpectedException(typeof(ApplicationException))]
		public async Task GetReferralLinkAsync_Throws_WhenLinkServiceFails()
		{
			// Arrange
			int userId = 102;
			string channel = "email";
			string referralCode = "REF456";

			_userRepoMock.Setup(x => x.GetReferralCodeByUserId(userId))
				.ReturnsAsync(referralCode);

			_linkServiceMock.Setup(x => x.GenerateReferralLinkAsync(referralCode, channel))
				.ThrowsAsync(new Exception("Branch API down"));

			// Act
			await _service.GetReferralLinkAsync(userId, channel);
		}

	}
}

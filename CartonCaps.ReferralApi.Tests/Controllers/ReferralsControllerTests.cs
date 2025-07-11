using CartonCaps.ReferralApi.Controllers;
using CartonCaps.ReferralApi.Models.DTO;
using CartonCaps.ReferralApi.Models.Requests;
using CartonCaps.ReferralApi.Models.Responses;
using CartonCaps.ReferralApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartonCaps.ReferralApi.Controllers.Tests
{
	[TestClass]
	public class ReferralsControllerTests
	{
		private Mock<IReferralService> _referralServiceMock;
		private Mock<IUserService> _userServiceMock;
		private Mock<IReferralLinkService> _linkServiceMock;
		private Mock<ILogger<ReferralsController>> _loggerMock;
		private ReferralsController _controller;

		[TestInitialize]
		public void Setup()
		{
			_referralServiceMock = new Mock<IReferralService>();
			_userServiceMock = new Mock<IUserService>();
			_linkServiceMock = new Mock<IReferralLinkService>();
			_loggerMock = new Mock<ILogger<ReferralsController>>();

			_controller = new ReferralsController(
				_referralServiceMock.Object,
				_userServiceMock.Object,
				_linkServiceMock.Object,
				_loggerMock.Object);
		}

		[TestMethod]
		public async Task GetReferrals_InvalidUserId_ReturnsBadRequest()
		{
			var result = await _controller.GetReferrals(0);
			Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
		}

		[TestMethod]
		public async Task GetReferrals_NoReferrals_ReturnsNotFound()
		{
			_referralServiceMock.Setup(s => s.GetUserReferralsAsync(100)).ReturnsAsync(new List<ReferralDto>());

			var result = await _controller.GetReferrals(100);
			Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
		}

		[TestMethod]
		public async Task GetReferrals_ReturnsOkWithData()
		{
			_referralServiceMock.Setup(s => s.GetUserReferralsAsync(100)).ReturnsAsync(new List<ReferralDto>
			{
				new ReferralDto { ReferralCode = "REF100", Status = "Invited" }
			});

			var result = await _controller.GetReferrals(100) as OkObjectResult;

			Assert.IsNotNull(result);
			Assert.AreEqual(200, result.StatusCode);
		}

		[TestMethod]
		public async Task InviteFriend_InvalidModel_ReturnsBadRequest()
		{
			_controller.ModelState.AddModelError("EmailOrPhone", "Required");

			var request = new ReferralInviteRequest();
			var result = await _controller.InviteFriend(request);

			Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
		}

		[TestMethod]
		public async Task InviteFriend_FailureFromService_Returns500()
		{
			_userServiceMock.Setup(s => s.GetReferralCode(100)).ReturnsAsync("REF100");
			_referralServiceMock.Setup(s => s.CreateReferralInvite(100, "abc@xyz.com", "email", "REF100"))
				.ReturnsAsync(new ReferralOperationResult { Success = false, Message = "Simulated failure" });

			var request = new ReferralInviteRequest
			{
				ReferrerUserId = 100,
				EmailOrPhone = "abc@xyz.com",
				Channel = "email"
			};

			var result = await _controller.InviteFriend(request) as ObjectResult;

			Assert.IsNotNull(result);
			Assert.AreEqual(500, result.StatusCode);
		}

		[TestMethod]
		public async Task InviteFriend_Successful_ReturnsOk()
		{
			_userServiceMock.Setup(s => s.GetReferralCode(100)).ReturnsAsync("REF100");
			_referralServiceMock.Setup(s => s.CreateReferralInvite(100, "abc@xyz.com", "sms", "REF100"))
				.ReturnsAsync(new ReferralOperationResult { Success = true });

			var request = new ReferralInviteRequest
			{
				ReferrerUserId = 100,
				EmailOrPhone = "abc@xyz.com",
				Channel = "sms"
			};

			var result = await _controller.InviteFriend(request) as OkObjectResult;

			Assert.IsNotNull(result);
			Assert.AreEqual(200, result.StatusCode);
		}

		[TestMethod]
		public async Task GetReferralLink_ReturnsOk_WithValidData()
		{
			// Arrange
			int userId = 100;
			string channel = "email";
			string mockLink = "https://mock.link/abc123";

			_referralServiceMock.Setup(s => s.GetReferralLinkAsync(userId, channel))
								.ReturnsAsync(mockLink);

			// Act
			var result = await _controller.GetReferralLink(userId, channel);

			// Assert
			var okResult = result as OkObjectResult;
			Assert.IsNotNull(okResult);
			Assert.AreEqual(200, okResult.StatusCode);
		}

		[TestMethod]
		public async Task GetReferralLink_ReturnsBadRequest_WhenUserIdIsInvalid()
		{
			// Arrange
			int userId = 0;
			string channel = "email";

			// Act
			var result = await _controller.GetReferralLink(userId, channel);

			// Assert
			var badResult = result as BadRequestObjectResult;
			Assert.IsNotNull(badResult);
			Assert.AreEqual("UserId and channel are required.", badResult.Value);
		}

		[TestMethod]
		public async Task GetReferralLink_ReturnsBadRequest_WhenChannelIsEmpty()
		{
			// Arrange
			int userId = 100;
			string channel = "";

			// Act
			var result = await _controller.GetReferralLink(userId, channel);

			// Assert
			var badResult = result as BadRequestObjectResult;
			Assert.IsNotNull(badResult);
			Assert.AreEqual("UserId and channel are required.", badResult.Value);
		}

		[TestMethod]
		public async Task GetReferralLink_ReturnsServerError_WhenExceptionIsThrown()
		{
			// Arrange
			int userId = 100;
			string channel = "sms";

			_referralServiceMock.Setup(s => s.GetReferralLinkAsync(userId, channel))
								.ThrowsAsync(new Exception("Service failed"));

			// Act
			var result = await _controller.GetReferralLink(userId, channel);

			// Assert
			var statusResult = result as ObjectResult;
			Assert.IsNotNull(statusResult);
			Assert.AreEqual(500, statusResult.StatusCode);
			Assert.AreEqual("Failed to retrieve referral link.", statusResult.Value);
		}
	}
}

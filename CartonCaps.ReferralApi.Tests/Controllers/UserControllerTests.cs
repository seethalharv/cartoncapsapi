using CartonCaps.ReferralApi.Controllers;
using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Models.Requests;
using CartonCaps.ReferralApi.Models.Responses;
using CartonCaps.ReferralApi.Repositories;
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
	public class UserControllerTests
	{
		private Mock<IUserRepository> _userRepositoryMock;
		private Mock<IReferralRepository> _referralRepositoryMock;
		private Mock<ILogger<UserController>> _loggerMock;
		private UserController _controller;

		[TestInitialize]
		public void Setup()
		{
			_userRepositoryMock = new Mock<IUserRepository>();
			_referralRepositoryMock = new Mock<IReferralRepository>();
			_loggerMock = new Mock<ILogger<UserController>>();

			_controller = new UserController(
				_userRepositoryMock.Object,
				_referralRepositoryMock.Object,
				_loggerMock.Object);
		}

		[TestMethod]
		public async Task UpdateReferralToRedeemedAsync_MissingReferralCode_ReturnsBadRequest()
		{
			var request = new UpdateReferralStatusRequest { userId = 200, ReferralCode = "" };

			var result = await _controller.UpdateReferralToRedeemedAsync(request);

			Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
		}

		[TestMethod]
		public async Task UpdateReferralToRedeemedAsync_ReferralCodeNotFound_ReturnsNotFound()
		{
			_userRepositoryMock.Setup(r => r.GetUserByReferralCodeAsync("REFX")).ReturnsAsync((UserReferralProfile)null);

			var request = new UpdateReferralStatusRequest { userId = 200, ReferralCode = "REFX" };

			var result = await _controller.UpdateReferralToRedeemedAsync(request);

			Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
		}

		[TestMethod]
		public async Task UpdateReferralToRedeemedAsync_ValidRequest_ReturnsOk()
		{
			_userRepositoryMock.Setup(r => r.GetUserByReferralCodeAsync("REF123")).ReturnsAsync(new UserReferralProfile
			{
				Id = 101,
				ReferralCode = "REF123"
			});

			_referralRepositoryMock.Setup(r => r.UpdateSuccessfulReferralToRedeemedAsync(200, "REF123"))
				.ReturnsAsync(new ReferralOperationResult
				{
					Success = true
				});

			var request = new UpdateReferralStatusRequest { userId = 200, ReferralCode = "REF123" };

			var result = await _controller.UpdateReferralToRedeemedAsync(request) as OkObjectResult;

			Assert.IsNotNull(result);
			Assert.AreEqual(200, result.StatusCode);
		}
	}
}

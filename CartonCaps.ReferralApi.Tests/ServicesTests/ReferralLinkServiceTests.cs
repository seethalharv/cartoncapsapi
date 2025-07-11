namespace CartonCaps.ReferralApi.Services.Tests;

using CartonCaps.ReferralApi.Models.Requests;
using CartonCaps.ReferralApi.Models.Responses;
using CartonCaps.ReferralApi.Services;
using CartonCaps.ReferralApi.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

[TestClass]
public class ReferralLinkServiceTests
{
	private Mock<IConfiguration> _mockConfig;
	private Mock<ILogger<ReferralLinkService>> _mockLogger;

	[TestInitialize]
	public void Setup()
	{
		_mockConfig = new Mock<IConfiguration>();
		_mockLogger = new Mock<ILogger<ReferralLinkService>>();
	}

	[TestMethod]
	public async Task GenerateReferralLinkAsync_WhenUseMockTrue_ReturnsMockUrl()
	{
		// Arrange
		_mockConfig.Setup(c => c["Branch:UseMock"]).Returns("true");

		var service = new ReferralLinkService(new HttpClient(), _mockConfig.Object, _mockLogger.Object);

		// Act
		var result = await service.GenerateReferralLinkAsync("REF123", "sms");

		// Assert
		Assert.AreEqual("https://cartoncaps.app.link/mock-ref123-sms", result);
	}

	[TestMethod]
	public async Task GenerateReferralLinkAsync_WhenUseMockFalse_CallsHttpAndReturnsUrl()
	{
		// Arrange
		var expectedUrl = "https://branch.io/generated-ref123";
		_mockConfig.Setup(c => c["Branch:UseMock"]).Returns("false");
		_mockConfig.Setup(c => c["Branch:Key"]).Returns("test_key");
		

		var fakeResponse = new BranchLinkResponse { url = expectedUrl };
		var json = JsonSerializer.Serialize(fakeResponse);
		var messageHandler = new FakeHttpMessageHandler(new HttpResponseMessage
		{
			StatusCode = HttpStatusCode.OK,
			Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
		});

		var httpClient = new HttpClient(messageHandler);
		var service = new ReferralLinkService(httpClient, _mockConfig.Object, _mockLogger.Object);

		// Act
		var result = await service.GenerateReferralLinkAsync("REF123", "email");

		// Assert
		Assert.AreEqual(expectedUrl, result);
	}

	[TestMethod]
	[ExpectedException(typeof(HttpRequestException))] // thrown by EnsureSuccessStatusCode()
	public async Task GenerateReferralLinkAsync_WhenApiReturnsBadRequest_ThrowsHttpRequestException()
	{
		_mockConfig.Setup(c => c["Branch:UseMock"]).Returns("false");
		_mockConfig.Setup(c => c["Branch:Key"]).Returns("test_key");

		var fakeResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
		var httpClient = new HttpClient(new FakeHttpMessageHandler(fakeResponse));

		var service = new ReferralLinkService(httpClient, _mockConfig.Object, _mockLogger.Object);

		await service.GenerateReferralLinkAsync("REF123", "email");
	}

	[TestMethod]
	[ExpectedException(typeof(Exception))] // because of: json?.url ?? throw new Exception(...)
	public async Task GenerateReferralLinkAsync_WhenApiReturnsEmptyBody_ThrowsException()
	{
		_mockConfig.Setup(c => c["Branch:UseMock"]).Returns("false");
		_mockConfig.Setup(c => c["Branch:Key"]).Returns("test_key");

		var emptyContentResponse = new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent("null", Encoding.UTF8, "application/json")
		};

		var httpClient = new HttpClient(new FakeHttpMessageHandler(emptyContentResponse));

		var service = new ReferralLinkService(httpClient, _mockConfig.Object, _mockLogger.Object);

		await service.GenerateReferralLinkAsync("REF123", "sms");
	}


}

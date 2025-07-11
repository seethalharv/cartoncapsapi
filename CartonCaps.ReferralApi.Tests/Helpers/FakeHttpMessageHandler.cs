using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartonCaps.ReferralApi.Tests.Helpers
{
	public class FakeHttpMessageHandler : HttpMessageHandler
	{
		private readonly HttpResponseMessage _response;

		public FakeHttpMessageHandler(HttpResponseMessage response)
		{
			_response = response;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			// Return the response you passed in during setup.
			return Task.FromResult(_response);
		}
	}
}

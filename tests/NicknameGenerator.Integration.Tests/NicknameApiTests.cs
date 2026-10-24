namespace NicknameGenerator.Integration.Tests
{
	using System.Net;
	using System.Net.Http.Json;
	using Microsoft.AspNetCore.Mvc.Testing;

	using Xunit;

	public sealed class NicknameApiTests : IClassFixture<WebApplicationFactory<Program>>
	{
		private readonly HttpClient _client;

		public NicknameApiTests(WebApplicationFactory<Program> factory)
		{
			_client = factory.CreateClient();
		}

		[Fact]
		public async Task Generate_ReturnsGoldenValue()
		{
			HttpResponseMessage _response = await _client.GetAsync("/api/nickname/550e8400-e29b-41d4-a716-446655440000");

			Assert.Equal(HttpStatusCode.OK, _response.StatusCode);

			NicknameResponse? _payload = await _response.Content.ReadFromJsonAsync<NicknameResponse>();
			Assert.NotNull(_payload);
			Assert.Equal("FrostyElk_00z5ReQiH", _payload!.Nickname);
		}

		[Fact]
		public async Task Generate_WithAttempt_IsDeterministic()
		{
			string _url = "/api/nickname/550e8400-e29b-41d4-a716-446655440000/attempt/1";

			NicknameResponse? _first = await _client.GetFromJsonAsync<NicknameResponse>(_url);
			NicknameResponse? _second = await _client.GetFromJsonAsync<NicknameResponse>(_url);

			Assert.NotNull(_first);
			Assert.NotNull(_second);
			Assert.Equal(_first!.Nickname, _second!.Nickname);
			Assert.NotEqual("FrostyElk_00z5ReQiH", _first.Nickname);
		}

		[Fact]
		public async Task InvalidGuid_Returns404()
		{
			HttpResponseMessage _response = await _client.GetAsync("/api/nickname/not-a-guid");
			Assert.Equal(HttpStatusCode.NotFound, _response.StatusCode);
		}

		private sealed record NicknameResponse(Guid UserId, string Nickname);
	}
}

namespace NicknameGenerator.DependencyInjection.Tests
{
	using Microsoft.Extensions.Configuration;

	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Options;

	using NicknameGenerator.Abstractions;

	using Xunit;

	public sealed class ServiceCollectionExtensionsTests
	{
		private const string VALID_SALT = "0123456789abcdef0123456789abcdef";

		[Fact]
		public void AddNicknameGenerator_WithConfigure_RegistersSingleton()
		{
			ServiceCollection _services = new();
			_services.AddNicknameGenerator(o => o.SecretSalt = VALID_SALT);
			ServiceProvider _provider = _services.BuildServiceProvider();

			INicknameGenerator _a = _provider.GetRequiredService<INicknameGenerator>();
			INicknameGenerator _b = _provider.GetRequiredService<INicknameGenerator>();
			Assert.Same(_a, _b);

			Guid _guid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
			Assert.Equal("FrostyElk_00z5ReQiH", _a.Generate(_guid));
		}

		[Fact]
		public void AddNicknameGenerator_WithConfiguration_BindsFromSection()
		{
			IConfigurationRoot _config = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["Nickname:SecretSalt"] = VALID_SALT,
					["Nickname:WordListVersion"] = "1",
					["Nickname:SuffixBytes"] = "6",
				})
				.Build();

			ServiceCollection _services = new();
			_services.AddNicknameGenerator(_config);
			ServiceProvider _provider = _services.BuildServiceProvider();

			INicknameGenerator _gen = _provider.GetRequiredService<INicknameGenerator>();
			Guid _guid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
			Assert.Equal("FrostyElk_00z5ReQiH", _gen.Generate(_guid));
		}

		[Fact]
		public void AddRandomNicknameGenerator_DoesNotRequireSalt()
		{
			ServiceCollection _services = new();
			_services.AddRandomNicknameGenerator();
			ServiceProvider _provider = _services.BuildServiceProvider();

			IRandomNicknameGenerator _gen = _provider.GetRequiredService<IRandomNicknameGenerator>();
			Assert.False(string.IsNullOrEmpty(_gen.Generate()));
		}

		[Fact]
		public void AddNicknameGenerator_WithoutSalt_ThrowsOnStart()
		{
			ServiceCollection _services = new();
			_services.AddNicknameGenerator(o => { /* no salt */ });
			ServiceProvider _provider = _services.BuildServiceProvider();

			// Access Value triggers validation
			OptionsValidationException _ex = Assert.Throws<OptionsValidationException>(
				() => _ = _provider.GetRequiredService<INicknameGenerator>());
			Assert.Contains("NicknameOptions", _ex.Message);
		}

		[Fact]
		public void CustomWordListProvider_IsRespected()
		{
			ServiceCollection _services = new();
			_services.AddSingleton<IWordListProvider>(
				new StubProvider(new WordList(1, new[] { "Solo" }, new[] { "Alpha" })));
			_services.AddNicknameGenerator(o => o.SecretSalt = VALID_SALT);
			ServiceProvider _provider = _services.BuildServiceProvider();

			INicknameGenerator _gen = _provider.GetRequiredService<INicknameGenerator>();
			Assert.StartsWith("SoloAlpha_", _gen.Generate(Guid.NewGuid()));
		}

		[Fact]
		public void CustomNicknameGenerator_TakesPrecedence()
		{
			// User registers their own INicknameGenerator BEFORE calling AddNicknameGenerator.
			// TryAddSingleton must respect the existing registration.
			ServiceCollection _services = new();
			_services.AddSingleton<INicknameGenerator>(new ConstantGenerator());
			_services.AddNicknameGenerator(o => o.SecretSalt = VALID_SALT);
			ServiceProvider _provider = _services.BuildServiceProvider();

			INicknameGenerator _gen = _provider.GetRequiredService<INicknameGenerator>();
			Assert.Equal("always-the-same", _gen.Generate(Guid.NewGuid()));
		}

		[Fact]
		public void AllThreeRegistrations_ShareSameInstance()
		{
			ServiceCollection _services = new ();
			_services.AddNicknameGenerator(o => o.SecretSalt = VALID_SALT);
			ServiceProvider _provider = _services.BuildServiceProvider();

			HmacNicknameGenerator _concrete = _provider.GetRequiredService<HmacNicknameGenerator>();
			INicknameGenerator _baseIf = _provider.GetRequiredService<INicknameGenerator>();
			IRetryableNicknameGenerator _retryIf = _provider.GetRequiredService<IRetryableNicknameGenerator>();

			Assert.Same(_concrete, _baseIf);
			Assert.Same(_concrete, _retryIf);
		}

		[Fact]
		public void RetryableGenerator_WorksThroughDI()
		{
			ServiceCollection _services = new ();
			_services.AddNicknameGenerator(o => o.SecretSalt = VALID_SALT);
			ServiceProvider _provider = _services.BuildServiceProvider();

			IRetryableNicknameGenerator _gen = _provider.GetRequiredService<IRetryableNicknameGenerator>();
			Guid _userId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

			// attempt=0 — совпадает с golden value
			Assert.Equal("FrostyElk_00z5ReQiH", _gen.Generate(_userId, 0));

			// attempt=1 — другой ник, но детерминированный
			string _a1 = _gen.Generate(_userId, 1);
			Assert.NotEqual(_a1, _gen.Generate(_userId, 0));
			Assert.Equal(_a1, _gen.Generate(_userId, 1));
		}

		private sealed class StubProvider : IWordListProvider
		{
			private readonly WordList _list;
			public StubProvider(WordList list) => _list = list;
			public WordList Get(int version) => _list;
		}

		private sealed class ConstantGenerator : INicknameGenerator
		{
			public string Generate(Guid userId) => "always-the-same";
		}
	}
}

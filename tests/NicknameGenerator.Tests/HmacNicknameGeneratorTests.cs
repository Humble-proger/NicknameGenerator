namespace NicknameGenerator.Tests
{
	using NicknameGenerator.Abstractions;

	using NicknameGenerator.Options;
	using Xunit;

	public class HmacNicknameGeneratorTests
	{
		private const string TEST_SALT = "0123456789abcdef0123456789abcdef"; // exactly 32 bytes
		private static readonly Guid _testGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

		private static HmacNicknameGenerator CreateDefault() =>
			new(new NicknameOptions { SecretSalt = TEST_SALT });

		[Fact]
		public void SameUser_ProducesSameNickname()
		{
			HmacNicknameGenerator _gen = CreateDefault();
			Assert.Equal(_gen.Generate(_testGuid), _gen.Generate(_testGuid));
		}

		[Fact]
		public void SameUser_ProducesSameNickname_AcrossInstances() =>
			Assert.Equal(
				CreateDefault().Generate(_testGuid),
				CreateDefault().Generate(_testGuid));

		[Fact]
		public void DifferentUsers_ProduceDifferentNicknames()
		{
			HmacNicknameGenerator _gen = CreateDefault();
			HashSet<string> _seen = [];
			Random _rng = new(42);
			byte[] _guidBytes = new byte[16];

			for (int _i = 0; _i < 10_000; _i++)
			{
				_rng.NextBytes(_guidBytes);
				Guid _guid = new(_guidBytes);
				Assert.True(_seen.Add(_gen.Generate(_guid)),
					$"Unexpected collision for Guid {_guid}.");
			}
		}

		[Fact]
		public void Nickname_ContainsConfiguredSeparator()
		{
			NicknameOptions _options = new() { SecretSalt = TEST_SALT, Separator = "-" };
			HmacNicknameGenerator _gen = new(_options);
			Assert.Contains("-", _gen.Generate(_testGuid));
		}

		[Theory]
		[InlineData(5, 7)]
		[InlineData(6, 9)]
		[InlineData(7, 10)]
		[InlineData(8, 11)]
		[InlineData(9, 13)]
		[InlineData(10, 14)]
		public void SuffixLength_MatchesSuffixBytes(int suffixBytes, int expectedSuffixLength)
		{
			NicknameOptions _options = new() { SecretSalt = TEST_SALT, SuffixBytes = suffixBytes };
			HmacNicknameGenerator _gen = new(_options);

			string _nick = _gen.Generate(_testGuid);
			int _sepIdx = _nick.LastIndexOf('_');
			Assert.True(_sepIdx > 0, $"Separator not found in '{_nick}'.");
			Assert.Equal(expectedSuffixLength, _nick.Length - _sepIdx - 1);
		}

		[Fact]
		public void DifferentSalts_ProduceDifferentNicknames()
		{
			HmacNicknameGenerator _a = new(
				new NicknameOptions { SecretSalt = new string('a', 32) });
			HmacNicknameGenerator _b = new(
				new NicknameOptions { SecretSalt = new string('b', 32) });

			Assert.NotEqual(_a.Generate(_testGuid), _b.Generate(_testGuid));
		}

		[Fact]
		public void EmptySalt_Throws()
		{
			NicknameOptions _options = new() { SecretSalt = "" };
			Assert.Throws<ArgumentException>(() => new HmacNicknameGenerator(_options));
		}

		[Fact]
		public void ShortSalt_Throws()
		{
			NicknameOptions _options = new() { SecretSalt = "too-short" };
			ArgumentException _ex = Assert.Throws<ArgumentException>(() => new HmacNicknameGenerator(_options));
			Assert.Contains("32 bytes", _ex.Message);
		}

		[Fact]
		public void GenerateParts_ToString_MatchesGenerate()
		{
			HmacNicknameGenerator _gen = CreateDefault();
			NicknameParts _parts = _gen.GenerateParts(_testGuid, 0);
			Assert.Equal($"{_parts.Adjective}{_parts.Noun}_{_parts.Suffix}", _gen.Generate(_testGuid));
		}

		[Fact]
		public void HmacGenerator_ImplementsBothInterfaces()
		{
			HmacNicknameGenerator _gen = CreateDefault();
			Assert.IsAssignableFrom<INicknameGenerator>(_gen);
			Assert.IsAssignableFrom<IRetryableNicknameGenerator>(_gen);
		}
	}
}

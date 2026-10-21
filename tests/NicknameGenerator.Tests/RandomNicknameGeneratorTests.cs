namespace NicknameGenerator.Tests
{
	using NicknameGenerator.Abstractions;
	using NicknameGenerator.Options;
	using Xunit;

	public sealed class RandomNicknameGeneratorTests
	{
		private static RandomNicknameGenerator CreateDefault() =>
			new(new NicknameOptions());

		[Fact]
		public void ProducesNicknamesWithoutSalt()
		{
			// Must not throw even though SecretSalt is empty.
			RandomNicknameGenerator _gen = CreateDefault();
			Assert.False(string.IsNullOrEmpty(_gen.Generate()));
		}

		[Fact]
		public void ConsecutiveCalls_ProduceDifferentNicknames()
		{
			RandomNicknameGenerator _gen = CreateDefault();
			HashSet<string> _set = [];
			for (int _i = 0; _i < 100; _i++)
				_set.Add(_gen.Generate());

			Assert.True(_set.Count >= 95, $"Only {_set.Count} unique nicknames out of 100.");
		}

		[Fact]
		public void Nickname_HasExpectedFormat()
		{
			RandomNicknameGenerator _gen = CreateDefault();
			string _nick = _gen.Generate();
			Assert.Matches("^[A-Za-z0-9]+_[0-9a-zA-Z]{9}$", _nick);
		}

		[Fact]
		public void RandomGenerator_DoesNotImplementRetryable()
		{
			RandomNicknameGenerator _gen = CreateDefault();
			Assert.IsAssignableFrom<IRandomNicknameGenerator>(_gen);
			Assert.IsNotAssignableFrom<IRetryableNicknameGenerator>(_gen);
		}
	}
}

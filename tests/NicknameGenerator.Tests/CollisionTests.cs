namespace NicknameGenerator.Tests
{
	using NicknameGenerator.Options;
	using Xunit;

	public sealed class CollisionTests
	{
		private const string TEST_SALT = "0123456789abcdef0123456789abcdef";
		private const int SAMPLE_SEED = 20240102;

		[Fact]
		public void NoCollisions_In100kNicknames_DefaultSettings()
		{
			// Space with SuffixBytes=6: 60 * 60 * 62^9 ≈ 1.35e19.
			// Birthday paradox expectation for N=100k: N²/(2*space) ≈ 3.7e-10 collisions.
			// Zero collisions is expected with overwhelming probability.
			AssertNoCollisions(count: 100_000, suffixBytes: 6);
		}

		[Fact]
		public void NoCollisions_In100kNicknames_MinimumSettings()
		{
			// Worst case: SuffixBytes=5 → 62^7 ≈ 3.5e12.
			// Space: 60 * 60 * 3.5e12 ≈ 1.26e16. Expectation at N=100k: ~0.0004.
			// Still zero collisions expected, but this is the "weakest" configuration.
			AssertNoCollisions(count: 100_000, suffixBytes: 5);
		}

		private static void AssertNoCollisions(int count, int suffixBytes)
		{
			HmacNicknameGenerator _gen = new(new NicknameOptions
			{
				SecretSalt = TEST_SALT,
				SuffixBytes = suffixBytes,
			});

			HashSet<string> _seen = new(capacity: count);
			Random _rng = new(SAMPLE_SEED);
			byte[] _guidBytes = new byte[16];

			for (int _i = 0; _i < count; _i++)
			{
				_rng.NextBytes(_guidBytes);
				string _nick = _gen.Generate(new Guid(_guidBytes));
				Assert.True(_seen.Add(_nick), $"Collision on iteration {_i}: '{_nick}'");
			}

			Assert.Equal(count, _seen.Count);
		}
	}
}

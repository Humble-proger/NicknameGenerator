namespace NicknameGenerator.Tests
{
	using NicknameGenerator.Abstractions;

	using NicknameGenerator.Options;
	using Xunit;

	/// <summary>
	/// Verifies that small changes in input produce large changes in output.
	/// Catches "only uses first 8 bytes of Guid" or "ignores suffix bytes" bugs.
	/// </summary>
	public class AvalancheTests
	{
		private const string TEST_SALT = "0123456789abcdef0123456789abcdef";

		[Fact]
		public void AdjacentGuids_ProduceDifferentAdjectivesAndNouns()
		{
			// Guids differing only in last byte must produce UNCORRELATED adjectives/nouns.
			// With 256 samples from 60 buckets: expected distinct = 60 * (1 - (59/60)^256) ≈ 59.2.
			// Sigma of distinct count here is ~0.7, so threshold 50 is ~13σ below expected.
			// A broken avalanche (e.g., hash ignoring last byte) would give EXACTLY 1 distinct.
			HmacNicknameGenerator _gen = new(new NicknameOptions { SecretSalt = TEST_SALT });
			HashSet<string> _adjSet = [];
			HashSet<string> _nounSet = [];

			for (int _i = 0; _i < 256; _i++)
			{
				// Vary only the last byte of the Guid
				Guid _g = new(0, 0, 0, [0, 0, 0, 0, 0, 0, 0, (byte)_i]);
				NicknameParts _parts = _gen.GenerateParts(_g, 0);
				_adjSet.Add(_parts.Adjective);
				_nounSet.Add(_parts.Noun);
			}

			Assert.True(_adjSet.Count >= 50,
				$"Only {_adjSet.Count}/60 adjectives appeared — avalanche broken.");
			Assert.True(_nounSet.Count >= 50,
				$"Only {_nounSet.Count}/60 nouns appeared — avalanche broken.");
		}

		[Fact]
		public void SingleBitChange_ChangesSuffix()
		{
			// Flipping the top bit of the Guid must change the suffix.
			HmacNicknameGenerator _gen = new(new NicknameOptions { SecretSalt = TEST_SALT });

			Guid _a = Guid.Parse("00000000-0000-0000-0000-000000000000");
			Guid _b = Guid.Parse("00000000-0000-0000-0000-000000000001");

			Assert.NotEqual(_gen.GenerateParts(_a, 0).Suffix, _gen.GenerateParts(_b, 0).Suffix);
		}
	}
}

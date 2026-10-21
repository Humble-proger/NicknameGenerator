namespace NicknameGenerator.Tests
{
	using NicknameGenerator.Options;
	using Xunit;
	using Xunit.Abstractions;

	public sealed class GoldenValuesTests
	{
		private const string TEST_SALT = "0123456789abcdef0123456789abcdef"; // exactly 32 bytes

		[Theory]
		[MemberData(nameof(GoldenPairs_v1))]
		public void VerifyGoldenValues_v1_DoNotChange(Guid userId, string expected)
		{
			HmacNicknameGenerator _gen = new(new NicknameOptions { SecretSalt = TEST_SALT });
			Assert.Equal(expected, _gen.Generate(userId));
		}

		[Theory]
		[MemberData(nameof(GoldenPairs_v1_attempts))]
		public void VerifyGoldenValues_v1_attempts_DoNotChange(Guid userId, int attempt, string expected)
		{
			HmacNicknameGenerator _gen = new (new NicknameOptions { SecretSalt = TEST_SALT });
			Assert.Equal(expected, _gen.Generate(userId, attempt));
		}

		public static IEnumerable<object[]> GoldenPairs_v1()
		{
			yield return new object[] { Guid.Empty, "PuffyRobin_1bpdIBZwE" };
			yield return new object[] { Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), "SnappyHawk_02TZYdzzn" };
			yield return new object[] { Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), "FrostyElk_00z5ReQiH" };
			yield return new object[] { Guid.Parse("00000000-0000-0000-0000-000000000001"), "WildDingo_15NckPT5d" };
			yield return new object[] { Guid.Parse("00000000-0000-0000-0000-000000000002"), "NobleEagle_0P4rwmXoI" };
			yield return new object[] { Guid.Parse("00000000-0000-0000-0000-000000000003"), "CleverWolf_02J5sKNBs" };
			yield return new object[] { Guid.Parse("10000000-0000-0000-0000-000000000000"), "FancyPenguin_1gvmemFlh" };
			yield return new object[] { Guid.Parse("80000000-0000-0000-0000-000000000000"), "BriskOwl_0TbKEPXAd" };
		}

		public static IEnumerable<object[]> GoldenPairs_v1_attempts()
		{
			yield return new object[] { Guid.Empty, 1, "SillyDove_0L2ictlYw" };
			yield return new object[] { Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), 1, "SpicyCrow_0j9XCwIfs" };
			yield return new object[] { Guid.Parse("550e8400-e29b-41d4-a716-446655440000"), 2, "SpicyDeer_0dN9URnH9" };
		}

		private readonly ITestOutputHelper _output;

		public GoldenValuesTests(ITestOutputHelper output) => _output = output;

		// -------------------------------------------------------------------------
		// Printer — one-time tool, not part of CI.
		// To regenerate after an INTENTIONAL breaking change:
		//   1. Remove `Skip = "..."` below
		//   2. dotnet test --filter "FullyQualifiedName~PrintGoldenValues" --logger "console;verbosity=detailed"
		//   3. Copy Standard Output into GoldenPairs_v1 (and rename method to _v2)
		//   4. Re-add Skip
		// -------------------------------------------------------------------------
		[Fact(Skip = "One-time tool. See comment above.")]
		public void PrintGoldenValues()
		{
			HmacNicknameGenerator _gen = new(new NicknameOptions { SecretSalt = TEST_SALT });
			foreach (object[] _pair in GoldenPairs_v1())
			{
				Guid _g = (Guid)_pair[0];
				_output.WriteLine($"        yield return new object[] {{ Guid.Parse(\"{_g}\"), \"{_gen.Generate(_g)}\" }};");
			}
		}
	}
}

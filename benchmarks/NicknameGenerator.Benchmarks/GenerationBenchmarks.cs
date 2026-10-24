namespace NicknameGenerator.Benchmarks
{
	using BenchmarkDotNet.Attributes;
	using NicknameGenerator.Abstractions;

	using NicknameGenerator.Options;

	/// <summary>
	/// Measures per-call cost of the deterministic and random nickname generators.
	/// All methods are on the hot path of user-facing APIs (registration, profile load),
	/// so both latency (ns) and allocations (B) matter.
	/// </summary>
	[MemoryDiagnoser]
	[SimpleJob(warmupCount: 3, iterationCount: 10)]
	public class GenerationBenchmarks
	{
		private const string SALT = "0123456789abcdef0123456789abcdef";

		private HmacNicknameGenerator _hmac = null!;
		private RandomNicknameGenerator _random = null!;
		private Guid _userId;

		[GlobalSetup]
		public void Setup()
		{
			NicknameOptions _options = new() { SecretSalt = SALT };
			_hmac = new HmacNicknameGenerator(_options);
			_random = new RandomNicknameGenerator(_options);

			// Fixed Guid so each invocation produces the same string — measures the
			// pipeline, not string comparison variance.
			_userId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
		}

		/// <summary>
		/// Baseline: <c>attempt = 0</c> — the common path, no appended bytes.
		/// </summary>
		[Benchmark(Baseline = true)]
		public string Hmac_Attempt0() => _hmac.Generate(_userId, 0);

		/// <summary>
		/// <c>attempt = 1</c> — exercises <c>AppendAttempt</c> (one extra allocation).
		/// </summary>
		[Benchmark]
		public string Hmac_Attempt1() => _hmac.Generate(_userId, 1);

		/// <summary>
		/// <c>attempt = 0</c>, returns parts instead of concatenated string.
		/// </summary>
		[Benchmark]
		public NicknameParts Hmac_Attempt0_Parts() => _hmac.GenerateParts(_userId, 0);

		/// <summary>
		/// Non-deterministic generator — uses <c>RandomNumberGenerator.GetBytes</c>.
		/// </summary>
		[Benchmark]
		public string Random() => _random.Generate();
	}
}

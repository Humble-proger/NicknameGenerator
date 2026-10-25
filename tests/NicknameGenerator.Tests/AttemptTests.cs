namespace NicknameGenerator.Tests
{
	using NicknameGenerator.Abstractions;

	using NicknameGenerator.Options;
	using Xunit;

	public class AttemptTests
	{
		private const string TEST_SALT = "0123456789abcdef0123456789abcdef";
		private static readonly Guid _testGuid =
			Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

		private static HmacNicknameGenerator Create() =>
			new(new NicknameOptions { SecretSalt = TEST_SALT });

		[Fact]
		public void AttemptZero_MatchesDefaultCall()
		{
			HmacNicknameGenerator _gen = Create();
			Assert.Equal(_gen.Generate(_testGuid), _gen.Generate(_testGuid, 0));
		}

		[Fact]
		public void AttemptZero_StillMatchesGoldenValue()
		{
			HmacNicknameGenerator _gen = Create();
			Assert.Equal("FrostyElk_00z5ReQiH", _gen.Generate(_testGuid, 0));
		}

		[Fact]
		public void DifferentAttempts_ProduceDifferentNicknames()
		{
			HmacNicknameGenerator _gen = Create();
			HashSet<string> _set = [];
			for (int _attempt = 0; _attempt < 20; _attempt++)
			{
				string _nick = _gen.Generate(_testGuid, _attempt);
				Assert.True(_set.Add(_nick),
					$"Duplicate nickname on attempt {_attempt}: '{_nick}'");
			}
		}

		[Fact]
		public void SameAttempt_IsDeterministic_AcrossInstances()
		{
			HmacNicknameGenerator _gen1 = Create();
			HmacNicknameGenerator _gen2 = Create();
			for (int _attempt = 0; _attempt <= 10; _attempt++)
			{
				Assert.Equal(_gen1.Generate(_testGuid, _attempt), _gen2.Generate(_testGuid, _attempt));
			}
		}

		[Fact]
		public void NegativeAttempt_Throws()
		{
			HmacNicknameGenerator _gen = Create();
			ArgumentOutOfRangeException _ex = Assert.Throws<ArgumentOutOfRangeException>(
				() => _gen.Generate(_testGuid, -1));
			Assert.Equal("attempt", _ex.ParamName);
		}

		[Fact]
		public void Int32MaxAttempt_DoesNotThrow()
		{
			HmacNicknameGenerator _gen = Create();
			_ = _gen.Generate(_testGuid, int.MaxValue);
		}

		[Fact]
		public void AttemptsAcrossDifferentUsers_AreUniform()
		{
			// Ensures the attempt bytes actually influence the hash. If they were ignored,
			// all attempts for the same user would be identical — caught by the test above.
			// Here we verify that attempt=1 outputs are still uniformly distributed.
			HmacNicknameGenerator _gen = Create();
			Dictionary<string, int> _adjCounts = [];
			Dictionary<string, int> _nounCounts = [];
			Random _rng = new(20240103);
			byte[] _guidBytes = new byte[16];

			const int _samples = 10_000;
			for (int _i = 0; _i < _samples; _i++)
			{
				_rng.NextBytes(_guidBytes);
				NicknameParts _parts = _gen.GenerateParts(new Guid(_guidBytes), attempt: 1);
				_adjCounts[_parts.Adjective] = _adjCounts.GetValueOrDefault(_parts.Adjective) + 1;
				_nounCounts[_parts.Noun] = _nounCounts.GetValueOrDefault(_parts.Noun) + 1;
			}

			// With 10K samples over 60 buckets: expected 167 per bucket.
			// If distribution were broken (e.g. only 5 buckets used), some counts would be 2000+.
			int _expected = _samples / 60;
			Assert.All(_adjCounts.Values, c =>
				Assert.InRange(c, _expected / 2, _expected * 2));
			Assert.All(_nounCounts.Values, c =>
				Assert.InRange(c, _expected / 2, _expected * 2));
		}

		[Fact]
		public void GenerateWithRetry_FindsFreeNickname()
		{
			HmacNicknameGenerator _gen = Create();
			HashSet<string> _used = [];
			Random _rng = new(20240104);
			byte[] _guidBytes = new byte[16];

			for (int _user = 0; _user < 1_000; _user++)
			{
				_rng.NextBytes(_guidBytes);
				Guid _userId = new(_guidBytes);

				string _nick = _gen.GenerateWithRetry(_userId, n => !_used.Contains(n));
				Assert.True(_used.Add(_nick));
			}
		}

		[Fact]
		public void GenerateWithRetry_ThrowsWhenAllTaken()
		{
			HmacNicknameGenerator _gen = Create();
			Assert.Throws<InvalidOperationException>(
				() => _gen.GenerateWithRetry(_testGuid, _ => false, maxAttempts: 3));
		}

		[Fact]
		public async Task GenerateWithRetryAsync_FindsFreeNickname()
		{
			HmacNicknameGenerator _gen = Create();
			HashSet<string> _used = [];
			Random _rng = new(20240106);
			byte[] _guidBytes = new byte[16];

			for (int _user = 0; _user < 100; _user++)
			{
				_rng.NextBytes(_guidBytes);
				Guid _userId = new(_guidBytes);

				string _nick = await _gen.GenerateWithRetryAsync(
					_userId, n => Task.FromResult(!_used.Contains(n)));
				Assert.True(_used.Add(_nick));
			}
		}

		[Fact]
		public async Task GenerateWithRetryAsync_ThrowsWhenAllTaken()
		{
			HmacNicknameGenerator _gen = Create();
			await Assert.ThrowsAsync<InvalidOperationException>(
				() => _gen.GenerateWithRetryAsync(_testGuid, _ => Task.FromResult(false), maxAttempts: 3));
		}

		[Fact]
		public async Task GenerateWithRetryAsync_RespectsCancellation()
		{
			HmacNicknameGenerator _gen = Create();
			using CancellationTokenSource _cts = new();
			int _calls = 0;

			Func<string, Task<bool>> _isAvailable = _ =>
			{
				_calls++;
				if (_calls >= 3) _cts.Cancel();
				return Task.FromResult(false);
			};

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
				_gen.GenerateWithRetryAsync(_testGuid, _isAvailable, maxAttempts: 100, ct: _cts.Token));

			Assert.True(_calls >= 3);
		}

		[Fact]
		public async Task GenerateWithRetryAsync_IsDeterministic()
		{
			HmacNicknameGenerator _gen = Create();
			string _attempt0 = _gen.Generate(_testGuid, 0);
			string _attempt1 = _gen.Generate(_testGuid, 1);

			Func<string, Task<bool>> _isAvailable = n =>
				Task.FromResult(n != _attempt0 && n != _attempt1);

			string _r1 = await _gen.GenerateWithRetryAsync(_testGuid, _isAvailable);
			string _r2 = await _gen.GenerateWithRetryAsync(_testGuid, _isAvailable);

			Assert.Equal(_r1, _r2);
			Assert.Equal(_gen.Generate(_testGuid, 2), _r1);  // должно быть attempt=2
		}

		[Fact]
		public async Task GenerateWithRetryAsync_ThrowsOnNullArguments()
		{
			HmacNicknameGenerator _gen = Create();

			await Assert.ThrowsAsync<ArgumentNullException>(() =>
				RetryableNicknameGeneratorExtensions.GenerateWithRetryAsync(
					null!, _testGuid, _ => Task.FromResult(true)));

			await Assert.ThrowsAsync<ArgumentNullException>(() =>
				_gen.GenerateWithRetryAsync(_testGuid, null!));
		}
	}
}

namespace NicknameGenerator.Tests
{
	using NicknameGenerator.Abstractions;

	using NicknameGenerator.Options;
	using Xunit;

	public sealed class GeneratorIntegrationTests
	{
		private const string TEST_SALT = "0123456789abcdef0123456789abcdef";
		private static readonly Guid _testGuid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

		[Fact]
		public void HmacGenerator_IsThreadSafe()
		{
			HmacNicknameGenerator _gen = new(new NicknameOptions { SecretSalt = TEST_SALT });
			string _expected = _gen.Generate(_testGuid);

			string[] _results = new string[1000];
			Parallel.For(0, _results.Length, i => _results[i] = _gen.Generate(_testGuid));

			foreach (string _r in _results)
				Assert.Equal(_expected, _r);
		}

		[Fact]
		public void RandomGenerator_IsThreadSafe()
		{
			RandomNicknameGenerator _gen = new(new NicknameOptions());
			System.Collections.Concurrent.ConcurrentBag<string> _set = [];

			Parallel.For(0, 1000, _ => _set.Add(_gen.Generate()));

			// All should be valid format - no exceptions, no nulls.
			foreach (string _nick in _set)
				Assert.Matches("^[A-Za-z0-9]+_[0-9a-zA-Z]{9}$", _nick);
		}

		[Fact]
		public void CustomWordList_IsRespected()
		{
			WordList _custom = new(1, ["Solo"], ["Alpha"]);
			StubProvider _provider = new(_custom);

			HmacNicknameGenerator _gen = new(
				new NicknameOptions { SecretSalt = TEST_SALT, WordListVersion = 1 },
				wordListProvider: _provider);

			string _nick = _gen.Generate(_testGuid);
			Assert.StartsWith("SoloAlpha_", _nick);
		}

		[Fact]
		public void DifferentWordListVersion_ProducesDifferentOutput()
		{
			// Same guid, same salt, different word list → different nickname.
			HmacNicknameGenerator _gen1 = new(
				new NicknameOptions { SecretSalt = TEST_SALT, WordListVersion = 1 });
			HmacNicknameGenerator _gen2 = new(
				new NicknameOptions { SecretSalt = TEST_SALT, WordListVersion = 1 },
				wordListProvider: new StubProvider(new WordList(1, ["X"], ["Y"])));

			Assert.NotEqual(_gen1.Generate(_testGuid), _gen2.Generate(_testGuid));
		}

		private sealed class StubProvider : IWordListProvider
		{
			private readonly WordList _list;
			public StubProvider(WordList list) => _list = list;
			public WordList Get(int version) => _list;
		}
	}
}

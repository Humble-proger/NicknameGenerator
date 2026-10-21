namespace NicknameGenerator.Tests
{
	using NicknameGenerator.Abstractions;
    using Internal;
	using Xunit;

    public sealed class EmbeddedWordListProviderTests
    {
        [Fact]
        public void V1_LoadsSuccessfully()
        {
            WordList _list = EmbeddedWordListProvider.Instance.Get(1);
            Assert.Equal(1, _list.Version);
            Assert.NotEmpty(_list.Adjectives);
            Assert.NotEmpty(_list.Nouns);
        }

        [Fact]
        public void V1_IsCached()
        {
            WordList _first = EmbeddedWordListProvider.Instance.Get(1);
            WordList _second = EmbeddedWordListProvider.Instance.Get(1);
            Assert.Same(_first, _second);
        }

        [Fact]
        public void V999_ThrowsNotSupported()
        {
			NotSupportedException _ex = Assert.Throws<NotSupportedException>(
                () => EmbeddedWordListProvider.Instance.Get(999));
            Assert.Contains("version 999", _ex.Message);
        }

		[Fact]
		public void V0_ThrowsArgumentOutOfRange() => Assert.Throws<ArgumentOutOfRangeException>(
				() => EmbeddedWordListProvider.Instance.Get(0));

		[Fact]
        public void AllWords_AreAsciiAlphanumeric()
        {
            WordList _list = EmbeddedWordListProvider.Instance.Get(1);
            foreach (string _word in _list.Adjectives.Concat(_list.Nouns))
            {
                Assert.True(_word.All(c => char.IsAsciiLetterOrDigit(c)),
                    $"Word '{_word}' contains non-alphanumeric char.");
            }
        }
    }
}
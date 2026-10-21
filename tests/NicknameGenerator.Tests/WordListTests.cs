namespace NicknameGenerator.Tests
{
	using NicknameGenerator.Abstractions;
	using Xunit;

    public class WordListTests
    {
        [Fact]
        public void Constructor_RejectsEmptyAdjectives()
        {
            Assert.Throws<ArgumentException>(() =>
                new WordList(1, [], ["x"]));
        }

		[Fact]
		public void Constructor_RejectsEmptyNouns() => Assert.Throws<ArgumentException>(() =>
																	new WordList(1, ["x"], []));

		[Fact]
        public void Constructor_RejectsNulls()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new WordList(1, null!, ["x"]));
            Assert.Throws<ArgumentNullException>(() =>
                new WordList(1, ["x"], null!));
        }

        [Fact]
        public void Constructor_RejectsZeroVersion()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new WordList(0, ["x"], ["y"]));
        }
    }
}
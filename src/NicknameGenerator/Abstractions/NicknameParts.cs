namespace NicknameGenerator.Abstractions
{
    /// <summary>
    /// Structured representation of a generated nickname.
    /// </summary>
    public sealed class NicknameParts
    {
        /// <summary>
        /// Adjective part (e.g. <c>Brave</c>).
        /// </summary>
        public string Adjective { get; }

        /// <summary>
        /// Noun part (e.g. <c>Falcon</c>).
        /// </summary>
        public string Noun { get; }

        /// <summary>
        /// Base62 suffix (e.g. <c>a3Kx9Lm</c>).
        /// </summary>
        public string Suffix { get; }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public NicknameParts(string adjective, string noun, string suffix)
        {
            Adjective = adjective ?? throw new ArgumentNullException(nameof(adjective));
            Noun = noun ?? throw new ArgumentNullException(nameof(noun));
            Suffix = suffix ?? throw new ArgumentNullException(nameof(suffix));
        }

		public override string ToString() => $"{Adjective}{Noun}_{Suffix}";
    }
}
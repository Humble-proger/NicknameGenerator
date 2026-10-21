namespace NicknameGenerator.Abstractions
{
    /// <summary>
    /// Immutable set of words used to build nicknames
    /// </summary>
    public sealed class WordList
    {
        /// <summary>
        /// Version this list was loaded for.
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// Adjective part candidates. Non-empty.
        /// </summary>
        public IReadOnlyList<string> Adjectives { get; }

        /// <summary>
        /// Noun part candidates. Non-empty.
        /// </summary>
        public IReadOnlyList<string> Nouns { get; }

        /// <summary>
        /// Creates a validated word list.
        /// </summary>
        public WordList(
            int version,
            IReadOnlyList<string> adjectives,
            IReadOnlyList<string> nouns
        )
        {
			if (version < 1) throw new ArgumentOutOfRangeException(nameof(version));
			if (adjectives is null) throw new ArgumentNullException(nameof(adjectives));
            if (nouns is null) throw new ArgumentNullException(nameof(nouns));
            if (adjectives.Count == 0) throw new ArgumentException("Must be non-empty", nameof(adjectives));
            if (nouns.Count == 0) throw new ArgumentException("Must be non-empty", nameof(nouns));
            
            Version = version;
            Adjectives = adjectives;
            Nouns = nouns;
		}
    }
}
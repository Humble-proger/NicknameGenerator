namespace NicknameGenerator
{
	using System.Security.Cryptography;
	using NicknameGenerator.Abstractions;
	using NicknameGenerator.Internal;

	using NicknameGenerator.Options;

	/// <summary>
	/// Non-deterministic nickname generator. Useful for previews or for nicknames
	/// that don't need to be stable across calls (e.g. "regenerate" buttons).
	/// </summary>
	public sealed class RandomNicknameGenerator : IRandomNicknameGenerator
	{
		private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

		private readonly NicknameOptions _options;
		private readonly WordList _wordList;
		private readonly int _suffixLength;

		/// <summary>Creates a new generator.</summary>
		/// <param name="options">
		/// Generator options. <see cref="NicknameOptions.SecretSalt"/> is not required —
		/// this generator does not use HMAC.
		/// </param>
		/// <param name="wordListProvider">Optional word-list provider. Defaults to embedded.</param>
		public RandomNicknameGenerator(NicknameOptions options, IWordListProvider? wordListProvider = null)
		{
			if (options is null)
				throw new ArgumentNullException(nameof(options));
			options.Validate(requireSecretSalt: false);

			_options = options;
			_wordList = (wordListProvider ?? EmbeddedWordListProvider.Instance).Get(options.WordListVersion);
			_suffixLength = Base62Encoder.GetPaddedLength(options.SuffixBytes);
		}

		public string Generate()
		{
			//  4 bytes for word indices + SuffixBytes (max 10) for suffix = 14. Use 16 for alignment.
			byte[] _buffer = new byte[16];
			_rng.GetBytes(_buffer);

			int _adjIndex = ((_buffer[0] << 8) | _buffer[1]) % _wordList.Adjectives.Count;
			int _nounIndex = ((_buffer[2] << 8) | _buffer[3]) % _wordList.Nouns.Count;

			string _suffix = Base62Encoder.Encode(_buffer, offset: 4, count: _options.SuffixBytes, minLength: _suffixLength);

			return $"{_wordList.Adjectives[_adjIndex]}{_wordList.Nouns[_nounIndex]}{_options.Separator}{_suffix}";
		}
	}
}

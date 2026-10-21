namespace NicknameGenerator
{
	using System;
	using System.Security.Cryptography;

	using System.Text;
	using NicknameGenerator.Abstractions;
	using NicknameGenerator.Internal;

	using NicknameGenerator.Options;


	/// <summary>
	/// Deterministic nickname generator based on HMAC-SHA256.
	/// The same user identifier always produces the same nickname, given identical
	/// options (salt, word list version, suffix length).
	/// </summary>
	public sealed class HmacNicknameGenerator : IRetryableNicknameGenerator
	{
		private readonly byte[] _salt;
		private readonly IWordListProvider _wordListProvider;
		private readonly NicknameOptions _options;
		private readonly WordList _wordList;
		private readonly int _suffixLength;

		/// <summary>Creates a new generator.</summary>
		/// <param name="options">Generator options. Must pass <see cref="NicknameOptions.Validate"/>.</param>
		/// <param name="wordListProvider"> Optional word-list provider. Defaults to the embedded provider (English, frozen versions).</param>
		/// <exception cref="ArgumentNullException">In case options is null.</exception>
		public HmacNicknameGenerator(NicknameOptions options, IWordListProvider? wordListProvider = null)
		{
			if (options is null)
				throw new ArgumentNullException(nameof(options));

			options.Validate(requireSecretSalt: true);

			_options = options;
			_salt = Encoding.UTF8.GetBytes(options.SecretSalt);
			_wordListProvider = wordListProvider ?? EmbeddedWordListProvider.Instance;
			_wordList = _wordListProvider.Get(options.WordListVersion);
			_suffixLength = Base62Encoder.GetPaddedLength(options.SuffixBytes);
		}

		public string Generate(Guid userId)
		{
			NicknameParts _parts = GenerateParts(userId, 0);
			return $"{_parts.Adjective}{_parts.Noun}{_options.Separator}{_parts.Suffix}";
		}

		public string Generate(Guid userId, int attempt)
		{
			NicknameParts _parts = GenerateParts(userId, attempt);
			return $"{_parts.Adjective}{_parts.Noun}{_options.Separator}{_parts.Suffix}";
		}

		public NicknameParts GenerateParts(Guid userId, int attempt)
		{
			if (attempt < 0)
				throw new ArgumentOutOfRangeException(nameof(attempt), attempt, "Must be >= 0.");
			
			byte[] _hash = ComputeHash(userId.ToByteArray(), attempt);

			int _adjIndex = ((_hash[0] << 8) | _hash[1]) % _wordList.Adjectives.Count;
			int _nounIndex = ((_hash[2] << 8) | _hash[3]) % _wordList.Nouns.Count;

			string _suffix = Base62Encoder.Encode(_hash, offset: 4, count: _options.SuffixBytes, minLength: _suffixLength);

			return new NicknameParts(
				_wordList.Adjectives[_adjIndex],
				_wordList.Nouns[_nounIndex],
				_suffix
			);
		}

		private byte[] ComputeHash(byte[] data, int attempt)
		{
			byte[] _input = attempt == 0 ? data : AppendAttempt(data, attempt);
			return ComputeHash(_input);
		}

		private byte[] AppendAttempt(byte[] data, int attempt)
		{
			byte[] _input = new byte[data.Length + 4];
			Buffer.BlockCopy(data, 0, _input, 0, data.Length);

			uint _ua = (uint) attempt;
			_input[data.Length] = (byte)(_ua >> 24);
			_input[data.Length + 1] = (byte)(_ua >> 16);
			_input[data.Length + 2] = (byte)(_ua >> 8);
			_input[data.Length + 3] = (byte) _ua;

			return _input;
		}

		private byte[] ComputeHash(byte[] data)
		{
#if NET8_0_OR_GREATER
            byte[] _hash = new byte[32];
            HMACSHA256.HashData(_salt, data, _hash);
            return _hash;
#else
			using HMACSHA256 _hmac = new(_salt);
			return _hmac.ComputeHash(data);
#endif
		}
	}
}

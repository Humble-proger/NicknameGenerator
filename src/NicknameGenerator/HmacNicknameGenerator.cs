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
    public sealed class HmacNicknameGenerator : INicknameGenerator
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
            NicknameParts _parts = GenerateParts(userId);
            return $"{_parts.Adjective}{_parts.Noun}{_options.Separator}{_parts.Suffix}";
        }

        internal NicknameParts GenerateParts(Guid userId)
        {
            byte[] _guidBytes = userId.ToByteArray();
            byte[] _hash = ComputeHash(_guidBytes);

            int _adjIndex = ((_hash[0] << 8) | _hash[1]) % _wordList.Adjectives.Count;
            int _nounIndex = ((_hash[2] << 8) | _hash[3]) % _wordList.Nouns.Count;

            string _suffix = Base62Encoder.Encode(_hash, offset: 4, count: _options.SuffixBytes, minLength: _suffixLength);

            return new NicknameParts(
                _wordList.Adjectives[_adjIndex],
                _wordList.Nouns[_nounIndex],
                _suffix
            );
        }

        private byte[] ComputeHash(byte[] data)
        {
#if NET8_0_OR_GREATER
            byte[] _hash = new byte[32];
            HMACSHA256.HashData(_salt, data, _hash);
            return _hash;
#else
            using HMACSHA256 _hmac = new (_salt);
            return _hmac.ComputeHash(data);
#endif
        }
	}
}
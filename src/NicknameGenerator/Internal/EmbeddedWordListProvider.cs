namespace NicknameGenerator.Internal
{
	using System.Collections.Concurrent;
	using System.Reflection;

	using System.Text;

	using NicknameGenerator.Abstractions;

	public sealed class EmbeddedWordListProvider : IWordListProvider
	{
		private static readonly ConcurrentDictionary<int, Lazy<WordList>> _cache = new();

		public static EmbeddedWordListProvider Instance { get; } = new();

		public WordList Get(int version)
		{
			if (version < 1)
				throw new ArgumentOutOfRangeException(nameof(version), version, "Must be >= 1.");

			return _cache.GetOrAdd(version, v => new Lazy<WordList>(() => Load(v))).Value;
		}

		private static WordList Load(int version)
		{
			string _resourceName = $"words-en-v{version}.txt";
			Assembly _asm = typeof(EmbeddedWordListProvider).Assembly;

			using Stream _stream = _asm.GetManifestResourceStream(_resourceName) ?? throw new NotSupportedException($"Word list version {version} is not embedded. Expected resource '{_resourceName}'.");

			using StreamReader _reader = new(_stream, Encoding.UTF8);
			(List<string> _adjectives, List<string> _nouns) = Parse(_reader, version);

			return new WordList(version, _adjectives.AsReadOnly(), _nouns.AsReadOnly());
		}

		private static (List<string> Adjectives, List<string> Nouns) Parse(TextReader reader, int version)
		{
			List<string> _adjectives = [];
			List<string> _nouns = [];
			List<string>? _current = null;

			string? _line;
			while ((_line = reader.ReadLine()) is not null)
			{
				_line = _line.Trim();
				if (_line.Length == 0) continue;

				if (_line.StartsWith("#", StringComparison.Ordinal))
				{
					string _section = _line.Substring(1).Trim();
					_current = _section switch
					{
						"adjectives" => _adjectives,
						"nouns" => _nouns,
						_ => throw new InvalidDataException($"Unknown section '#{_section}' in words-en-v{version}.txt")
					};
					continue;
				}

				if (_current is null)
					throw new InvalidDataException($"Word '{_line}' appears before any section in words-en-v{version}.txt.");

				ValidateWord(_line, version);
				_current.Add(_line);
			}

			if (_adjectives.Count == 0 || _nouns.Count == 0)
				throw new InvalidDataException($"words-en-v{version}.txt must contain both '# adjectives' and '# nouns' sections.");

			return (_adjectives, _nouns);
		}

		private static void ValidateWord(string word, int version)
		{
			if (word.Length > 32)
				throw new InvalidDataException($"Word '{word}' too long (>32 chars) in words-en-v{version}.txt.");

			foreach (char _c in word)
			{
				if (!((_c >= 'a' && _c <= 'z') || (_c >= 'A' && _c <= 'Z') || (_c >= '0' && _c <= '9')))
					throw new InvalidDataException($"Word '{word}' contains illegal character '{_c}' in words-en-v{version}.txt. Only ASCII letters and digits are allowed.");
			}
		}
	}
}

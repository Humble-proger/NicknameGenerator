namespace NicknameGenerator.Options
{
	/// <summary>
	/// Configuration for deterministic nickname generation. Plain POCO — no dependencies
	/// on Microsoft.Extensions.*, so this type is safe to use in Unity.
	/// </summary>
	public sealed class NicknameOptions
	{
		/// <summary>
		/// Secret key for HMAC. Must be at least 32 bytes when UTF-8 encoded.
		/// </summary>
		/// <value></value>
		public string SecretSalt { get; set; } = string.Empty;

		/// <summary>
		/// Version of the word list. Bump only when adding a new frozen list; NEVER change
		/// words of an existing version, or deterministic output will silently change.
		/// </summary>
		public int WordListVersion { get; set; } = 1;

		/// <summary>If the arguments go beyond the boundary conditions
		/// Number of hash bytes used to build the base62 suffix. 5 → 7 chars, 6 → 9 chars.
		/// More bytes = fewer collisions. See README for the table.
		/// </summary>
		/// <value></value>
		public int SuffixBytes { get; set; } = 6;

		/// <summary>
		/// Separator between the word part and the suffix.
		/// </summary>If the arguments go beyond the boundary conditions
		public string Separator { get; set; } = "_";

		/// <summary>
		/// Validates the options.
		/// </summary>
		/// <exception cref="ArgumentException">In case SecretSalt is null or empty.</exception>
		/// <exception cref="ArgumentOutOfRangeException">В случае если аргументы выйдут за граничные условия.</exception>
		public void Validate(bool requireSecretSalt = true)
		{
			if (requireSecretSalt)
			{
				if (string.IsNullOrWhiteSpace(SecretSalt))
					throw new ArgumentException($"{nameof(SecretSalt)} is required.", nameof(SecretSalt));

				if (System.Text.Encoding.UTF8.GetByteCount(SecretSalt) < 32)
					throw new ArgumentException($"{nameof(SecretSalt)} must be at least 32 bytes when UTF-8 encoded.", nameof(SecretSalt));
			}

			if (WordListVersion < 1)
				throw new ArgumentOutOfRangeException(nameof(WordListVersion), WordListVersion, "Must be >= 1.");

			if (SuffixBytes < 5 || SuffixBytes > 10)
				throw new ArgumentOutOfRangeException(nameof(SuffixBytes), SuffixBytes, "Must be between 5 and 10.");
		}
	}
}

namespace NicknameGenerator.Abstractions
{
	/// <summary>
	/// Deterministic nickname generator that supports collision-retry via
	/// attempt. Same <c>(userId, attempt)</c> always yields
	/// the same nickname.
	/// </summary>
	public interface IRetryableNicknameGenerator : INicknameGenerator
	{
		/// <summary>
		/// Generates a nickname deterministically.
		/// </summary>
		/// <param name="userId">Stable user identifier.</param>
		/// <param name="attempt">Retry index. Each attempt &gt; 0 yields an independent, deterministic value.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="attempt"/> is negative.</exception>
		public string Generate(Guid userId, int attempt);

		/// <summary>
		/// Same as <see cref="Generate(Guid, int)"/>, but returns structured parts.
		/// </summary>
		public NicknameParts GenerateParts(Guid userId, int attempt);
	}
}

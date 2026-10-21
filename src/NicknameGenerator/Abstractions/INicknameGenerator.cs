namespace NicknameGenerator.Abstractions
{
	/// <summary>
	/// Deterministic nickname generator: same userId always yields the same nickname.
	/// </summary>
	public interface INicknameGenerator
	{
		/// <summary>
		/// Generates a nickname deterministically from the given user identifier.
		/// </summary>
		/// <param name="userId">Stable user identifier (e.g. Guid from your database)</param>
		/// <returns>Nickname, e.g. <c>BraveFalcon_a3Kx9Lm</c>.</returns>
		public string Generate(Guid userId);
	}
}

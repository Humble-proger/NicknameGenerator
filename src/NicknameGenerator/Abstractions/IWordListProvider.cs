namespace NicknameGenerator.Abstractions
{
	/// <summary>
	/// Provides word lists by version. Implement this to plug custom word lists
	/// (e.g. from a database or a file) without rebuilding the library.
	/// </summary>
	public interface IWordListProvider
	{
		/// <summary>
		/// Returns a frozen word list for the given <paramref name="version"/>.
		/// The same version must always yield the same words — never modify an existing version.
		/// </summary>
		/// <exception cref="NotSupportedException">The version is not available.</exception>
		public WordList Get(int version);
	}
}

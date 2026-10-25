namespace NicknameGenerator.Abstractions
{
	/// <summary>
	/// Convenience extensions for <see cref="IRetryableNicknameGenerator"/>.
	/// </summary>
	public static class RetryableNicknameGeneratorExtensions
	{
		/// <summary>
		/// Synchronously tries attempts <c>0..maxAttempts-1</c> until
		/// <paramref name="isAvailable"/> returns <c>true</c>.
		/// </summary>
		public static string GenerateWithRetry(
			this IRetryableNicknameGenerator generator,
			Guid userId,
			Func<string, bool> isAvailable,
			int maxAttempts = 16)
		{
			if (generator is null) throw new ArgumentNullException(nameof(generator));
			if (isAvailable is null) throw new ArgumentNullException(nameof(isAvailable));
			if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));

			for (int _attempt = 0; _attempt < maxAttempts; _attempt++)
			{
				string _nickname = generator.Generate(userId, _attempt);
				if (isAvailable(_nickname))
				{
					return _nickname;
				}
			}

			throw new InvalidOperationException($"No available nickname for user {userId} after {maxAttempts} attempts. Increase maxAttempts or SuffixBytes.");
		}

		/// <summary>
		/// Asynchronously tries attempts <c>0..maxAttempts-1</c> until
		/// <paramref name="isAvailableAsync"/> returns <c>true</c>.
		/// </summary>
		/// <remarks>
		/// Use this when your availability check performs I/O (DB, HTTP, cache).
		/// The generator itself is CPU-bound and stays synchronous —
		/// only the availability callback is awaited.
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> or <paramref name="isAvailableAsync"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="maxAttempts"/> is less than 1.</exception>
		/// <exception cref="InvalidOperationException">All <paramref name="maxAttempts"/> nicknames were taken.</exception>
		/// <exception cref="OperationCanceledException"><paramref name="ct"/> was cancelled.</exception>
		public static async Task<string> GenerateWithRetryAsync(
			this IRetryableNicknameGenerator generator,
			Guid userId,
			Func<string, Task<bool>> isAvailableAsync,
			int maxAttempts = 16,
			CancellationToken ct = default
		)
		{
			if (generator is null) throw new ArgumentNullException(nameof(generator));
			if (isAvailableAsync is null) throw new ArgumentNullException(nameof(isAvailableAsync));
			if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));

			for (int _attempt = 0; _attempt < maxAttempts; _attempt++)
			{
				ct.ThrowIfCancellationRequested();

				string _nickname = generator.Generate(userId, _attempt);
				bool _available = await isAvailableAsync(_nickname).ConfigureAwait(false);

				if (_available)
					return _nickname;
			}

			throw new InvalidOperationException($"No available nickname for user {userId} after {maxAttempts} attempts. Increase maxAttempts or SuffixBytes.");
		}
	}
}

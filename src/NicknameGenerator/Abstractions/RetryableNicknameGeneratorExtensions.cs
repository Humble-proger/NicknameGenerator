namespace NicknameGenerator.Abstractions
{
    /// <summary>
    /// Convenience extensions for <see cref="IRetryableNicknameGenerator"/>.
    /// </summary>
    public static class RetryableNicknameGeneratorExtensions
    {
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
    }
}
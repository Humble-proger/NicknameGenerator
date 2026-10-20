namespace NicknameGenerator.Abstractions
{
    /// <summary>
    /// Non-deterministic nickname generator, useful for previews or non-bound nicknames.
    /// </summary>
    public interface IRandomNicknameGenerator
    {
        /// <summary>
        /// Generates a random nickname on each call.
        /// </summary>
        public string Generate();
    }
}
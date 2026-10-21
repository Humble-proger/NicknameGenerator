namespace NicknameGenerator.Tests
{
    using NicknameGenerator.Abstractions;
    using NicknameGenerator.Internal;
    using NicknameGenerator.Options;
    using Xunit;

    /// <summary>
    /// Verifies that the hash output is uniformly distributed across word list indices
    /// and that adjective/noun choices are independent. Catches subtle bugs like
    /// "use hash[0] for both adj and noun" or "always use first byte of hash".
    /// </summary>
    public sealed class DistributionTests
    {
        private const string TEST_SALT = "0123456789abcdef0123456789abcdef";
        private const int SAMPLE_SIZE = 60_000;   // ~1000 per bucket with 60 words → χ² valid
        private const double CHISQUARECRITICAL_59DF_01 = 88.4;   // α=0.01, df=59
        private const int SAMPLE_SEED = 20240101;  // fixed → deterministic test

        [Fact]
        public void AdjectiveIndices_AreUniform()
        {
            double _chiSq = ComputeChiSquare(parts => parts.Adjective, bucketCount: 60);
            Assert.True(_chiSq < CHISQUARECRITICAL_59DF_01,
                $"Adjective distribution not uniform: χ²={_chiSq:F1}, critical={CHISQUARECRITICAL_59DF_01}");
        }

        [Fact]
        public void NounIndices_AreUniform()
        {
            double _chiSq = ComputeChiSquare(parts => parts.Noun, bucketCount: 60);
            Assert.True(_chiSq < CHISQUARECRITICAL_59DF_01,
                $"Noun distribution not uniform: χ²={_chiSq:F1}, critical={CHISQUARECRITICAL_59DF_01}");
        }

        [Fact]
        public void AdjectiveAndNoun_AreIndependent()
        {
            // Joint coverage: if adj and noun come from the same hash bytes (correlated),
            // we'd observe only ~60 distinct (adj, noun) pairs. If independent, we should
            // observe ~3400 out of 3600 possible pairs with 60K samples.
            HashSet<(string, string)> _observed = [];
            foreach (NicknameParts _parts in SampleParts(10_000))
            {
                _observed.Add((_parts.Adjective, _parts.Noun));
            }

            // Independent expectation: 3600 * (1 - e^(-10000/3600)) ≈ 3378 distinct pairs.
            // Correlation would give ~60. Threshold of 3000 is 25σ below the independent mean.
            Assert.True(_observed.Count >= 3000,
                $"Only {_observed.Count} distinct (adj, noun) pairs observed. " +
                "Adj and noun are likely correlated (same hash bytes).");
        }

        [Fact]
        public void SuffixLastCharacter_IsUniform()
        {
			// Last base62 digit = X mod 62, uniform over 62 buckets for X uniform in [0, 2^48).
			// This catches: broken HMAC, broken Alphabet, wrong offset in Base62Encoder.Encode.
			int[] _counts = new int[62];
            foreach (NicknameParts _parts in SampleParts(5_000))
                _counts[IndexOf(_parts.Suffix[_parts.Suffix.Length - 1])]++;

            int _expected = 5_000 / 62;
            double _chiSq = 0;
            foreach (int _count in _counts)
            {
                double _diff = _count - _expected;
                _chiSq += _diff * _diff / (double)_expected;
            }

            Assert.True(_chiSq < 95.0,
                $"Last suffix character not uniform: χ²={_chiSq:F1} (df=61, critical≈90.5).");
        }

        // --- helpers ---

        private static IEnumerable<NicknameParts> SampleParts(int count)
        {
            HmacNicknameGenerator _gen = new(new NicknameOptions { SecretSalt = TEST_SALT });
            Random _rng = new(SAMPLE_SEED);
            byte[] _guidBytes = new byte[16];

            for (int _i = 0; _i < count; _i++)
            {
                _rng.NextBytes(_guidBytes);
                yield return _gen.GenerateParts(new Guid(_guidBytes));
            }
        }

        private static double ComputeChiSquare(Func<NicknameParts, string> selector, int bucketCount)
        {
            int[] _counts = new int[bucketCount];
            Dictionary<string, int> _wordToIndex = [];

            foreach (NicknameParts _ in SampleParts(1))
            {
                break;
            }

            WordList _list = EmbeddedWordListProvider.Instance.Get(1);
            for (int _i = 0; _i < _list.Adjectives.Count; _i++) _wordToIndex[_list.Adjectives[_i]] = _i;
            for (int _i = 0; _i < _list.Nouns.Count; _i++) _wordToIndex[_list.Nouns[_i]] = _i;

            foreach (NicknameParts _parts in SampleParts(SAMPLE_SIZE))
            {
                string _word = selector(_parts);
                _counts[_wordToIndex[_word]]++;
            }

            int _expected = SAMPLE_SIZE / bucketCount;
            double _chiSq = 0;
            foreach (int _count in _counts)
            {
                double _diff = _count - _expected;
                _chiSq += _diff * _diff / (double)_expected;
            }
            return _chiSq;
        }

        private static int IndexOf(char c)
        {
            // Must match Base62Encoder.Alphabet order
            const string _alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int _idx = _alphabet.IndexOf(c);
            if (_idx < 0) throw new InvalidOperationException($"Unexpected base62 char: '{c}'");
            return _idx;
        }
    }
}
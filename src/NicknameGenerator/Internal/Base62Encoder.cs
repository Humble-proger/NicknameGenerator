namespace NicknameGenerator.Internal
{
	using System.Numerics;

	/// <summary>
	/// Deterministic base62 encoder. Treats input bytes as a big-endian unsigned integer.
	/// Big-endian is a fixed choice, not a platform default: golden values depend on it.
	/// </summary>
	public static class Base62Encoder
	{
		private const string ALPHABET = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private const int ALPHABET_LENGTH = 62;

		/// <summary>
		/// Encodes <paramref name="count"/> bytes from <paramref name="data"/>, starting at
		/// <paramref name="offset"/>, as a base62 string. Left-pads with '0' up to
		/// <paramref name="minLength"/> characters.
		/// </summary>
		public static string Encode(byte[] data, int offset, int count, int minLength = 0)
		{
			if (data is null) throw new ArgumentNullException(nameof(data));
			if (offset < 0 || offset > data.Length) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0 || offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(count));
			if (minLength < 0) throw new ArgumentOutOfRangeException(nameof(minLength));

			if (count == 0)
			{
				return minLength > 0 ? new string(ALPHABET[0], minLength) : string.Empty;
			}

			byte[] _littleEndian = new byte[count + 1];
			for (int _i = 0; _i < count; _i++)
			{
				_littleEndian[_i] = data[offset + count - 1 - _i];
			}

			BigInteger _value = new(_littleEndian);

			if (_value.IsZero)
			{
				return minLength > 0
					? new string(ALPHABET[0], minLength)
					: ALPHABET[0].ToString();
			}

			int _charCount = 0;
			BigInteger _remaining = _value;
			while (_remaining > BigInteger.Zero)
			{
				_remaining /= ALPHABET_LENGTH;
				_charCount++;
			}
			if (_charCount < minLength)
			{
				_charCount = minLength;
			}

			char[] _buffer = new char[_charCount];
			int _pos = _charCount;
			while (_value > BigInteger.Zero)
			{
				int _digit = (int)(_value % ALPHABET_LENGTH);
				_buffer[--_pos] = ALPHABET[_digit];
				_value /= ALPHABET_LENGTH;
			}

			// Left-pad with '0'
			while (_pos > 0)
			{
				_buffer[--_pos] = ALPHABET[0];
			}

			return new string(_buffer);
		}

		/// <summary>
		/// Encodes all bytes of <paramref name="data"/>.
		/// </summary>
		public static string Encode(byte[] data, int minLength = 0)
		{
			if (data is null) throw new ArgumentNullException(nameof(data));
			return Encode(data, 0, data.Length, minLength);
		}

		/// <summary>
		/// Number of base62 chars required to represent <paramref name="byteCount"/> bytes
		/// with full entropy. Used to compute the fixed suffix length.
		/// </summary>
		/// <remarks>
		/// = ceil(byteCount * 8 / log2(62)). Precomputed for the supported range to avoid
		/// floating-point math in the hot path.
		/// </remarks>
		public static int GetPaddedLength(int byteCount) => byteCount switch
		{
			1 => 2,
			2 => 3,
			3 => 5,
			4 => 6,
			5 => 7,
			6 => 9,
			7 => 10,
			8 => 11,
			9 => 13,
			10 => 14,
			_ => throw new ArgumentOutOfRangeException(nameof(byteCount), byteCount,
					  "Supported range is 1..10 bytes."),
		};
	}
}

namespace NicknameGenerator.Tests
{
	using NicknameGenerator.Internal;
	using Xunit;

	public sealed class Base62EncoderTests
	{
		[Fact]
		public void Zero_EncodesAsSingleZero() => Assert.Equal("0", Base62Encoder.Encode([0]));

		[Fact]
		public void Zero_WithPadding_EncodesAsFullZeros() => Assert.Equal("0000000", Base62Encoder.Encode([0], minLength: 7));

		[Fact]
		public void One_EncodesAsOne() => Assert.Equal("1", Base62Encoder.Encode([1]));

		[Fact]
		public void IsBigEndian() => Assert.Equal("1", Base62Encoder.Encode([0x00, 0x01]));

		[Fact]
		public void HighBit_IsPositiveNumber() => Assert.Equal("47", Base62Encoder.Encode([0xFF]));

		[Theory]
		[InlineData(5, 7)]
		[InlineData(6, 9)]
		[InlineData(7, 10)]
		[InlineData(8, 11)]
		[InlineData(9, 13)]
		[InlineData(10, 14)]
		public void PaddedLength_MatchesMath(int byteCount, int expected) => Assert.Equal(expected, Base62Encoder.GetPaddedLength(byteCount));

		[Fact]
		public void PaddedOutput_HasFixedLength()
		{
			Random _rng = new(42);
			for (int _i = 0; _i < 1000; _i++)
			{
				byte[] _bytes = new byte[6];
				_rng.NextBytes(_bytes);
				string _s = Base62Encoder.Encode(_bytes, minLength: 9);
				Assert.Equal(9, _s.Length);
			}
		}

		[Fact]
		public void AllBytesSupported_NoExceptions()
		{
			for (int _b = 0; _b <= 0xFF; _b++)
			{
				string _s = Base62Encoder.Encode([(byte)_b]);
				Assert.NotEmpty(_s);
			}
		}

		[Fact]
		public void SameInput_SameOutput()
		{
			byte[] _data = [0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC];
			Assert.Equal(Base62Encoder.Encode(_data), Base62Encoder.Encode(_data));
		}

		[Fact]
		public void DifferentInput_DifferentOutput()
		{
			byte[] _a = [0x00, 0x00, 0x00, 0x00, 0x00, 0x01];
			byte[] _b = [0x00, 0x00, 0x00, 0x00, 0x00, 0x02];
			Assert.NotEqual(Base62Encoder.Encode(_a), Base62Encoder.Encode(_b));
		}
	}
}

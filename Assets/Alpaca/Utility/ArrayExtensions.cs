using Int32 = System.Int32;


namespace Alpaca
{
	internal static class ArrayExtensions
	{
		public static Int32 GetHighestBitSet( this byte[] data )
		{
			for( int i = 0; i < data.Length; ++i )
			{
				int byteIndex = data.Length - 1 - i;

				for( int j = 0; j < 8; ++j )
				{
					int bitIndex = 7 - j;
					if( (data[byteIndex] & (1 << bitIndex)) != 0 )
					{
						return (byteIndex * 8) + bitIndex;
					}
				}
			}

			return 0;
		}

		public static bool GetBit( this byte[] data, Int32 bitIndex )
		{
			Int32 mask = 1 << (bitIndex & 0x7);
			Int32 i = (bitIndex >> 3);
			Int32 result = (data[bitIndex] & mask);
			return result != 0;
		}

		public static void SetBit( this byte[] data, Int32 bitIndex, bool value )
		{
			Int32 mask = 1 << (bitIndex & 0x7);
			Int32 i = (bitIndex >> 3);
			Int32 result = value ? (data[i] | mask) : (data[i] & ~mask);
			data[i] = (byte)result;
		}
	}
}
using System;

namespace Alpaca.Serialization
{
	public static class Arithmetic
	{
		// ZigZag encodes a signed integer and maps it to a unsigned integer
		public static ulong ZigZagEncode( Int64 value ) { return (UInt64)((value >> 63) ^ (value << 1)); }

		// Decodes a ZigZag encoded integer back to a signed integer
		public static long ZigZagDecode( UInt64 value) { return ( ((Int64)(value >> 1) & 0x7FFFFFFFFFFFFFFFL) ^ ((Int64)(value << 63) >> 63) ); }

		public static int VarIntSize(ulong value) =>
				value <= 240 ? 1 :
				value <= 2287 ? 2 :
				value <= 67823 ? 3 :
				value <= 16777215 ? 4 :
				value <= 4294967295 ? 5 :
				value <= 1099511627775 ? 6 :
				value <= 281474976710655 ? 7 :
				value <= 72057594037927935 ? 8 :
				9;
	}
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using UnityEngine;


namespace Alpaca.Serialization
{
	// A stream that can do bit wise manipulation
	// IMPORTANT! After writing bools, you must call AlignToByte() before writing any non-bool
	public class BitWriter : IPoolable<BitWriter>
	{
		DataStream _sink;
		byte[] _bitmap;

		bool _inUse;


		#region Interfaces

		// IPoolable interface
		public bool InUse() { return _inUse; }
		public void SetInUse() { _inUse = true; }

		// ICapacityConstruct<BitWriter> interface
		public BitWriter CapacityConstruct()
		{
			return new BitWriter( _sink.CapacityConstruct() );
		}
		
		// IDisposable interface
		public void Dispose()
		{
			_inUse = false;
			_sink.Reset();
		}

		#endregion // Interfaces


		// PUBLIC

		public BitWriter( DataStream stream )
		{
			_sink = stream;
			_bitmap = new byte[ DataStream.Div8Ceil( stream.GetByteCapacity() ) ];
		}

		// main writing interface, either normal (uncompressed) or packed (compressed)

		public void Normal<T>(T value)
		{
			BaseNormal<T>.s_magic.Write( this, value );
		}

		public void Packed<T>(T value)
		{
			BasePacked<T>.s_magic.Write( this, value );
		}

		public void AlignToByte() { _sink.ByteAlignWrite(); }

		public void Write<T>( ref T source ) where T : IBitSerializable
		{
			source.Write( this );
		}

		public UInt64 GetStableHash64() { return _sink.GetStableHash64(); }

		// array interface
		// TODO: longer-term, implement a nice FixedArray<T> and use that instead

		public void ArrayNormal<T>( T[] payload, Int32 payloadCount )
		{
			Normal<Int32>( payloadCount );
			for( Int32 i = 0; i < payloadCount; ++i ) { Normal<T>( payload[i] ); }
		}

		public void ArrayPacked<T>( T[] payload, Int32 payloadCount )
		{
			Packed<Int32>( payloadCount );
			for( Int32 i = 0; i < payloadCount; ++i ) { Packed<T>( payload[i] ); }
		}

		public void ArrayNormalDiff<T>( T[] payload, Int32 payloadCount, T[] compare, Int32 compareCount )
		{
			Normal<Int32>( payloadCount );
			WriteBitmap<T>( payload, payloadCount, compare, compareCount );
			for( Int32 i = 0; i < payloadCount; ++i ) 
			{
				if( _bitmap.GetBit(i) ) { Normal<T>( payload[i] ); }
			}
		}

		public void ArrayPackedDiff<T>( T[] payload, Int32 payloadCount, T[] compare, Int32 compareCount )
		{
			Packed<Int32>( payloadCount );
			WriteBitmap<T>( payload, payloadCount, compare, compareCount );
			for( Int32 i = 0; i < payloadCount; ++i ) 
			{
				if( _bitmap.GetBit(i) ) { Packed<T>( payload[i] ); }
			}
		}


		// internal workhorse writing functions

		public void Bit    ( bool   v ) { _sink.WriteBit (        v); }
		public void Byte   ( byte   v ) { _sink.WriteByte(        v); }
		public void ByteU32( UInt32 v ) { _sink.WriteByte( (byte)v ); }
		public void ByteU64( UInt64 v ) { _sink.WriteByte( (byte)v ); }

		public void UInt16( UInt16 v )
		{
			UInt32 value = (UInt32)v;
			ByteU32(  value		);
			ByteU32( (value >>  8) );
		}
		public void UInt32( UInt32 value )
		{
			ByteU32(  value        );
			ByteU32( (value >>  8) );
			ByteU32( (value >> 16) );
			ByteU32( (value >> 24) );
		}
		public void UInt64( UInt64 value )
		{
			ByteU64(  value        );
			ByteU64( (value >>  8) );
			ByteU64( (value >> 16) );
			ByteU64( (value >> 24) );
			ByteU64( (value >> 32) );
			ByteU64( (value >> 40) );
			ByteU64( (value >> 48) );
			ByteU64( (value >> 56) );
		}
 
		// writes a variable length integer from the stream, up to size UInt64
		// this could take as many as 9 bytes (ie, it might actually be larger)
		public void UInt64Packed(UInt64 value)
		{
			if (value <= 240) ByteU64(value);
			else if (value <= 2287)
			{
				ByteU64(((value - 240) >> 8) + 241);
				ByteU64(value - 240);
			}
			else if (value <= 67823)
			{
				ByteU64(249);
				ByteU64((value - 2288) >> 8);
				ByteU64(value - 2288);
			}
			else
			{
				UInt64 header = 255;
				UInt64 match = 0x00FF_FFFF_FFFF_FFFFUL;
				while (value <= match)
				{
					--header;
					match >>= 8;
				}
				ByteU64(header);
				int max = (int)(header - 247);
				for (int i = 0; i < max; ++i) { ByteU64(value >> (i << 3)); }
			}
		}


		// PRIVATE

		void WriteBitmap<T>( T[] payload, Int32 payloadCount, T[] compare, Int32 compareCount )
		{
			Int32 diffCount = 0;
			Int32 commonCount = Math.Min( payloadCount, compareCount );
			Int32 i = 0;
			for( ; i < commonCount; ++i )
			{
				if( !EqualityComparer<T>.Default.Equals( payload[i], compare[i] ) )
				{
					_bitmap.SetBit(i, true);
					++diffCount;
				}
				else
				{
					_bitmap.SetBit(i, false);
				}
			}
			// if payload is longer than compare, all values in payload after compare length are "changed"
			for( ; i < payloadCount; ++i )
			{
				_bitmap.SetBit(i, true);
				++diffCount;
			}
			// pad the byte array with zeros (technically not necessary as these values should be ignored on read)
			while( (i & 0x7) != 0 )
			{
				_bitmap.SetBit(i, false);
				++i;
			}

			Int32 bitmapCount = DataStream.Div8Ceil( payloadCount );
			_sink.WriteByteArray( _bitmap, 0, bitmapCount );
		}
	}
}
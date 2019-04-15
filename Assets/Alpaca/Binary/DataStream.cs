using System;
using System.IO;

using UnityEngine;


namespace Alpaca.Serialization
{
	// implement this on classes or structs you want to serialize with BitReader and BitWriter
	public interface IBitSerializable
	{
		void Read( BitReader reader );
		void Write( BitWriter writer );
	}

	// Supports writing bits and bytes.
	// See BitWriter and BitReader for easy to use, higher level interfaces.
	//
	// Caller is responsible for:
	// - ensuring there is sufficient capacity in the DataStream before writing
	// - ensuring there is enough data before reading
	// - ensuring that the DataStream is byte-aligned before reading or writing bytes
	public class DataStream : ICapacityConstruct<DataStream>
	{ 
		byte[] _buffer;
		// current amount of data in the buffer (ie, the amount of bits written to it)
		Int32 _bitLength;
		// current position where bits/bytes will be read or written
		Int32 _bitPosition;


		#region Interfaces

		// ICapacityConstruct<DataStream> interface
		public DataStream CapacityConstruct()
		{
			return new DataStream( GetByteCapacity() );
		}
		
		#endregion // Interfaces


		// PUBLIC

		public Int32 GetByteCapacity() { return _buffer.Length; }
		public Int32 GetByteLength() { return Div8Ceil(_bitLength); }
		public void SetByteLength( Int32 byteLength ) { _bitLength = byteLength * 8; }
		// note that you can ONLY read/write bits until the stream is byte aligned (see ByteAlignRead/ByteAlignWrite)
		public bool IsByteAligned() { return (_bitPosition & 0x7) == 0; }

		public Int32 GetBitCapacity() { return _buffer.Length << 3; }

		public Int32 GetBytePosition() { return (_bitPosition >> 3); }
		public void SetBytePosition( Int32 bytePosition ) { _bitPosition = bytePosition * 8; }
		
		public Int32 GetBitPosition() { return _bitPosition; }
		public void SetBitPosition( Int32 bitPosition ) { _bitPosition = bitPosition; }

		public bool HasDataToRead( Int32 sizeInBits ) { return (_bitPosition + sizeInBits) <= _bitLength; }
		public bool HasRoomToWrite( Int32 sizeInBits ) { return (_bitPosition + sizeInBits) <= GetBitCapacity(); }

		public byte[] GetBuffer() { return _buffer; }
		public UInt64 GetStableHash64() { return _buffer.GetStableHash64(); }

		public void Reset()
		{
			_bitLength = 0;
			_bitPosition = 0;
		}

		public DataStream( int byteCapacity )
		{
			_buffer = new byte[byteCapacity];
			_bitPosition = 0;
			_bitLength = 0;
		}

		public bool ReadBit()
		{
			Debug.Assert( HasDataToRead(1) );
			return _buffer.GetBit( _bitPosition++ );
		}

		public void ByteAlignRead()
		{
			_bitPosition = Div8Ceil( _bitPosition );
			Debug.Assert( GetBytePosition() <= GetByteLength() );
		}

		public void WriteBit( bool value )
		{
			Debug.Assert( HasRoomToWrite(1) );
			_buffer.SetBit( _bitPosition, value );
			++_bitPosition;
			_bitLength = Math.Max( _bitLength, _bitPosition );
		}

		public void ByteAlignWrite()
		{
			if( IsByteAligned() ) { return; }
			Debug.Assert( GetBytePosition() < GetByteCapacity() );
			Int32 mask = (Int32)(0xFFFFFFFF >> (32 - (_bitPosition & 0x7)));
			Int32 i = GetBytePosition();
			_buffer[i] = (byte)(_buffer[i] & mask);
			_bitPosition = Div8Ceil( _bitPosition );
			_bitLength = Math.Max( _bitLength, _bitPosition );
		}

		public byte ReadByte()
		{
			Debug.Assert( IsByteAligned() );
			Debug.Assert( HasDataToRead(8) );
			return ReadByteInternal();
		}

		public void ReadByteArray( byte[] destination, int destinationOffset, int byteCount )
		{
			Debug.Assert( IsByteAligned() );
			Int32 bitCount = byteCount << 3;
			Debug.Assert( HasDataToRead( bitCount ) );
			for( int i = 0; i < byteCount; ++i )
			{
				destination[destinationOffset + i] = ReadByteInternal();
			}
		}

		public void WriteByte( byte value )
		{
			Debug.Assert( IsByteAligned() );
			Debug.Assert( HasRoomToWrite(8) );
			WriteByteInternal( value );
			_bitLength = Math.Max( _bitLength, _bitPosition );
		}

		public void WriteByteArray( byte[] source, int sourceOffset, int byteCount )
		{
			Debug.Assert( IsByteAligned() );
			Int32 bitCount = byteCount << 3;
			Debug.Assert( HasRoomToWrite( bitCount ) );
			for( int i = 0; i < byteCount; ++i )
			{
				WriteByteInternal( source[sourceOffset + i] );
			}
			_bitLength = Math.Max( _bitPosition, _bitLength );
		}

		public override string ToString()
		{
			return System.BitConverter.ToString( _buffer, 0, (int)GetByteLength());
		}


		// STATIC

		public static Int32 Div8Ceil( Int32 value )
		{
			bool addOne = (value & 0x7) != 0;
			return (value >> 3) + (addOne ? 1 : 0);
		}


		// PRIVATE

		// does not assert if there is no data to read
		byte ReadByteInternal()
		{
			Int32 i = GetBytePosition();
			_bitPosition += 8;
			return (byte)_buffer[i];
		}

		// does not advance the length nor assert if there is no room to write
		void WriteByteInternal( byte value )
		{
			_buffer[ GetBytePosition() ] = value;
			_bitPosition += 8;
		}
	}
}
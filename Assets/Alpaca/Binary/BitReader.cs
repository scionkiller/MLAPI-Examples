using System;
using System.IO;
using System.Text;

using Alpaca.Internal;

using UnityEngine;

namespace Alpaca.Serialization
{

// A stream that can do bit wise manipulation
// IMPORTANT! After reading bools, you must call AlignToByte() before reading any non-bool
public class BitReader: IPoolable<BitReader>
{
	DataStream _source;
	byte[] _bitmap;

	bool _inUse;


	#region Interfaces

	// IPoolable interface
	public bool InUse() { return _inUse; }
	public void SetInUse() { _inUse = true; }

	// ICapacityConstruct<BitReader> interface
	public BitReader CapacityConstruct()
	{
		return new BitReader( _source.CapacityConstruct() );
	}
	
	// IDisposable interface
	public void Dispose()
	{
		_inUse = false;
		_source.Reset();
	}

	#endregion // Interfaces


	// PUBLIC

	public BitReader( DataStream stream )
	{
		_source = stream;
		_bitmap = new byte[ DataStream.Div8Ceil( stream.GetByteCapacity() ) ];
	}

	// main reading interface, either normal (uncompressed) or packed (compressed)
	
	public T Normal<T>()
	{
		return BaseNormal<T>.s_magic.Read( this );
	}
	public T Packed<T>()
	{
		return BasePacked<T>.s_magic.Read( this );
	}

	public void AlignToByte() { _source.ByteAlignRead(); }

	public void Read<T>( ref T destination ) where T : IBitSerializable
	{
		destination.Read( this );
	}

	public DataStream GetStream() { return _source; }
	public Int32 GetLength() { return _source.GetByteLength(); }

	// array interface: all functions return actual number of elements in use after call
	// TODO: longer-term, implement a nice FixedArray<T> and use that instead

	public Int32 ArrayNormal<T>( T[] buffer )
	{
		Int32 length = Normal<Int32>();
		if( !VerifyLength<T>( buffer, length ) ) { return 0; }
		for( Int32 i = 0; i < length; ++i ) { buffer[i] = Normal<T>(); }
		return length;
	}

	public Int32 ArrayPacked<T>( T[] buffer )
	{
		Int32 length = Packed<Int32>();
		if( !VerifyLength<T>( buffer, length ) ) { return 0; }
		for( Int32 i = 0; i < length; ++i) { buffer[i] = Packed<T>(); }
		return length;
	}

	public Int32 ArrayDiffNormal<T>( T[] buffer )
	{
		Int32 length = Normal<Int32>();
		if( !VerifyLength<T>( buffer, length ) ) { return 0; }
		ReadBitsByteAligned( _bitmap, length );
		for( Int32 i = 0; i < length; ++i )
		{
			if( _bitmap.GetBit(i) ) { buffer[i] = Normal<T>(); }
		}
		return length;
	}

	public Int32 ArrayDiffPacked<T>( T[] buffer )
	{
		Int32 length = Packed<Int32>();
		if( !VerifyLength<T>( buffer, length ) ) { return 0; }
		ReadBitsByteAligned( _bitmap, length );
		for( Int32 i = 0; i < length; ++i )
		{
			if( _bitmap.GetBit(i) ) { buffer[i] = Packed<T>(); }
		}
		return length;
	}


	// internal workhorse reading functions, for use by TypeSerializer

	public bool   Bit    () { return          _source.ReadBit(); }
	public byte   Byte   () { return (byte  )_source.ReadByte(); }
	public UInt32 ByteU32() { return (UInt32)_source.ReadByte(); }
	public UInt64 ByteU64() { return (UInt64)_source.ReadByte(); }

	public UInt16 UInt16()
	{ return (UInt16)
		( (ByteU32() << 0)
		| (ByteU32() << 8)
		);
	}

	public UInt32 UInt32() 
	{ return
		( (ByteU32() <<  0)
		| (ByteU32() <<  8)
		| (ByteU32() << 16)
		| (ByteU32() << 24)
		);
	}
	public UInt64 UInt64() 
	{ return
		( (ByteU64() <<  0)
		| (ByteU64() <<  8)
		| (ByteU64() << 16)
		| (ByteU64() << 24)
		| (ByteU64() << 32)
		| (ByteU64() << 40)
		| (ByteU64() << 48)
		| (ByteU64() << 56)
		);
	}

	// reads a variable length integer from the stream, up to size UInt64
	// this could take as many as 9 bytes (ie, it might actually be larger)
	public UInt64 UInt64Packed()
	{
		UInt64 header = ByteU64();
		if (header <= 240) return header;
		if (header <= 248) return    240 + ((header - 241) << 8) + ByteU64();
		if (header == 249) return 2288UL + (     ByteU64() << 8) + ByteU64();
		UInt64 res = ByteU64() | (ByteU64() << 8) | (ByteU64() << 16);
		int cmp = 2;
		int hdr = (int)(header - 247);
		while (hdr > ++cmp) res |= ByteU64() << (cmp << 3);
		return res;
	}


	// PRIVATE

	// reads a section of byte-aligned bits (if bit count is not divisible by 8, the source should have padded with zeros)
	bool ReadBitsByteAligned( byte[] buffer, Int32 bitCount )
	{
		int byteCount = DataStream.Div8Ceil( bitCount );
		if( buffer.Length < byteCount )
		{
			Log.Error( $"Insufficient capacity in buffer array! Need: {byteCount} have: {buffer.Length}" );
			return false;
		}
		_source.ReadByteArray( buffer, 0, byteCount );
		return true;
	}

	bool VerifyLength<T>( T[] buffer, Int32 neededLength )
	{
		if( buffer.Length < neededLength )
		{
			Log.Error( $"Insufficient capacity in buffer array! Need: {neededLength} have: {buffer.Length}" );
			return false;
		}
		return true;
	}
}

} // namespace Alpaca.Serialization
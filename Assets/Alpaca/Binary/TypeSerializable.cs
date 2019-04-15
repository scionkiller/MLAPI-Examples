using System;
using System.Runtime.InteropServices;
using NotSupportedException = System.NotSupportedException;

using UnityEngine;


namespace Alpaca.Serialization
{
	// Create an instance of this struct to allow for lockless and garbage free conversion between
	// floating point and unsigned types, which is useful in serialization.
	[StructLayout(LayoutKind.Explicit)]
	public struct BitwiseTypeConverter
	{
		[FieldOffset(0)] public  float f;
		[FieldOffset(0)] public UInt32 i;
		[FieldOffset(0)] public double d;
		[FieldOffset(0)] public UInt64 l;

		[FieldOffset(0)] public byte r;
		[FieldOffset(0)] public byte g;
		[FieldOffset(0)] public byte b;
		[FieldOffset(0)] public byte a;

		public   float ConvertToFloat  ( UInt32 data ) { i = data; return f; }
		public  double ConvertToDouble ( UInt64 data ) { l = data; return d; }
		public  UInt32 ConvertToUInt32 (  float data ) { f = data; return i; }
		public  UInt64 ConvertToUInt64 ( double data ) { d = data; return l; }
		public Color32 ConvertToColor32( UInt32 data ) { i = data; return new Color32( r, g, b, a ); }
		public  UInt32 ConvertToUInt32 (Color32 data )
		{
			r = data.r;
			g = data.g;
			b = data.b;
			a = data.a;
			return i;
		}
	}

	// some interface magic to do function specialization on a generic function call
	public interface INormal<T>
	{
		T    Read ( BitReader r );
		void Write( BitWriter w, T v );
	}

	public interface IPacked<T>
	{
		T    Read ( BitReader r );
		void Write( BitWriter w, T v );
	}

	public class BaseNormal<T> : INormal<T>
	{
		public static readonly INormal<T> s_magic = TypeSerializer.s_magic as INormal<T> ?? new BaseNormal<T>();

		// throw exception if Read/Write for a specific T is not defined
		T INormal<T>.Read( BitReader r )
		{
			throw new NotSupportedException();
		}
		void INormal<T>.Write( BitWriter w, T v )
		{
			throw new NotSupportedException();
		}
	}

	public class BasePacked<T> : IPacked<T>
	{
		public static readonly IPacked<T> s_magic = TypeSerializer.s_magic as IPacked<T> ?? new BasePacked<T>();
		T IPacked<T>.Read( BitReader r )
		{
			throw new NotSupportedException();
		}
		void IPacked<T>.Write( BitWriter w, T v )
		{
			throw new NotSupportedException();
		}
	}

	public class TypeSerializer
		: INormal<      bool>                  , IPacked<      bool>
		, INormal<      byte>, INormal<  sbyte>, IPacked<      byte>, IPacked<  sbyte>
		, INormal<      char>                  , IPacked<      char>
		, INormal<    UInt16>, INormal<  Int16>, IPacked<    UInt16>, IPacked<  Int16>
		, INormal<    UInt32>, INormal<  Int32>, IPacked<    UInt32>, IPacked<  Int32>
		, INormal<    UInt64>, INormal<  Int64>, IPacked<    UInt64>, IPacked<  Int64>
		, INormal<     float>, INormal< double>, IPacked<     float>, IPacked< double>
		, INormal<   Vector2>, INormal<Vector3>, IPacked<   Vector2>, IPacked<Vector3>
		, INormal<   Vector4>, INormal<    Ray>, IPacked<   Vector4>, IPacked<    Ray>
		, INormal<     Color>, INormal<Color32>, IPacked<     Color>, IPacked<Color32>
		, INormal<Quaternion>                  , IPacked<Quaternion>
	{
		public static TypeSerializer s_magic = new TypeSerializer();
		public static BitwiseTypeConverter s_convert;

		// NORMAL INTERFACES

		   bool INormal<   bool>.Read( BitReader r ) { return        r.Bit  (); }
		   byte INormal<   byte>.Read( BitReader r ) { return        r.Byte  (); }
		  sbyte INormal<  sbyte>.Read( BitReader r ) { return (sbyte)r.Byte  (); }
		   char INormal<   char>.Read( BitReader r ) { return ( char)r.UInt16(); }
		 UInt16 INormal< UInt16>.Read( BitReader r ) { return        r.UInt16(); }
		  Int16 INormal<  Int16>.Read( BitReader r ) { return (Int16)r.UInt16(); }
		 UInt32 INormal< UInt32>.Read( BitReader r ) { return        r.UInt32(); }
		  Int32 INormal<  Int32>.Read( BitReader r ) { return (Int32)r.UInt32(); }
		 UInt64 INormal< UInt64>.Read( BitReader r ) { return        r.UInt64(); }
		  Int64 INormal<  Int64>.Read( BitReader r ) { return (Int64)r.UInt64(); }

		  float INormal<  float>.Read( BitReader r ) { return s_convert.ConvertToFloat  ( r.UInt32() ); }
		 double INormal< double>.Read( BitReader r ) { return s_convert.ConvertToDouble ( r.UInt64() ); }
		Color32 INormal<Color32>.Read( BitReader r ) { return s_convert.ConvertToColor32( r.UInt32() ); }

		   Vector2 INormal<   Vector2>.Read( BitReader r ) { return new    Vector2( r.Normal<  float>(), r.Normal<  float>()                                       ); }
		   Vector3 INormal<   Vector3>.Read( BitReader r ) { return new    Vector3( r.Normal<  float>(), r.Normal<  float>(), r.Normal<float>()                    ); }
		   Vector4 INormal<   Vector4>.Read( BitReader r ) { return new    Vector4( r.Normal<  float>(), r.Normal<  float>(), r.Normal<float>(), r.Normal<float>() ); }
		     Color INormal<     Color>.Read( BitReader r ) { return new      Color( r.Normal<  float>(), r.Normal<  float>(), r.Normal<float>(), r.Normal<float>() ); }
		       Ray INormal<       Ray>.Read( BitReader r ) { return new        Ray( r.Normal<Vector3>(), r.Normal<Vector3>()                                       ); }
		Quaternion INormal<Quaternion>.Read( BitReader r ) { return new Quaternion( r.Normal<  float>(), r.Normal<  float>(), r.Normal<float>(), r.Normal<float>() ); }

		void INormal<   bool>.Write( BitWriter w,    bool v ) { w.Bit   (         v ); }
		void INormal<   byte>.Write( BitWriter w,    byte v ) { w.Byte  (         v ); }
		void INormal<  sbyte>.Write( BitWriter w,   sbyte v ) { w.Byte  ( (  byte)v ); }
		void INormal<   char>.Write( BitWriter w,    char v ) { w.UInt16( (  char)v ); }
		void INormal< UInt16>.Write( BitWriter w,  UInt16 v ) { w.UInt16(         v ); }
		void INormal<  Int16>.Write( BitWriter w,   Int16 v ) { w.UInt16( (UInt16)v ); }
		void INormal< UInt32>.Write( BitWriter w,  UInt32 v ) { w.UInt32(         v ); }
		void INormal<  Int32>.Write( BitWriter w,   Int32 v ) { w.UInt32( (UInt32)v ); }
		void INormal< UInt64>.Write( BitWriter w,  UInt64 v ) { w.UInt64(         v ); }
		void INormal<  Int64>.Write( BitWriter w,   Int64 v ) { w.UInt64( (UInt64)v ); }

		void INormal<  float>.Write( BitWriter w,   float v ) { w.Normal<UInt32>( s_convert.ConvertToUInt32( v ) ); }
		void INormal< double>.Write( BitWriter w,  double v ) { w.Normal<UInt64>( s_convert.ConvertToUInt64( v ) ); }
		void INormal<Color32>.Write( BitWriter w, Color32 v ) { w.Normal<UInt32>( s_convert.ConvertToUInt32( v ) ); }

		void INormal<   Vector2>.Write( BitWriter w,    Vector2 v ) { w.Normal<  float>( v.x      ); w.Normal<  float>( v.y         );                                                         } 
		void INormal<   Vector3>.Write( BitWriter w,    Vector3 v ) { w.Normal<  float>( v.x      ); w.Normal<  float>( v.y         ); w.Normal<float>( v.z );                             } 
		void INormal<   Vector4>.Write( BitWriter w,    Vector4 v ) { w.Normal<  float>( v.x      ); w.Normal<  float>( v.y         ); w.Normal<float>( v.z ); w.Normal<float>( v.w ); } 
		void INormal<     Color>.Write( BitWriter w,      Color v ) { w.Normal<  float>( v.r      ); w.Normal<  float>( v.g         ); w.Normal<float>( v.b ); w.Normal<float>( v.a ); } 
		void INormal<       Ray>.Write( BitWriter w,        Ray v ) { w.Normal<Vector3>( v.origin ); w.Normal<Vector3>( v.direction ); } 
		void INormal<Quaternion>.Write( BitWriter w, Quaternion v ) { w.Normal<  float>( v.x      ); w.Normal<  float>( v.y         ); w.Normal<float>( v.z ); w.Normal<float>( v.w ); } 

		// PACKED INTERFACES

		   bool IPacked<   bool>.Read( BitReader r ) { return        r.Bit   (); } // packing is not effective on sizes 1 byte or less so just don't pack
		   byte IPacked<   byte>.Read( BitReader r ) { return        r.Byte  (); } // packing is not effective on sizes 1 byte or less so just don't pack
		  sbyte IPacked<  sbyte>.Read( BitReader r ) { return (sbyte)r.Byte  (); } // packing is not effective on sizes 1 byte or less so just don't pack
		   char IPacked<   char>.Read( BitReader r ) { return (  char)                         r.UInt64Packed()  ; }
		 UInt16 IPacked< UInt16>.Read( BitReader r ) { return (UInt16)                         r.UInt64Packed()  ; }
		  Int16 IPacked<  Int16>.Read( BitReader r ) { return ( Int16)Arithmetic.ZigZagDecode( r.UInt64Packed() ); }
		 UInt32 IPacked< UInt32>.Read( BitReader r ) { return (UInt32)                         r.UInt64Packed()  ; }
		  Int32 IPacked<  Int32>.Read( BitReader r ) { return ( Int32)Arithmetic.ZigZagDecode( r.UInt64Packed() ); }
		 UInt64 IPacked< UInt64>.Read( BitReader r ) { return                                  r.UInt64Packed()  ; }
		  Int64 IPacked<  Int64>.Read( BitReader r ) { return         Arithmetic.ZigZagDecode( r.UInt64Packed() ); }

		  float IPacked<  float>.Read( BitReader r ) { return s_convert.ConvertToFloat  ( (UInt32)r.UInt64Packed() ); }
		 double IPacked< double>.Read( BitReader r ) { return s_convert.ConvertToDouble (         r.UInt64Packed() ); }
		Color32 IPacked<Color32>.Read( BitReader r ) { return s_convert.ConvertToColor32( (UInt32)r.UInt64Packed() ); }

		   Vector2 IPacked<   Vector2>.Read( BitReader r ) { return new    Vector2( r.Packed<  float>(), r.Packed<  float>()                                       ); }
		   Vector3 IPacked<   Vector3>.Read( BitReader r ) { return new    Vector3( r.Packed<  float>(), r.Packed<  float>(), r.Packed<float>()                    ); }
		   Vector4 IPacked<   Vector4>.Read( BitReader r ) { return new    Vector4( r.Packed<  float>(), r.Packed<  float>(), r.Packed<float>(), r.Packed<float>() ); }
		     Color IPacked<     Color>.Read( BitReader r ) { return new      Color( r.Packed<  float>(), r.Packed<  float>(), r.Packed<float>(), r.Packed<float>() ); }
		       Ray IPacked<       Ray>.Read( BitReader r ) { return new        Ray( r.Packed<Vector3>(), r.Packed<Vector3>()                                       ); }
		Quaternion IPacked<Quaternion>.Read( BitReader r ) { return new Quaternion( r.Packed<  float>(), r.Packed<  float>(), r.Packed<float>(), r.Packed<float>() ); }

		void IPacked<   bool>.Write( BitWriter w,    bool v ) { w.Bit   (         v ); }
		void IPacked<   byte>.Write( BitWriter w,    byte v ) { w.Byte  (         v ); }
		void IPacked<  sbyte>.Write( BitWriter w,   sbyte v ) { w.Byte  ( (  byte)v ); }
		void IPacked<   char>.Write( BitWriter w,    char v ) { w.UInt64Packed(                          v   ); }
		void IPacked< UInt16>.Write( BitWriter w,  UInt16 v ) { w.UInt64Packed(                          v   ); }
		void IPacked<  Int16>.Write( BitWriter w,   Int16 v ) { w.UInt64Packed( Arithmetic.ZigZagEncode( v ) ); }
		void IPacked< UInt32>.Write( BitWriter w,  UInt32 v ) { w.UInt64Packed(                          v   ); }
		void IPacked<  Int32>.Write( BitWriter w,   Int32 v ) { w.UInt64Packed( Arithmetic.ZigZagEncode( v ) ); }
		void IPacked< UInt64>.Write( BitWriter w,  UInt64 v ) { w.UInt64Packed(                          v   ); }
		void IPacked<  Int64>.Write( BitWriter w,   Int64 v ) { w.UInt64Packed( Arithmetic.ZigZagEncode( v ) ); }

		void IPacked<  float>.Write( BitWriter w,   float v ) { w.UInt64Packed( s_convert.ConvertToUInt32( v ) ); }
		void IPacked< double>.Write( BitWriter w,  double v ) { w.UInt64Packed( s_convert.ConvertToUInt64( v ) ); }
		void IPacked<Color32>.Write( BitWriter w, Color32 v ) { w.UInt64Packed( s_convert.ConvertToUInt32( v ) ); }

		void IPacked<   Vector2>.Write( BitWriter w,    Vector2 v ) { w.Packed<  float>( v.x      ); w.Packed<  float>( v.y         );                                                         } 
		void IPacked<   Vector3>.Write( BitWriter w,    Vector3 v ) { w.Packed<  float>( v.x      ); w.Packed<  float>( v.y         ); w.Packed<float>( v.z );                             } 
		void IPacked<   Vector4>.Write( BitWriter w,    Vector4 v ) { w.Packed<  float>( v.x      ); w.Packed<  float>( v.y         ); w.Packed<float>( v.z ); w.Packed<float>( v.w ); } 
		void IPacked<     Color>.Write( BitWriter w,      Color v ) { w.Packed<  float>( v.r      ); w.Packed<  float>( v.g         ); w.Packed<float>( v.b ); w.Packed<float>( v.a ); } 
		void IPacked<       Ray>.Write( BitWriter w,        Ray v ) { w.Packed<Vector3>( v.origin ); w.Packed<Vector3>( v.direction ); } 
		void IPacked<Quaternion>.Write( BitWriter w, Quaternion v ) { w.Packed<  float>( v.x      ); w.Packed<  float>( v.y         ); w.Packed<float>( v.z ); w.Packed<float>( v.w ); } 
	}
}
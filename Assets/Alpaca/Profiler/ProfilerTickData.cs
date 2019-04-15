using System.Collections.Generic;

using Alpaca.Serialization;
using InternalMessage = Alpaca.AlpacaConstant.InternalMessage;

using UInt32 = System.UInt32;
using UInt16 = System.UInt16;


namespace Alpaca.Profiling
{
	public enum TickType : byte
	{ Event
	, Receive
	, Send
	}

	public class ProfilerTick : IBitSerializable
	{
		TickType _type;
		UInt32 _frame;
		UInt32 _id;
		UInt32 _totalByteCount;
		List<TickEvent> _events;

		UInt32 GetByteCount() { return _totalByteCount; }

		public ProfilerTick( TickType type, UInt32 frame, UInt32 id )
		{
			_type = type;
			_frame = frame;
			_id = id;
			_totalByteCount = 0;
			_events = new List<TickEvent>();
		}
		
		public void RecordEvent( UInt32 byteCount, ChannelIndex channel, InternalMessage message )
		{
			_events.Add( new TickEvent( byteCount, channel, message ) );
			_totalByteCount += byteCount;
		}

		public void Read( BitReader reader )
		{
			_type = (TickType)reader.Normal<byte>();
			_frame = reader.Packed<UInt32>();
			_id = reader.Packed<UInt32>();
			_totalByteCount = 0;

			UInt16 count = reader.Packed<UInt16>();
			_events = new List<TickEvent>(count);
			for( int i = 0; i < count; ++i )
			{
				TickEvent e = new TickEvent();
				e.Read( reader );
				_events.Add( e );
				_totalByteCount += e.GetByteCount();
			}
		}

		public void Write( BitWriter writer )
		{
			writer.Normal<byte>( (byte)_type );
			writer.Packed<UInt32>( _frame );
			writer.Packed<UInt32>( _id );

			writer.Packed<UInt16>( (UInt16)_events.Count );
			for( int i = 0; i < _events.Count; ++i )
			{
				_events[i].Write( writer );
			}
		}
	}

	public struct TickEvent : IBitSerializable
	{
		UInt32 _byteCount;
		ChannelIndex _channel;
		InternalMessage _message;

		public UInt32 GetByteCount() { return _byteCount; }

		public TickEvent( UInt32 byteCount, ChannelIndex channel, InternalMessage message )
		{
			_byteCount = byteCount;
			_channel = channel;
			_message = message;
		}

		public void Read( BitReader reader )
		{
			_byteCount = reader.Packed<UInt32>();
			_channel.Read(reader);
			_message = (InternalMessage)reader.Packed<UInt32>();
		}

		public void Write( BitWriter writer )
		{
			writer.Packed<UInt32>(_byteCount);
			_channel.Write( writer );
			writer.Packed<UInt32>( (UInt32)_message );	
		}
	}
}
using System;
using System.Runtime.InteropServices;


namespace Alpaca
{
    // Represents the id of a Node, which is a specific server or client
	[StructLayout(LayoutKind.Explicit)]
    public struct NodeId
    {
		[Flags]
		enum Flags : byte
		{
			  None = 0x00
			, Server = 0x01
		}

		public static readonly NodeId SERVER_NODE_ID = new NodeId( Flags.Server, 0, 0 );

		[FieldOffset(0)]
		byte _flags;  // for now used only to determine if the node is a server

		[FieldOffset(1)]
		byte _hostId;  // hostId this client is on ?!?

		[FieldOffset(2)] 
		UInt16 _connectionId; // id this client is assigned to

		[FieldOffset(0)]
		UInt32 _id; // the above data packed into 32 bits


        public bool IsServer()
        {
            return (flags & Flags.Server) != 0;
        }

		public NodeId( byte flags, byte hostId, UInt16 connectionId )
		{
			_id = 0; // this is dumb, but harmless and it silences a compile warning
			_flags = flags;
			_hostId = hostId;
			_connectionId = connectionId;
		}

        public override int GetHashCode()
        {
            return (int)_id;
        }

		public override bool Equals( object obj )
		{
			return obj is NodeId && this == (NodeId)obj;
		}

        public static bool operator ==(NodeId A, NodeId B)
        {
			return (A._id == B._id);
        }

        public static bool operator !=(NodeId A, NodeId B)
        {
            return !(A == B);
        }
    }
}
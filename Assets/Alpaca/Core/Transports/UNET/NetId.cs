using System;
using System.Runtime.InteropServices;


namespace Alpaca
{
    // Represents the id of a Node, which is a specific server or client
	[StructLayout(LayoutKind.Explicit)]
    public struct NodeId
    {
		public static readonly NodeId SERVER_NODE_ID = new NodeId( true, 0, 0 );

		[FieldOffset(0)]
		public byte flags;  // for now used only to determine if the node is a server

		[FieldOffset(1)]
		public byte hostId;  // hostId this client is on ?!?

		[FieldOffset(2)] 
		public UInt16 connectionId; // id this client is assigned to

		[FieldOffset(0)]
		public UInt32 id; // the above data packed into 32 bits


        public bool IsServer()
        {
            return (flags & 0x1) != 0;
        }

		// TODO: cozeroff, finish this damn calss
        /// <summary>
        /// Initializes a new instance of the netId struct from transport values
        /// </summary>
        /// <param name="hostId">Host identifier.</param>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="isServer">If set to <c>true</c> is isServer.</param>
        public NetId(byte hostId, ushort connectionId, bool isServer)
        {
            HostId = hostId;
            ConnectionId = connectionId;
			Meta = isServer ? (byte)1 : (byte)0;
        }
        /// <summary>
        /// Initializes a new instance of the netId struct from a clientId
        /// </summary>
        /// <param name="clientId">Client identifier.</param>
        public NetId(uint clientId)
        {
            HostId = (byte)(clientId & 0xFF);
            ConnectionId = (ushort)((byte)((clientId >> 8) & 0xFF) | (ushort)(((clientId >> 16) & 0xFF) << 8));
            Meta = (byte)((clientId >> 24) & 0xFF);
        }

        public override int GetHashCode()
        {
            return (int)id;
        }

        public static bool operator ==(NodeId A, NodeId B)
        {
			return (A.id == B.id);
        }

        public static bool operator !=(NodeId A, NodeId B)
        {
            return !(A == B);
        }
    }
}
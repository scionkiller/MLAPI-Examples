using Alpaca.Profiling;
using Alpaca.Serialization;

namespace Alpaca.Internal
{
	/*
	internal static partial class InternalMessageHandler
	{
		internal static void Send(byte messageType, string channelName, BitStream messageStream, SecuritySendFlags flags)
		{
			AlpacaNetwork network = AlpacaNetwork.GetSingleton();

			bool encrypted = ((flags & SecuritySendFlags.Encrypted) == SecuritySendFlags.Encrypted) && network.config.EnableEncryption;
			bool authenticated = ((flags & SecuritySendFlags.Authenticated) == SecuritySendFlags.Authenticated) && network.config.EnableEncryption;

			if( authenticated || encrypted )
			{
				for( int i = 0; i < network._connectedClients.GetCount(); i++)
				{
					Send( network._connectedClients.GetAt(i).GetId(), messageType, channelName, messageStream, flags );
				}
			}
			else
			{
				messageStream.PadStream();

				using (BitStream stream = MessageManager.WrapMessage(messageType, 0, messageStream, flags))
				{
					NetworkProfiler.StartEvent(TickType.Send, (uint)stream.Length, channelName, AlpacaConstant.INTERNAL_MESSAGE_NAME[messageType]);
					for (int i = 0; i < network._connectedClients.GetCount(); i++)
					{
						Client c = network._connectedClients.GetAt(i);
						if( network.IsServer && c.GetId() == network.ServerClientId ) continue;
						byte error;
						network.config.NetworkTransport.QueueMessageForSending( c.GetId(), stream.GetBuffer(), (int)stream.Length, MessageManager.channels[channelName], false, out error );
					}
					NetworkProfiler.EndEvent();
				}
			}
		}

		internal static void Send(byte messageType, string channelName, uint clientIdToIgnore, BitStream messageStream, SecuritySendFlags flags)
		{
			AlpacaNetwork network = AlpacaNetwork.GetSingleton();

			bool encrypted = ((flags & SecuritySendFlags.Encrypted) == SecuritySendFlags.Encrypted) && network.config.EnableEncryption;
			bool authenticated = ((flags & SecuritySendFlags.Authenticated) == SecuritySendFlags.Authenticated) && network.config.EnableEncryption;

			if (encrypted || authenticated)
			{
				for( int i = 0; i < network._connectedClients.GetCount(); ++i )
				{
					Client c = network._connectedClients.GetAt(i);
					if( c.GetId() != clientIdToIgnore )
					{
						Send( c.GetId(), messageType, channelName, messageStream, flags );
					}
				}
			}
			else
			{
				messageStream.PadStream();

				using (BitStream stream = MessageManager.WrapMessage(messageType, 0, messageStream, flags))
				{
					NetworkProfiler.StartEvent(TickType.Send, (uint)stream.Length, channelName, AlpacaConstant.INTERNAL_MESSAGE_NAME[messageType]);
					for (int i = 0; i < network._connectedClients.GetCount(); i++)
					{
						Client c = network._connectedClients.GetAt(i);
						if( c.GetId() == clientIdToIgnore || (network.IsServer && c.GetId() == network.ServerClientId) )
							continue;

						byte error;
						network.config.NetworkTransport.QueueMessageForSending( c.GetId(), stream.GetBuffer(), (int)stream.Length, MessageManager.channels[channelName], false, out error);
					}
					NetworkProfiler.EndEvent();
				}
			}
		}
	}
	*/
}

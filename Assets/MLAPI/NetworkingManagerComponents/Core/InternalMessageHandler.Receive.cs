using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Alpaca.Components;
#if !DISABLE_CRYPTOGRAPHY
using Alpaca.Cryptography;
#endif
using Alpaca.Data;
using Alpaca.Logging;
using Alpaca.Serialization;
using UnityEngine;

namespace Alpaca.Internal
{
    internal static partial class InternalMessageHandler
    {
#if !DISABLE_CRYPTOGRAPHY
        // Runs on client
        internal static void HandleHailRequest(uint clientId, Stream stream, int channelId)
        {
			NetworkingManager network = NetworkingManager.GetSingleton();
            X509Certificate2 certificate = null;
            byte[] serverDiffieHellmanPublicPart = null;
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                if (network.config.EnableEncryption)
                {
                    // Read the certificate
                    if (network.config.SignKeyExchange)
                    {
                        // Allocation justification: This runs on client and only once, at initial connection
                        certificate = new X509Certificate2(reader.ReadByteArray());
                        if (CryptographyHelper.VerifyCertificate(certificate, network.ConnectedHostname))
                        {
                            // The certificate is not valid :(
                            // Man in the middle.
                            if (LogHelper.CurrentLogLevel <= LogLevel.Normal) if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Invalid certificate. Disconnecting");
                            network.StopClient();
                            return;
                        }
                        else
                        {
                            network.config.ServerX509Certificate = certificate;
                        }
                    }

                    // Read the ECDH
                    // Allocation justification: This runs on client and only once, at initial connection
                    serverDiffieHellmanPublicPart = reader.ReadByteArray();
                    
                    // Verify the key exchange
                    if (network.config.SignKeyExchange)
                    {
                        byte[] serverDiffieHellmanPublicPartSignature = reader.ReadByteArray();

                        RSACryptoServiceProvider rsa = certificate.PublicKey.Key as RSACryptoServiceProvider;

                        if (rsa != null)
                        {
                            using (SHA256Managed sha = new SHA256Managed())
                            {
                                if (!rsa.VerifyData(serverDiffieHellmanPublicPart, sha, serverDiffieHellmanPublicPartSignature))
                                {
                                    if (LogHelper.CurrentLogLevel <= LogLevel.Normal) if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Invalid signature. Disconnecting");
                                    network.StopClient();
                                    return;
                                }   
                            }
                        }
                    }
                }
            }

            using (PooledBitStream outStream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(outStream))
                {
                    if (network.config.EnableEncryption)
                    {
                        // Create a ECDH key
                        EllipticDiffieHellman diffieHellman = new EllipticDiffieHellman(EllipticDiffieHellman.DEFAULT_CURVE, EllipticDiffieHellman.DEFAULT_GENERATOR, EllipticDiffieHellman.DEFAULT_ORDER);
                        network.clientAesKey = diffieHellman.GetSharedSecret(serverDiffieHellmanPublicPart);
                        byte[] diffieHellmanPublicKey = diffieHellman.GetPublicKey();
                        writer.WriteByteArray(diffieHellmanPublicKey);
                        if (network.config.SignKeyExchange)
                        {
                            RSACryptoServiceProvider rsa = certificate.PublicKey.Key as RSACryptoServiceProvider;

                            if (rsa != null)
                            {
                                using (SHA256CryptoServiceProvider sha = new SHA256CryptoServiceProvider())
                                {
                                    writer.WriteByteArray(rsa.Encrypt(sha.ComputeHash(diffieHellmanPublicKey), false));   
                                }
                            }
                            else
                            {
                                throw new CryptographicException("[Alpaca] Only RSA certificates are supported. No valid RSA key was found");
                            }
                        }
                    }
                }
                // Send HailResponse
                InternalMessageHandler.Send(NetworkingManager.GetSingleton().ServerClientId, Constants.ALPACA_CERTIFICATE_HAIL_RESPONSE, "ALPACA_INTERNAL", outStream, SecuritySendFlags.None, true);
            }
        }

        // Ran on server
        internal static void HandleHailResponse(uint clientId, Stream stream, int channelId)
        {
			NetworkingManager network = NetworkingManager.GetSingleton();
			PendingClient p = network._pendingClients.Get(clientId);
            if(  p == null
			  || p.ConnectionState != PendingClient.State.PendingHail
			  )
			{
				return;
			}

            if( !network.config.EnableEncryption )
			{
				return;
			}

            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                if( p.KeyExchange != null )
                {
                    byte[] diffieHellmanPublic = reader.ReadByteArray();
                    p.AesKey = p.KeyExchange.GetSharedSecret(diffieHellmanPublic);
                    if (network.config.SignKeyExchange)
                    {
                        byte[] diffieHellmanPublicSignature = reader.ReadByteArray();
                        X509Certificate2 certificate = network.config.ServerX509Certificate;
                        RSACryptoServiceProvider rsa = certificate.PrivateKey as RSACryptoServiceProvider;

                        if (rsa != null)
                        {
                            using (SHA256Managed sha = new SHA256Managed())
                            {
                                byte[] clientHash = rsa.Decrypt(diffieHellmanPublicSignature, false);
                                byte[] serverHash = sha.ComputeHash(diffieHellmanPublic);
                                
                                if (!CryptographyHelper.ConstTimeArrayEqual(clientHash, serverHash))
                                {
                                    //Man in the middle.
                                    if (LogHelper.CurrentLogLevel <= LogLevel.Normal) if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Signature doesnt match for the key exchange public part. Disconnecting");
                                    network.DisconnectClient(clientId);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            throw new CryptographicException("[Alpaca] Only RSA certificates are supported. No valid RSA key was found");
                        }
                    }
                }
            }

            p.ConnectionState = PendingClient.State.PendingConnection;
            p.KeyExchange = null;
            
            // Send greetings, they have passed all the handshakes
            using (PooledBitStream outStream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(outStream))
                {
                    writer.WriteInt64Packed(DateTime.Now.Ticks); // This serves no purpose.
                }
                InternalMessageHandler.Send(clientId, Constants.ALPACA_GREETINGS, "ALPACA_INTERNAL", outStream, SecuritySendFlags.None, true);
            }
        }

        internal static void HandleGreetings(uint clientId, Stream stream, int channelId)
        {
            // Server greeted us, we can now initiate our request to connect.
            NetworkingManager.GetSingleton().SendConnectionRequest();
        }
#endif

        internal static void HandleConnectionRequest(uint clientId, Stream stream, int channelId)
        {
			NetworkingManager network = NetworkingManager.GetSingleton();

            using( PooledBitReader reader = PooledBitReader.Get(stream) )
            {
                ulong configHash = reader.ReadUInt64Packed();
				// TODO: find out why this config comparison fails when built on different machines, and restore this safety check
                // if(  !netManager.config.CompareConfig(configHash) )
                // {
                //     if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("NetworkConfiguration mismatch. The configuration between the server and client does not match");
                //     netManager.DisconnectClient(clientId);
                //     return;
                // }

                if( network.config.ConnectionApproval )
                {
                    byte[] connectionBuffer = reader.ReadByteArray();
                    network.ConnectionApprovalCallback(connectionBuffer, clientId, network.HandleApproval);
                }
                else
                {
                    network.HandleApproval(clientId, true);
                }
            }
        }

        internal static void HandleConnectionApproved(uint clientId, Stream stream, int channelId)
        {
			NetworkingManager network = NetworkingManager.GetSingleton();

            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                network.LocalClientId = reader.ReadUInt32Packed();

                float netTime = reader.ReadSinglePacked();
                int remoteStamp = reader.ReadInt32Packed();
                int msDelay = network.config.NetworkTransport.GetRemoteDelayTimeMS(clientId, remoteStamp, out byte error);
                network.NetworkTime = netTime + (msDelay / 1000f);
				network._connectedClients.Add( network.LocalClientId, new NetworkedClient() { ClientId = network.LocalClientId } );

				int objectCount = reader.ReadInt32Packed();
				for( int i = 0; i < objectCount; i++ )
				{
					AddObjectInternal( network, reader, stream );
				}

                network.IsConnectedClient = true;
                if( network.OnClientConnectedCallback != null )
				{
                    network.OnClientConnectedCallback.Invoke(network.LocalClientId);
				}
            }
        }

        internal static void HandleAddObject(uint clientId, Stream stream, int channelId)
        {
            using( PooledBitReader reader = PooledBitReader.Get(stream) )
            {
				AddObjectInternal( NetworkingManager.GetSingleton(), reader, stream );
            }
        }

        internal static void HandleClientDisconnect(uint clientId, Stream stream, int channelId)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint disconnectedClientId = reader.ReadUInt32Packed();
                NetworkingManager.GetSingleton().OnClientDisconnect(disconnectedClientId);
            }
        }

        internal static void HandleDestroyObject(uint clientId, Stream stream, int channelId)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint netId = reader.ReadUInt32Packed();
                SpawnManager.OnDestroyObject(netId, true);
            }
        }

        internal static void HandleSpawnPoolObject(uint clientId, Stream stream, int channelId)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint netId = reader.ReadUInt32Packed();

                float xPos = reader.ReadSinglePacked();
                float yPos = reader.ReadSinglePacked();
                float zPos = reader.ReadSinglePacked();

                float xRot = reader.ReadSinglePacked();
                float yRot = reader.ReadSinglePacked();
                float zRot = reader.ReadSinglePacked();

                SpawnManager.SpawnedObjects[netId].transform.position = new Vector3(xPos, yPos, zPos);
                SpawnManager.SpawnedObjects[netId].transform.rotation = Quaternion.Euler(xRot, yRot, zRot);
                SpawnManager.SpawnedObjects[netId].gameObject.SetActive(true);
            }
        }

        internal static void HandleDestroyPoolObject(uint clientId, Stream stream, int channelId)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint netId = reader.ReadUInt32Packed();
                SpawnManager.SpawnedObjects[netId].gameObject.SetActive(false);
            }
        }

        internal static void HandleChangeOwner(uint clientId, Stream stream, int channelId)
        {
			NetworkingManager network = NetworkingManager.GetSingleton();

            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint netId = reader.ReadUInt32Packed();
                uint ownerClientId = reader.ReadUInt32Packed();
                if( SpawnManager.SpawnedObjects[netId].OwnerClientId == network.LocalClientId)
                {
                    //We are current owner.
                    SpawnManager.SpawnedObjects[netId].InvokeBehaviourOnLostOwnership();
                }
                if (ownerClientId == network.LocalClientId)
                {
                    //We are new owner.
                    SpawnManager.SpawnedObjects[netId].InvokeBehaviourOnGainedOwnership();
                }
                SpawnManager.SpawnedObjects[netId].OwnerClientId = ownerClientId;
            }
        }

        internal static void HandleAddObjects(uint clientId, Stream stream, int channelId)
        {
            using( PooledBitReader reader = PooledBitReader.Get(stream) )
            {
				ushort objectCount = reader.ReadUInt16Packed();
				for (int i = 0; i < objectCount; i++)
				{
					AddObjectInternal( NetworkingManager.GetSingleton(), reader, stream );
				}
            }
        }

        internal static void HandleTimeSync(uint clientId, Stream stream, int channelId)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                float netTime = reader.ReadSinglePacked();
                int timestamp = reader.ReadInt32Packed();

                int msDelay = NetworkingManager.GetSingleton().config.NetworkTransport.GetRemoteDelayTimeMS(clientId, timestamp, out byte error);
                NetworkingManager.GetSingleton().NetworkTime = netTime + (msDelay / 1000f);
            }
        }

        internal static void HandleNetworkedVarDelta(uint clientId, Stream stream, int channelId)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint netId = reader.ReadUInt32Packed();
                ushort orderIndex = reader.ReadUInt16Packed();

                if (SpawnManager.SpawnedObjects.ContainsKey(netId))
                {
                    NetworkedBehaviour instance = SpawnManager.SpawnedObjects[netId].GetBehaviourAtOrderIndex(orderIndex);
                    if (instance == null)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("NetworkedVar message recieved for a non existant behaviour");
                        return;
                    }
                    NetworkedBehaviour.HandleNetworkedVarDeltas(instance.networkedVarFields, stream, clientId, instance);
                }
                else
                {
                    if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("NetworkedVar message recieved for a non existant object with id: " + netId);
                    return;
                }
            }
        }

        internal static void HandleNetworkedVarUpdate(uint clientId, Stream stream, int channelId)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint netId = reader.ReadUInt32Packed();
                ushort orderIndex = reader.ReadUInt16Packed();

                if (SpawnManager.SpawnedObjects.ContainsKey(netId))
                {
                    NetworkedBehaviour instance = SpawnManager.SpawnedObjects[netId].GetBehaviourAtOrderIndex(orderIndex);
                    if (instance == null)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("NetworkedVar message recieved for a non existant behaviour");
                        return;
                    }
                    NetworkedBehaviour.HandleNetworkedVarUpdate(instance.networkedVarFields, stream, clientId, instance);
                }
                else
                {
                    if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("NetworkedVar message recieved for a non existant object with id: " + netId);
                    return;
                }
            }
        }
        
        internal static void HandleServerRPC(uint clientId, Stream stream, int channelId)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint networkId = reader.ReadUInt32Packed();
                ushort behaviourId = reader.ReadUInt16Packed();
                ulong hash = reader.ReadUInt64Packed();

                if (SpawnManager.SpawnedObjects.ContainsKey(networkId)) 
                { 
                    NetworkedBehaviour behaviour = SpawnManager.SpawnedObjects[networkId].GetBehaviourAtOrderIndex(behaviourId);
                    if (behaviour != null)
                    {
                        behaviour.OnRemoteServerRPC(hash, clientId, stream);
                    }
                }
            }
        }
        
        internal static void HandleServerRPCRequest(uint clientId, Stream stream, int channelId, SecuritySendFlags security)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint networkId = reader.ReadUInt32Packed();
                ushort behaviourId = reader.ReadUInt16Packed();
                ulong hash = reader.ReadUInt64Packed();
                ulong responseId = reader.ReadUInt64Packed();

                if (SpawnManager.SpawnedObjects.ContainsKey(networkId)) 
                { 
                    NetworkedBehaviour behaviour = SpawnManager.SpawnedObjects[networkId].GetBehaviourAtOrderIndex(behaviourId);
                    if (behaviour != null)
                    {
                        object result = behaviour.OnRemoteServerRPC(hash, clientId, stream);

                        using (PooledBitStream responseStream = PooledBitStream.Get())
                        {
                            using (PooledBitWriter responseWriter = PooledBitWriter.Get(responseStream))
                            {
                                responseWriter.WriteUInt64Packed(responseId);
                                responseWriter.WriteObjectPacked(result);
                            }
                            
                            InternalMessageHandler.Send(clientId, Constants.ALPACA_SERVER_RPC_RESPONSE, MessageManager.reverseChannels[channelId], responseStream, security);
                        }
                    }
                }
            }
        }
        
        internal static void HandleServerRPCResponse(uint clientId, Stream stream, int channelId)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                ulong responseId = reader.ReadUInt64Packed();

                if (ResponseMessageManager.ContainsKey(responseId))
                {
                    RpcResponseBase responseBase = ResponseMessageManager.GetByKey(responseId);

                    if (responseBase.ClientId != clientId) return;
                    
                    ResponseMessageManager.Remove(responseId);
                    
                    responseBase.IsDone = true;
                    responseBase.Result = reader.ReadObjectPacked(responseBase.Type);
                    responseBase.IsSuccessful = true;
                }
            }
        }
        
        internal static void HandleClientRPC(uint clientId, Stream stream, int channelId)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint networkId = reader.ReadUInt32Packed();
                ushort behaviourId = reader.ReadUInt16Packed();
                ulong hash = reader.ReadUInt64Packed();
                
                if (SpawnManager.SpawnedObjects.ContainsKey(networkId)) 
                {
                    NetworkedBehaviour behaviour = SpawnManager.SpawnedObjects[networkId].GetBehaviourAtOrderIndex(behaviourId);
                    if (behaviour != null)
                    {
                        behaviour.OnRemoteClientRPC(hash, clientId, stream);
                    }
                }
            }
        }
        
        internal static void HandleClientRPCRequest(uint clientId, Stream stream, int channelId, SecuritySendFlags security)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint networkId = reader.ReadUInt32Packed();
                ushort behaviourId = reader.ReadUInt16Packed();
                ulong hash = reader.ReadUInt64Packed();
                ulong responseId = reader.ReadUInt64Packed();
                
                if (SpawnManager.SpawnedObjects.ContainsKey(networkId)) 
                {
                    NetworkedBehaviour behaviour = SpawnManager.SpawnedObjects[networkId].GetBehaviourAtOrderIndex(behaviourId);
                    if (behaviour != null)
                    {
                        object result = behaviour.OnRemoteClientRPC(hash, clientId, stream);
                        
                        using (PooledBitStream responseStream = PooledBitStream.Get())
                        {
                            using (PooledBitWriter responseWriter = PooledBitWriter.Get(responseStream))
                            {
                                responseWriter.WriteUInt64Packed(responseId);
                                responseWriter.WriteObjectPacked(result);
                            }
                            
                            InternalMessageHandler.Send(clientId, Constants.ALPACA_CLIENT_RPC_RESPONSE, MessageManager.reverseChannels[channelId], responseStream, security);
                        }
                    }
                }
            }
        }
        
        internal static void HandleClientRPCResponse(uint clientId, Stream stream, int channelId)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                ulong responseId = reader.ReadUInt64Packed();

                if (ResponseMessageManager.ContainsKey(responseId))
                {
                    RpcResponseBase responseBase = ResponseMessageManager.GetByKey(responseId);
                    
                    if (responseBase.ClientId != clientId) return;
                    
                    ResponseMessageManager.Remove(responseId);
                    
                    responseBase.IsDone = true;
                    responseBase.Result = reader.ReadObjectPacked(responseBase.Type);
                    responseBase.IsSuccessful = true;
                }
            }
        }
        
        internal static void HandleCustomMessage(uint clientId, Stream stream, int channelId)
        {
            NetworkingManager.GetSingleton().InvokeOnIncomingCustomMessage(clientId, stream);
        }

		// TODO: having both reader and stream is a bit redundant, but this calls other functions that take a Stream
		static void AddObjectInternal( NetworkingManager network, PooledBitReader reader, Stream stream )
		{
			bool isPlayerObject = reader.ReadBool();
			uint networkId = reader.ReadUInt32Packed();
			uint ownerId = reader.ReadUInt32Packed();
			ulong prefabHash = reader.ReadUInt64Packed();

			bool destroyWithScene = reader.ReadBool();
			bool sceneDelayedSpawn = reader.ReadBool();
			uint sceneSpawnedInIndex = reader.ReadUInt32Packed();

			float xPos = reader.ReadSinglePacked();
			float yPos = reader.ReadSinglePacked();
			float zPos = reader.ReadSinglePacked();

			float xRot = reader.ReadSinglePacked();
			float yRot = reader.ReadSinglePacked();
			float zRot = reader.ReadSinglePacked();

			bool hasPayload = reader.ReadBool();
			int payLoadLength = hasPayload ? reader.ReadInt32Packed() : 0;

			NetworkedObject netObject = SpawnManager.CreateSpawnedObject( SpawnManager.GetNetworkedPrefabIndexOfHash(prefabHash), networkId, ownerId, isPlayerObject, new Vector3(xPos, yPos, zPos), Quaternion.Euler(xRot, yRot, zRot), true, stream, hasPayload, payLoadLength, true);

			if( isPlayerObject )
			{
				network._connectedClients.Add(ownerId, new NetworkedClient() { ClientId = ownerId, PlayerObject = netObject });

				if( network.OnAvatarSpawn != null
				  && ownerId == network.LocalClientId
				  )
				{
					network.OnAvatarSpawn.Invoke( netObject.gameObject );
				}
			}
		}
    }
}

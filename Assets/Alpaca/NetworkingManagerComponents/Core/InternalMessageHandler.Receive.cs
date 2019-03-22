using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Alpaca.Components;
#if !DISABLE_CRYPTOGRAPHY
using Alpaca.Cryptography;
#endif
using Alpaca.Data;

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
			AlpacaNetwork network = AlpacaNetwork.GetSingleton();
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
                            LogHelper.LogError("Invalid certificate. Possible man in the middle attack? Disconnecting");
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
                InternalMessageHandler.Send(AlpacaNetwork.GetSingleton().ServerClientId, AlpacaConstant.ALPACA_CERTIFICATE_HAIL_RESPONSE, "ALPACA_INTERNAL", outStream, SecuritySendFlags.None, true);
            }
        }

        // Ran on server
        internal static void HandleHailResponse(uint clientId, Stream stream, int channelId)
        {
			/*
			AlpacaNetwork network = AlpacaNetwork.GetSingleton();
			PendingClient p = network._pendingClients.Get(clientId);
            if(  p == null
			  || p.GetState() != PendingClient.State.PendingHail
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
                InternalMessageHandler.Send(clientId, AlpacaConstant.ALPACA_GREETINGS, "ALPACA_INTERNAL", outStream, SecuritySendFlags.None, true);
            }
			*/
        }

        internal static void HandleGreetings(uint clientId, Stream stream, int channelId)
        {
            // Server greeted us, we can now initiate our request to connect.
            AlpacaNetwork.GetSingleton().SendConnectionRequest();
        }
#endif

        internal static void HandleConnectionRequest(uint clientId, Stream stream, int channelId)
        {
			AlpacaNetwork network = AlpacaNetwork.GetSingleton();

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

                 network.HandleApproval(clientId, true);
            }
        }

        internal static void HandleConnectionApprovedClient(uint clientId, Stream stream, int channelId)
        {
			AlpacaNetwork network = AlpacaNetwork.GetSingleton();

            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                network.LocalClientId = reader.ReadUInt32Packed();

                float netTime = reader.ReadSinglePacked();
                int remoteStamp = reader.ReadInt32Packed();
                int msDelay = network.config.NetworkTransport.GetRemoteDelayTimeMS(clientId, remoteStamp, out byte error);
                network.NetworkTime = netTime + (msDelay / 1000f);
				// TODO: cozeroff
				//network._connectedClients.Add( network.LocalClientId, new Client() { ClientId = network.LocalClientId } );

				int objectCount = reader.ReadInt32Packed();
				for( int i = 0; i < objectCount; i++ )
				{
					SpawnEntityClient( network, reader );
				}

                network.IsConnectedClient = true;
                if( network.OnClientConnectedCallback != null )
				{
                    network.OnClientConnectedCallback.Invoke(network.LocalClientId);
				}
            }
        }

        internal static void HandleAddObjectClient(uint clientId, Stream stream, int channelId)
        {
            using( PooledBitReader reader = PooledBitReader.Get(stream) )
            {
				SpawnEntityClient( AlpacaNetwork.GetSingleton(), reader );
            }
        }

		internal static void HandleAddObjectsClient(uint clientId, Stream stream, int channelId)
        {
            using( PooledBitReader reader = PooledBitReader.Get(stream) )
            {
				ushort objectCount = reader.ReadUInt16Packed();
				for (int i = 0; i < objectCount; i++)
				{
					SpawnEntityClient( AlpacaNetwork.GetSingleton(), reader );
				}
            }
        }

        internal static void HandleClientDisconnectClient(uint clientId, Stream stream, int channelId)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint disconnectedClientId = reader.ReadUInt32Packed();
                AlpacaNetwork.GetSingleton().OnClientDisconnectClient( disconnectedClientId );
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
			/*
			AlpacaNetwork network = AlpacaNetwork.GetSingleton();

            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint entityId = reader.ReadUInt32Packed();
                uint ownerClientId = reader.ReadUInt32Packed();
                _entity[entityId].SetOwnerClientId( ownerClientId );
            }
			*/
        }

        internal static void HandleTimeSync(uint clientId, Stream stream, int channelId)
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                float netTime = reader.ReadSinglePacked();
                int timestamp = reader.ReadInt32Packed();

                int msDelay = AlpacaNetwork.GetSingleton().config.NetworkTransport.GetRemoteDelayTimeMS(clientId, timestamp, out byte error);
                AlpacaNetwork.GetSingleton().NetworkTime = netTime + (msDelay / 1000f);
            }
        }

        internal static void HandleNetworkedVarDelta(uint clientId, Stream stream, int channelId)
        {
			/*
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint netId = reader.ReadUInt32Packed();
                ushort orderIndex = reader.ReadUInt16Packed();

                if (SpawnManager.SpawnedObjects.ContainsKey(netId))
                {
                    Conduct instance = SpawnManager.SpawnedObjects[netId].GetBehaviourAtOrderIndex(orderIndex);
                    if (instance == null)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("NetworkedVar message recieved for a non existant behaviour");
                        return;
                    }
                    Conduct.HandleNetworkedVarDeltas(instance.networkedVarFields, reader, clientId, instance);
                }
                else
                {
                    if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("NetworkedVar message recieved for a non existant object with id: " + netId);
                    return;
                }
            }
			*/
        }

        internal static void HandleNetworkedVarUpdate(uint clientId, Stream stream, int channelId)
        {
            /*
			using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint netId = reader.ReadUInt32Packed();
                ushort orderIndex = reader.ReadUInt16Packed();

                if (SpawnManager.SpawnedObjects.ContainsKey(netId))
                {
                    Conduct instance = SpawnManager.SpawnedObjects[netId].GetBehaviourAtOrderIndex(orderIndex);
                    if (instance == null)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("NetworkedVar message recieved for a non existant behaviour");
                        return;
                    }
                    Conduct.HandleNetworkedVarUpdate(instance.networkedVarFields, stream, clientId, instance);
                }
                else
                {
                    if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("NetworkedVar message recieved for a non existant object with id: " + netId);
                    return;
                }
            }
			*/
        }
        
        internal static void HandleServerRPC(uint clientId, Stream stream, int channelId)
        {
			/*
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint networkId = reader.ReadUInt32Packed();
                ushort behaviourId = reader.ReadUInt16Packed();
                ulong hash = reader.ReadUInt64Packed();

                if (SpawnManager.SpawnedObjects.ContainsKey(networkId)) 
                { 
                    Conduct behaviour = SpawnManager.SpawnedObjects[networkId].GetBehaviourAtOrderIndex(behaviourId);
                    if (behaviour != null)
                    {
                        behaviour.OnRemoteServerRPC(hash, clientId, stream);
                    }
                }
            }
			*/
        }
        
        internal static void HandleServerRPCRequest(uint clientId, Stream stream, int channelId, SecuritySendFlags security)
        {
			/*
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint networkId = reader.ReadUInt32Packed();
                ushort behaviourId = reader.ReadUInt16Packed();
                ulong hash = reader.ReadUInt64Packed();
                ulong responseId = reader.ReadUInt64Packed();

                if (SpawnManager.SpawnedObjects.ContainsKey(networkId)) 
                { 
                    Conduct behaviour = SpawnManager.SpawnedObjects[networkId].GetBehaviourAtOrderIndex(behaviourId);
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
                            
                            InternalMessageHandler.Send(clientId, AlpacaConstant.ALPACA_SERVER_RPC_RESPONSE, MessageManager.reverseChannels[channelId], responseStream, security);
                        }
                    }
                }
            }
			*/
        }
        
        internal static void HandleServerRPCResponse(uint clientId, Stream stream, int channelId)
        {
			/*
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                ulong responseId = reader.ReadUInt64Packed();

                if (ResponseMessageManager.ContainsKey(responseId))
                {
                    RpcResponseBase responseBase = ResponseMessageManager.GetByKey(responseId);

                    if (responseBase.GetId() != clientId) return;
                    
                    ResponseMessageManager.Remove(responseId);
                    
                    responseBase.IsDone = true;
                    responseBase.Result = reader.ReadObjectPacked(responseBase.Type);
                    responseBase.IsSuccessful = true;
                }
            }
			*/
        }
        
        internal static void HandleClientRPC(uint clientId, Stream stream, int channelId)
        {
			/*
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint networkId = reader.ReadUInt32Packed();
                ushort behaviourId = reader.ReadUInt16Packed();
                ulong hash = reader.ReadUInt64Packed();
                
                if (SpawnManager.SpawnedObjects.ContainsKey(networkId)) 
                {
                    Conduct behaviour = SpawnManager.SpawnedObjects[networkId].GetBehaviourAtOrderIndex(behaviourId);
                    if (behaviour != null)
                    {
                        behaviour.OnRemoteClientRPC(hash, clientId, stream);
                    }
                }
            }
			*/
        }
        
        internal static void HandleClientRPCRequest(uint clientId, Stream stream, int channelId, SecuritySendFlags security)
        {
			/*
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                uint networkId = reader.ReadUInt32Packed();
                ushort behaviourId = reader.ReadUInt16Packed();
                ulong hash = reader.ReadUInt64Packed();
                ulong responseId = reader.ReadUInt64Packed();
                
                if (SpawnManager.SpawnedObjects.ContainsKey(networkId)) 
                {
                    Conduct behaviour = SpawnManager.SpawnedObjects[networkId].GetBehaviourAtOrderIndex(behaviourId);
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
                            
                            InternalMessageHandler.Send(clientId, AlpacaConstant.ALPACA_CLIENT_RPC_RESPONSE, MessageManager.reverseChannels[channelId], responseStream, security);
                        }
                    }
                }
            }
			*/
        }
        
        internal static void HandleClientRPCResponse(uint clientId, Stream stream, int channelId)
        {
			/*
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
			*/
        }
        
        internal static void HandleCustomMessage(uint clientId, Stream stream, int channelId)
        {
            AlpacaNetwork.GetSingleton().InvokeOnIncomingCustomMessage(clientId, stream);
        }


		// PRIVATE

		static void SpawnEntityClient( AlpacaNetwork network, PooledBitReader reader )
		{
			Debug.Assert( network.IsClient );

			Entity.Spawn data = new Entity.Spawn();
			data.ReadFrom( reader );

			Entity entity = Entity.SpawnEntity( network, data );
			// TODO: cozeroff still necessary or can SyncVars just both use initial values?
			entity.ReadNetworkedVarData( reader );

			if( entity.IsAvatar() )
			{
				uint ownerClientId = entity.GetOwnerClientId();

				// TODO: cozeroff
				/*
				Client c = new Client( ownerClientId, entity, null );
				network._connectedClients.Add( ownerClientId, c );
				*/

				if( network.OnAvatarSpawn != null
				  && ownerClientId == network.LocalClientId
				  )
				{
					Debug.Log( "OnAvatarSpawn called for local client:" + ownerClientId );
					network.OnAvatarSpawn.Invoke( entity );
				}
			}
			else
			{
				network.OnEntitySpawn( entity );
			}
		}
    }
}

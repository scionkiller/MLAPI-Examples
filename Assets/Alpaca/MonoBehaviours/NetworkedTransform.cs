using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

using Alpaca.Serialization;
using Alpaca.Data;

namespace Alpaca.Prototyping
{
    /// <summary>
    /// A prototype component for syncing transforms
    /// </summary>
    [AddComponentMenu("Alpaca/NetworkedTransform")]
    public class NetworkedTransform : Conduct
    {
		/*
        internal class ClientSendInfo
        {
            public uint clientId;
            public float lastSent;
            public Vector3? lastMissedPosition;
            public Quaternion? lastMissedRotation;
        }

        /// <summary>
        /// The base amount of sends per seconds to use when range is disabled
        /// </summary>
        [Range(0, 120)]
        public float FixedSendsPerSecond = 20f;
        /// <summary>
        /// Is the sends per second assumed to be the same across all instances
        /// </summary>
        [Tooltip("This assumes that the SendsPerSecond is synced across clients")]
        public bool AssumeSyncedSends = true;
        /// <summary>
        /// Enable interpolation
        /// </summary>
        [Tooltip("This requires AssumeSyncedSends to be true")]
        public bool InterpolatePosition = true;
        /// <summary>
        /// The distance before snaping to the position
        /// </summary>
        [Tooltip("The transform will snap if the distance is greater than this distance")]
        public float SnapDistance = 10f;
        /// <summary>
        /// Should the server interpolate
        /// </summary>
        public bool InterpolateServer = true;
        /// <summary>
        /// The min meters to move before a send is sent
        /// </summary>
        public float MinMeters = 0.15f;
        /// <summary>
        /// The min degrees to rotate before a send it sent
        /// </summary>
        public float MinDegrees = 1.5f;
        /// <summary>
        /// Enables extrapolation
        /// </summary>
        public bool ExtrapolatePosition = false;
        /// <summary>
        /// The maximum amount of expected send rates to extrapolate over when awaiting new packets.
        /// A higher value will result in continued extrapolation after an object has stopped moving
        /// </summary>
        public float MaxSendsToExtrapolate = 5;

        private float lerpT;
        private Vector3 lerpStartPos;
        private Quaternion lerpStartRot;
        private Vector3 lerpEndPos;
        private Quaternion lerpEndRot;

        private float lastSendTime;
        private Vector3 lastSentPos;
        private Quaternion lastSentRot;

        private float lastRecieveTime;
        
        /// <summary>
        /// Enables range based send rate
        /// </summary>
        public bool EnableRange;

        /// <summary>
        /// Checks for missed sends without provocation. Provocation being a client inside it's normal SendRate
        /// </summary>
        public bool EnableNonProvokedResendChecks;

        /// <summary>
        /// The curve to use to calculate the send rate
        /// </summary>
        public AnimationCurve DistanceSendrate = AnimationCurve.Constant(0, 500, 20);
        private readonly Dictionary<uint, ClientSendInfo> clientSendInfo = new Dictionary<uint, ClientSendInfo>();

        /// <summary>
        /// The delegate used to check if a move is valid
        /// </summary>
        /// <param name="oldPos">The previous position</param>
        /// <param name="newPos">The new requested position</param>
        /// <returns>Returns whether or not the move is valid</returns>
        public delegate bool MoveValidationDelegate(Vector3 oldPos, Vector3 newPos);
        /// <summary>
        /// If set, moves will only be accepted if the custom delegate returns true
        /// </summary>
        public MoveValidationDelegate IsMoveValidDelegate = null;

        private void OnValidate()
        {
            if (!AssumeSyncedSends && InterpolatePosition)
                InterpolatePosition = false;
            if (InterpolateServer && !InterpolatePosition)
                InterpolateServer = false;
            if (MinDegrees < 0)
                MinDegrees = 0;
            if (MinMeters < 0)
                MinMeters = 0;
            if (EnableNonProvokedResendChecks && !EnableRange)
                EnableNonProvokedResendChecks = false;
        }
        
        private float GetDistanceSendRate(Vector3 pos1, Vector3 pos2)
        {
            return DistanceSendrate.Evaluate(Vector3.Distance(pos1, pos2));
        }
        
        public override void OnInitializeNetworkBehaviour()
        {
            lastSentRot = transform.rotation;
            lastSentPos = transform.position;

            lerpStartPos = transform.position;
            lerpStartRot = transform.rotation;

            lerpEndPos = transform.position;
            lerpEndRot = transform.rotation;
        }

        private void Update()
        {
            if( _isLocalOwned )
            {
                if( _network.NetworkTime - lastSendTime >= (1f / FixedSendsPerSecond) && (Vector3.Distance(transform.position, lastSentPos) > MinMeters || Quaternion.Angle(transform.rotation, lastSentRot) > MinDegrees))
                {
                    lastSendTime = _network.NetworkTime;
                    lastSentPos = transform.position;
                    lastSentRot = transform.rotation;
                    using (PooledBitStream stream = PooledBitStream.Get())
                    {
                        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                        {
                            writer.WriteSinglePacked(transform.position.x);
                            writer.WriteSinglePacked(transform.position.y);
                            writer.WriteSinglePacked(transform.position.z);

                            writer.WriteSinglePacked(transform.rotation.eulerAngles.x);
                            writer.WriteSinglePacked(transform.rotation.eulerAngles.y);
                            writer.WriteSinglePacked(transform.rotation.eulerAngles.z);

                            if( _network.IsServer )
							{
                                InvokeClientRpcOnEveryoneExcept(ApplyTransform, _ownerClientId, stream);
							}
							else
							{
                                InvokeServerRpc(SubmitTransform, stream);
							}
                        }
                    }

                }
            }
            else
            {
                // If we are server and interpolation is turned on for server OR we are not server and interpolation is turned on
                if( (_network.IsServer && InterpolateServer && InterpolatePosition) || (!_network.IsServer && InterpolatePosition))
                {
                    if (Vector3.Distance(transform.position, lerpEndPos) > SnapDistance)
                    {
                        // Snap, set T to 1 (100% of the lerp)
                        lerpT = 1f;
                    }

					float sendDelay = 0f;
					bool simpleDelay = (_network.IsServer || !EnableRange || !AssumeSyncedSends);
					if( simpleDelay )
					{
						sendDelay = 1f / FixedSendsPerSecond;
					}
					else
					{
						sendDelay = 1f / GetDistanceSendRate(transform.position, _network._connectedClients[_network.LocalClientId].GetAvatar().transform.position );
					}
                    lerpT = Time.deltaTime / sendDelay;

                    if (ExtrapolatePosition && (Time.time - lastRecieveTime < sendDelay * MaxSendsToExtrapolate) )
					{
                        transform.position = Vector3.LerpUnclamped(lerpStartPos, lerpEndPos, lerpT);
					}
					else
					{
                        transform.position = Vector3.Lerp(lerpStartPos, lerpEndPos, lerpT);
					}

                    if (ExtrapolatePosition && (Time.time - lastRecieveTime < sendDelay * MaxSendsToExtrapolate) )
                        transform.rotation = Quaternion.SlerpUnclamped(lerpStartRot, lerpEndRot, lerpT);
                    else
                        transform.rotation = Quaternion.Slerp(lerpStartRot, lerpEndRot, lerpT);
                }
            }

            if( _network.IsServer && EnableRange && EnableNonProvokedResendChecks )
			{
				CheckForMissedSends();
			}
        }

        [ClientRPC]
        private void ApplyTransform(uint clientId, Stream stream)
        {
            if (!enabled) return;
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {

                float xPos = reader.ReadSinglePacked();
                float yPos = reader.ReadSinglePacked();
                float zPos = reader.ReadSinglePacked();

                float xRot = reader.ReadSinglePacked();
                float yRot = reader.ReadSinglePacked();
                float zRot = reader.ReadSinglePacked();

                if (InterpolatePosition)
                {
                    lastRecieveTime = Time.time;
                    lerpStartPos = transform.position;
                    lerpStartRot = transform.rotation;
                    lerpEndPos = new Vector3(xPos, yPos, zPos);
                    lerpEndRot = Quaternion.Euler(xRot, yRot, zRot);
                    lerpT = 0;
                }
                else
                {
                    transform.position = new Vector3(xPos, yPos, zPos);
                    transform.rotation = Quaternion.Euler(new Vector3(xRot, yRot, zRot));
					Debug.Log( "ApplyTransform with no interpolation called. position: " + transform.position.ToString() + " rotation: " + transform.rotation.ToString() );
                }
            }
        }

        [ServerRPC]
        private void SubmitTransform(uint clientId, Stream stream)
        {
            if (!enabled) return;
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {

                float xPos = reader.ReadSinglePacked();
                float yPos = reader.ReadSinglePacked();
                float zPos = reader.ReadSinglePacked();

                float xRot = reader.ReadSinglePacked();
                float yRot = reader.ReadSinglePacked();
                float zRot = reader.ReadSinglePacked();

                if (IsMoveValidDelegate != null && !IsMoveValidDelegate(lerpEndPos, new Vector3(xPos, yPos, zPos)))
                {
                    //Invalid move!
                    //TODO: Add rubber band (just a message telling them to go back)
                    return;
                }

                using (PooledBitStream writeStream = PooledBitStream.Get())
				using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
				{
					writer.WriteSinglePacked(xPos);
					writer.WriteSinglePacked(yPos);
					writer.WriteSinglePacked(zPos);

					writer.WriteSinglePacked(xRot);
					writer.WriteSinglePacked(yRot);
					writer.WriteSinglePacked(zRot);

					if( EnableRange )
					{
						for( int i = 0; i < _network._connectedClients.GetCount(); ++i )
						{
							Client c = _network._connectedClients.GetAt(i);
							if( !clientSendInfo.ContainsKey( c.GetId() ) )
							{
								clientSendInfo.Add( c.GetId(), new ClientSendInfo()
								{
									clientId = c.GetId(),
									lastMissedPosition = null,
									lastMissedRotation = null,
									lastSent = 0
								});
							}

							ClientSendInfo info = clientSendInfo[c.GetId()];
							Vector3 receiverPosition = c.GetAvatar().transform.position;
							Vector3 senderPosition = _network._connectedClients[_ownerClientId].GetAvatar().transform.position;

							float lerpTime = 1f / GetDistanceSendRate( receiverPosition, senderPosition );
							if( _network.NetworkTime - info.lastSent >= lerpTime )
							{
								info.lastSent = _network.NetworkTime;
								info.lastMissedPosition = null;
								info.lastMissedRotation = null;

								InvokeClientRpcOnClient(ApplyTransform, c.GetId(), writeStream);
							}
							else
							{
								info.lastMissedPosition = new Vector3(xPos, yPos, zPos);
								info.lastMissedRotation = Quaternion.Euler(xRot, yRot, zRot);
							}
						}
					}
					else
					{
						Debug.Log( "SubmitTransform called on server" );
						InvokeClientRpcOnEveryoneExcept(ApplyTransform, _ownerClientId, writeStream);
					}
				}
            }
        }

        private void CheckForMissedSends()
        {
            for (int i = 0; i < _network._connectedClients.GetCount(); i++)
            {
				Client c = _network._connectedClients.GetAt(i);
                if( !clientSendInfo.ContainsKey( c.GetId() ) )
                {
                    clientSendInfo.Add( c.GetId(), new ClientSendInfo()
                    {
                        clientId = c.GetId(),
                        lastMissedPosition = null,
                        lastMissedRotation = null,
                        lastSent = 0
                    });
                }
                ClientSendInfo info = clientSendInfo[c.GetId()];

				Client owner = _network._connectedClients.Get(_ownerClientId);

                Vector3 receiverPosition = c.GetAvatar().transform.position;
                Vector3 senderPosition = owner.GetAvatar().transform.position;
                
				float lerpTime = 1f / GetDistanceSendRate( receiverPosition, senderPosition );
                if( _network.NetworkTime - info.lastSent >= lerpTime )
                {
                    Vector3 pos = owner.GetAvatar().transform.position;
                    Vector3 rot = owner.GetAvatar().transform.rotation.eulerAngles;
                    
                    info.lastSent = _network.NetworkTime;
                    info.lastMissedPosition = null;
                    info.lastMissedRotation = null;
                    
                    using (PooledBitStream stream = PooledBitStream.Get())
                    {
                        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                        {
                            writer.WriteSinglePacked(pos.x);
                            writer.WriteSinglePacked(pos.y);
                            writer.WriteSinglePacked(pos.z);

                            writer.WriteSinglePacked(rot.x);
                            writer.WriteSinglePacked(rot.y);
                            writer.WriteSinglePacked(rot.z);

                            InvokeClientRpcOnClient(ApplyTransform, c.GetId(), stream);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Teleports the transform to the given position and rotation
        /// </summary>
        /// <param name="position">The position to teleport to</param>
        /// <param name="rotation">The rotation to teleport to</param>
        public void Teleport(Vector3 position, Quaternion rotation)
        {   
            if (InterpolateServer && IsServer || IsClient)
            {
                lerpStartPos = position;
                lerpStartRot = rotation;
                lerpEndPos = position;
                lerpEndRot = rotation;
                lerpT = 0;
            }
        }
	*/
    }
}

using System;
using System.Collections.Generic;
using System.Security.Cryptography;

using Alpaca.Cryptography;
using Alpaca.Serialization;


namespace Alpaca.Internal
{
	internal class MessageManager
	{
/*
		internal static BitStream WrapMessage(byte messageType, uint clientId, BitStream messageBody, SecuritySendFlags flags)
		{
			try
			{
				bool encrypted = ((flags & SecuritySendFlags.Encrypted) == SecuritySendFlags.Encrypted) && AlpacaNetwork.GetSingleton().config.EnableEncryption;
				bool authenticated = (flags & SecuritySendFlags.Authenticated) == SecuritySendFlags.Authenticated && AlpacaNetwork.GetSingleton().config.EnableEncryption;

				PooledBitStream outStream = PooledBitStream.Get();

				using (PooledBitWriter outWriter = PooledBitWriter.Get(outStream))
				{
					outWriter.WriteBit(encrypted);
					outWriter.WriteBit(authenticated);
					
					if (authenticated || encrypted)
					{
						AlpacaNetwork network = AlpacaNetwork.GetSingleton();

						outWriter.WritePadBits();
						long hmacWritePos = outStream.Position;

						if (authenticated) outStream.Write(HMAC_PLACEHOLDER, 0, HMAC_PLACEHOLDER.Length);

						if (encrypted)
						{
							using (RijndaelManaged rijndael = new RijndaelManaged())
							{
								rijndael.GenerateIV();
								rijndael.Padding = PaddingMode.PKCS7;

								byte[] key = network.GetPublicEncryptionKey(clientId);
								if (key == null)
								{
									Log.Error("Failed to grab key");
									return null;
								}

								rijndael.Key = key;

								outStream.Write(rijndael.IV);

								using (CryptoStream encryptionStream = new CryptoStream(outStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write))
								{
									encryptionStream.WriteByte(messageType);
									encryptionStream.Write(messageBody.GetBuffer(), 0, (int)messageBody.Length);
								}
							}
						}
						else
						{
							outStream.WriteByte(messageType);
							outStream.Write(messageBody.GetBuffer(), 0, (int)messageBody.Length);
						}

						if (authenticated)
						{
							byte[] key = network.GetPublicEncryptionKey(clientId);
							if (key == null)
							{
								Log.Error("Failed to grab key");
								return null;
							}

							using (HMACSHA256 hmac = new HMACSHA256(key))
							{
								byte[] computedHmac = hmac.ComputeHash(outStream.GetBuffer(), 0, (int)outStream.Length);

								outStream.Position = hmacWritePos;
								outStream.Write(computedHmac, 0, computedHmac.Length);
							}
						}
					}
					else
					{
						outWriter.WriteBits(messageType, 6);
						outStream.Write(messageBody.GetBuffer(), 0, (int)messageBody.Length);
					}
				}

				return outStream;
			}
			catch (Exception e)
			{
				Log.Error("Error while wrapping headers");
				Log.Error(e.ToString());

				return null;
			}
		}
		*/
	}
}
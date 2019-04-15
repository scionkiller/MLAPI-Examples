﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;

using Alpaca.Cryptography;
using Alpaca.Serialization;


namespace Alpaca.Internal
{
	internal static class MessageManager
	{
	/*
		internal static readonly Dictionary<string, int> channels = new Dictionary<string, int>();
		internal static readonly Dictionary<int, string> reverseChannels = new Dictionary<int, string>();

		private static readonly byte[] IV_BUFFER = new byte[16];
		private static readonly byte[] HMAC_BUFFER = new byte[32];
		private static readonly byte[] HMAC_PLACEHOLDER = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	*/

		// This method is responsible for unwrapping a message, that is extracting the messagebody.
		// Could include decrypting and/or authentication.
		internal static BitStream UnwrapMessage(BitStream inputStream, uint clientId, out byte messageType, out SecuritySendFlags security)
		{
			using (PooledBitReader inputHeaderReader = PooledBitReader.Get(inputStream))
			{
				try
				{
					if (inputStream.Length < 1)
					{
						Log.Error("The incomming message was too small");
						messageType = AlpacaConstant.INVALID;
						security = SecuritySendFlags.None;
						return null;
					}

					bool isEncrypted = inputHeaderReader.ReadBit();
					bool isAuthenticated = inputHeaderReader.ReadBit();

					if (isEncrypted && isAuthenticated) security = SecuritySendFlags.Encrypted | SecuritySendFlags.Authenticated;
					else if (isEncrypted) security = SecuritySendFlags.Encrypted;
					else if (isAuthenticated) security = SecuritySendFlags.Authenticated;
					else security = SecuritySendFlags.None;
					
					if (isEncrypted || isAuthenticated)
					{
						AlpacaNetwork network = AlpacaNetwork.GetSingleton();

						if (!network.config.EnableEncryption)
						{
							Log.Error("Got a encrypted and/or authenticated message but encryption was not enabled");
							messageType = AlpacaConstant.INVALID;
							return null;
						}

						// Skip last bits in first byte
						inputHeaderReader.SkipPadBits();

						if (isAuthenticated)
						{
							long hmacStartPos = inputStream.Position;

							int readHmacLength = inputStream.Read(HMAC_BUFFER, 0, HMAC_BUFFER.Length);

							if (readHmacLength != HMAC_BUFFER.Length)
							{
								Log.Error("HMAC length was invalid");
								messageType = AlpacaConstant.INVALID;
								return null;
							}

							// Now we have read the HMAC, we need to set the hmac in the input to 0s to perform the HMAC.
							inputStream.Position = hmacStartPos;
							inputStream.Write(HMAC_PLACEHOLDER, 0, HMAC_PLACEHOLDER.Length);

							byte[] key = network.GetPublicEncryptionKey(clientId);
							if( key == null )
							{
								Log.Error("Failed to grab key");
								messageType = AlpacaConstant.INVALID;
								return null;
							}

							using (HMACSHA256 hmac = new HMACSHA256(key))
							{
								byte[] computedHmac = hmac.ComputeHash(inputStream.GetBuffer(), 0, (int)inputStream.Length);


								if (!CryptographyHelper.ConstTimeArrayEqual(computedHmac, HMAC_BUFFER))
								{
									Log.Error("Received HMAC did not match the computed HMAC");
									messageType = AlpacaConstant.INVALID;
									return null;
								}
							}
						}

						if (isEncrypted)
						{
							int ivRead = inputStream.Read(IV_BUFFER, 0, IV_BUFFER.Length);

							if (ivRead != IV_BUFFER.Length)
							{
								Log.Error("Invalid IV size");
								messageType = AlpacaConstant.INVALID;
								return null;
							}

							PooledBitStream outputStream = PooledBitStream.Get();

							using (RijndaelManaged rijndael = new RijndaelManaged())
							{
								rijndael.IV = IV_BUFFER;
								rijndael.Padding = PaddingMode.PKCS7;

								byte[] key = network.GetPublicEncryptionKey(clientId);
								if (key == null)
								{
									Log.Error("Failed to grab key");
									messageType = AlpacaConstant.INVALID;
									return null;
								}

								rijndael.Key = key;

								using (CryptoStream cryptoStream = new CryptoStream(outputStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write))
								{
									cryptoStream.Write(inputStream.GetBuffer(), (int)inputStream.Position, (int)(inputStream.Length - inputStream.Position));
								}

								outputStream.Position = 0;

								if (outputStream.Length == 0)
								{
									Log.Error("The incomming message was too small");
									messageType = AlpacaConstant.INVALID;
									return null;
								}

								int msgType = outputStream.ReadByte();
								messageType = msgType == -1 ? AlpacaConstant.INVALID : (byte)msgType;
							}

							return outputStream;
						}
						else
						{
							if (inputStream.Length - inputStream.Position <= 0)
							{
								Log.Error("The incomming message was too small");
								messageType = AlpacaConstant.INVALID;
								return null;
							}

							int msgType = inputStream.ReadByte();
							messageType = msgType == -1 ? AlpacaConstant.INVALID : (byte)msgType;
							return inputStream;
						}
					}
					else
					{
						messageType = inputHeaderReader.ReadByteBits(6);
						// The input stream is now ready to be read from. It's "safe" and has the correct position
						return inputStream;
					}
				}
				catch (Exception e)
				{
					Log.Error("Error while unwrapping headers");
					Log.Error(e.ToString());

					security = SecuritySendFlags.None;
					messageType = AlpacaConstant.INVALID;
					return null;
				}
			}
		}

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
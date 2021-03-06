﻿using System.Security.Cryptography;
using System.Text;
using System.Numerics;
using Array = System.Array;

using UnityEngine;

using Alpaca.Internal;



namespace Alpaca.Cryptography
{
	internal class EllipticDiffieHellman
	{
		protected static readonly RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();

		public static BigInteger DEFAULT_PRIME
		{
			get
			{
				if (defaultPrime == null)
				{
					try
					{
						defaultPrime = (BigInteger.Parse("1") << 255) - 19;
					}
					catch( System.Exception )
					{
						Log.Error( "CryptoLib failed to parse BigInt. If you are using .NET 2.0 Subset, switch to .NET 2.0 or .NET 4.5" );
					}
				}

				return defaultPrime;
			}
		}

		private static BigInteger defaultPrime;

		public static BigInteger DEFAULT_ORDER
		{
			get
			{
				if (defaultOrder == null)
				{
					try
					{
						defaultOrder = (new BigInteger(1) << 252) +
									   BigInteger.Parse("27742317777372353535851937790883648493");
					}
					catch( System.Exception )
					{
						Log.Error( "CryptoLib failed to parse BigInt. If you are using .NET 2.0 Subset, switch to .NET 2.0 or .NET 4.5" );
					}
				}

				return defaultOrder;
			}
		}

		private static BigInteger defaultOrder;

		public static EllipticCurve DEFAULT_CURVE
		{
			get
			{
				if (defaultCurve == null)
				{
					try
					{
						defaultCurve = new EllipticCurve(486662, 1, DEFAULT_PRIME, EllipticCurve.CurveType.Montgomery);
					}
					catch (System.Exception)
					{
						Log.Error( "CryptoLib failed to parse BigInt. If you are using .NET 2.0 Subset, switch to .NET 2.0 or .NET 4.5" );
					}
				}

				return defaultCurve;
			}
		}

		private static EllipticCurve defaultCurve;

		public static CurvePoint DEFAULT_GENERATOR
		{
			get
			{
				if (defaultGenerator == null)
				{
					try
					{
						defaultGenerator = new CurvePoint(9, BigInteger.Parse("14781619447589544791020593568409986887264606134616475288964881837755586237401"));
					}
					catch (System.Exception)
					{
						Log.Error( "CryptoLib failed to parse BigInt. If you are using .NET 2.0 Subset, switch to .NET 2.0 or .NET 4.5" );
					}
				}

				return defaultGenerator;
			}
		}

		private static CurvePoint defaultGenerator;

		protected readonly EllipticCurve curve;
		public readonly BigInteger priv;
		protected readonly CurvePoint generator, pub;

		public EllipticDiffieHellman(EllipticCurve curve, CurvePoint generator, BigInteger order, byte[] priv = null)
		{
			this.curve = curve;
			this.generator = generator;

			// Generate private key
			if (priv == null)
			{
				this.priv = new BigInteger();
				// TODO: cozeroff FIX THIS! THIS IS TOTALLY BROKEN!
				//this.priv.GenRandomBits( order.ToByteArray().Length, rand );
			}
			else this.priv = new BigInteger(priv);

			// Generate public key
			pub = curve.Multiply(generator, this.priv);
		}

		public byte[] GetPublicKey()
		{
			byte[] p1 = pub.X.ToByteArray();
			byte[] p2 = pub.Y.ToByteArray();

			byte[] ser = new byte[4 + p1.Length + p2.Length];
			ser[0] = (byte)(p1.Length & 255);
			ser[1] = (byte)((p1.Length >> 8) & 255);
			ser[2] = (byte)((p1.Length >> 16) & 255);
			ser[3] = (byte)((p1.Length >> 24) & 255);
			Array.Copy(p1, 0, ser, 4, p1.Length);
			Array.Copy(p2, 0, ser, 4 + p1.Length, p2.Length);

			return ser;
		}

		public byte[] GetPrivateKey() => priv.ToByteArray();

		public byte[] GetSharedSecret(byte[] pK)
		{
			byte[] p1 = new byte[pK[0] | (pK[1] << 8) | (pK[2] << 16) | (pK[3] << 24)]; // Reconstruct x-axis size
			byte[] p2 = new byte[pK.Length - p1.Length - 4];
			Array.Copy(pK, 4, p1, 0, p1.Length);
			Array.Copy(pK, 4 + p1.Length, p2, 0, p2.Length);

			CurvePoint remotePublic = new CurvePoint(new BigInteger(p1), new BigInteger(p2));

			byte[] secret = curve.Multiply(remotePublic, priv).X.ToByteArray(); // Use the x-coordinate as the shared secret

			// PBKDF2-HMAC-SHA1 (Common shared secret generation method)
			return new Rfc2898DeriveBytes(secret, Encoding.UTF8.GetBytes("P1sN0R4inb0wPl5P1sPls"), 1000).GetBytes(32);
		}
	}
}
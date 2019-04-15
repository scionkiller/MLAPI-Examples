using System.Security.Cryptography;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using Alpaca.Serialization;


namespace Alpaca.Cryptography
{
	/// <summary>
	/// Helper class for encryption purposes
	/// </summary>
	public static class CryptographyHelper
	{
		/// <summary>
		/// The delegate type used to validate certificates
		/// </summary>
		/// <param name="certificate">The certificate to validate</param>
		/// <param name="hostname">The hostname the certificate is claiming to be</param>
		public delegate bool VerifyCertificateDelegate(X509Certificate2 certificate, string hostname);
		/// <summary>
		/// The delegate to invoke to validate the certificates
		/// </summary>
		public static VerifyCertificateDelegate OnValidateCertificateCallback = null;
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="certificate">The certificate to validate</param>
		/// <param name="hostname">The hostname the certificate is claiming to be</param>
		/// <returns>Whether or not the certificate is considered valid</returns>
		public static bool VerifyCertificate(X509Certificate2 certificate, string hostname)
		{
			if (OnValidateCertificateCallback != null) return OnValidateCertificateCallback(certificate, hostname);
			return certificate.Verify() && (hostname == certificate.GetNameInfo(X509NameType.DnsName, false) || hostname == "127.0.0.1");
		}

		internal static bool ConstTimeArrayEqual(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
				return false;
			
			int i = a.Length;
			int cmp = 0;
			
			while (i != 0)
			{
				--i;
				cmp |= (a[i] ^ b[i]);
			}
			
			return cmp == 0;
		}
	}
}
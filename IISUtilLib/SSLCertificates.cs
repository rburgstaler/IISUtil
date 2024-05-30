using ACMEClientLib;
using ACMEClientLib.Crypto;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO.Pem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IISUtilLib
{
    public delegate void SSLOutputMessage(String msg);
    public class SSLCertificates
    {
        public void PrintOutAllCerts(SSLOutputMessage msgOutput)
        {
            var stores = Enum.GetValues(typeof(StoreName)).Cast<StoreName>();
            foreach (StoreName store in stores)
            {
                X509Store st = new X509Store(store, StoreLocation.LocalMachine);
                msgOutput("==========="+Enum.GetName(typeof(StoreName), store) + "===========");

                PrintCerts(st, msgOutput);
            }

            X509Store st2 = new X509Store("WebHosting");
            msgOutput("===========" + st2.Name + "===========");

            PrintCerts(st2, msgOutput);
        }

        public static string ByteArrayToHexString(byte[] byteArray)
        {
            String retVal = BitConverter.ToString(byteArray ?? new Byte[0]).Replace("-", "").ToLower();
            return retVal;
        }
		
        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public void PrintCerts(X509Store store, SSLOutputMessage msgOutput)
        {
            try
            {
                store.Open(OpenFlags.OpenExistingOnly);
                foreach (var cert in store.Certificates)
                {
                    msgOutput($"Subject: {cert.Subject}, FriendlyName: {cert.FriendlyName}, SubjectName.Name: {cert.SubjectName.Name}, Hash: {ByteArrayToHexString(cert.GetCertHash())}");
                }
            }
            catch (Exception exp)
            {
                msgOutput(exp.Message);
            }
        }

        public static void GetInstalledCertificates(SSLOutputMessage msgOutput)
        {
            (new SSLCertificates()).PrintOutAllCerts(msgOutput);
        }


        //A good default for certificateStore is WebHosting
        public static CertInfo GetCertInfo(string PFXFileName, string PFXPassword, Action<string> StatusMsg)
        {
            CertInfo retVal = new CertInfo();

            // See http://paulstovell.com/blog/x509certificate2
            X509KeyStorageFlags flags = X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet;
            flags |= X509KeyStorageFlags.Exportable;
            X509Certificate2 certificate = new X509Certificate2(PFXFileName, PFXPassword, flags);

            retVal.FriendlyName = certificate.FriendlyName;
            retVal.Hash.SHA1 = ByteArrayToHexString(certificate.GetCertHash());
            retVal.CommonName = CertUtil.ExtractCommonName(certificate.Subject);
            retVal.NotAfter = certificate.NotAfter.ToUniversalTime();
            retVal.NotBefore = certificate.NotBefore.ToUniversalTime();   
            retVal.Subject = certificate.Subject;
            retVal.Issuer = certificate.GetIssuerName();

            /*
            using (var fs = new FileStream(PFXFileName, FileMode.Open))
            {
                fs.Seek(0, SeekOrigin.Begin);
                StatusMsg($"hello world {fs.Length}");


                Pkcs12Store cert = new Pkcs12Store(fs, PFXPassword.ToCharArray());

                
                foreach (object a in cert.Aliases)
                {
                    String alias = (String)a;

                   
                    X509CertificateEntry[] ces = cert.GetCertificateChain(alias);
                    StatusMsg($"{alias}: {ces.Length}");

                    foreach (X509CertificateEntry ce in ces)
                    {
                        byte[] hashBytes;
                        byte[] result;
                        byte[] input;
                        // Use input string to calculate MD5 hash
                        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                        {
                            input = ce.Certificate.GetEncoded();

                            MD5Digest d = new MD5Digest();
                            result = new byte[d.GetDigestSize()];
                            d.BlockUpdate(input, 0, input.Length);
                            d.DoFinal(result, 0);
                            hashBytes = md5.ComputeHash(result);
                        }

                        //ce.Certificate.
                        StatusMsg(ce.Certificate.GetBasicConstraints().ToString());
                        StatusMsg(CertUtil.ExtractCommonName(ce.Certificate.SubjectDN.ToString()));
                        StatusMsg($"  ->{ce.Certificate.SubjectDN.ToString()}  SHA_256:{ByteArrayToHexString(DigestUtilities.CalculateDigest("SHA_512", ce.Certificate.CertificateStructure.ToAsn1Object().GetEncoded()))}");
                        StatusMsg($"  ->{ce.Certificate.SubjectDN.ToString()}  {ByteArrayToHexString(hashBytes)}");
                        StatusMsg($"  ->{ce.Certificate.SubjectDN.ToString()}  {ByteArrayToHexString(result)}");

                        Sha256Digest d256 = new Sha256Digest();
                        d256.BlockUpdate(input, 0, input.Length);
                        result = new byte[d256.GetDigestSize()];
                        d256.DoFinal(result, 0);
                        StatusMsg($"  ->{ce.Certificate.SubjectDN.ToString()}  {ByteArrayToHexString(result)}");

                        Sha1Digest d1 = new Sha1Digest();
                        d1.BlockUpdate(input, 0, input.Length);
                        result = new byte[d1.GetDigestSize()];
                        d1.DoFinal(result, 0);
                        StatusMsg($"  ->{ce.Certificate.SubjectDN.ToString()}  {ByteArrayToHexString(result)}");

                    }



                }

                //tatusMsg(cert.SubjectDN.ToString());
                //StatusMsg(cert.NotAfter.ToUniversalTime().ToString());
                //StatusMsg(CertUtil.GetCertFriendlyName(CertUtil.ExtractCommonName(cert.NotAfter.ToUniversalTime().ToString()), cert.NotBefore, cert.NotAfter));



                //m.CopyTo(fs);
            }

            StatusMsg("hello world");
            String fileName = "D:\\Debug\\Certs\\star.burgstaler.com.crt";
            fileName = "D:\\Debug\\Certs\\star.burgstaler.com.nopass.hack.pem";
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                fs.Seek(0, SeekOrigin.Begin);
                StatusMsg($"hello world {fs.Length}");


                Org.BouncyCastle.X509.X509Certificate cert = CertHelper.ImportCertificate(EncodingFormat.PEM, fs);
                StatusMsg(cert.SubjectDN.ToString());
                //StatusMsg("Basic constraints: "+cert.GetBasicConstraints().);
                StatusMsg(cert.NotAfter.ToUniversalTime().ToString());
                StatusMsg(CertUtil.GetCertFriendlyName(CertUtil.ExtractCommonName(cert.NotAfter.ToUniversalTime().ToString()), cert.NotBefore, cert.NotAfter));

             

                //m.CopyTo(fs);
            }
			*/

            return retVal;
        }

        //A good default for certificateStore is WebHosting
        public static byte[] InstallCertificate(string pfxFilename, string PFXPassword, String certificateStore, Action<string> StatusMsg)
        {
            byte[] retVal = new byte[0];
            X509Store store = null;
            X509Certificate2 certificate = null;

            try
            {
                store = new X509Store(certificateStore, StoreLocation.LocalMachine);
                store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);
            }
            catch (CryptographicException)
            {
                store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);
            }
            catch (Exception ex)
            {
                StatusMsg($"Error encountered while opening certificate store. Error: {ex.Message}");
                throw new Exception(ex.Message);
            }

            StatusMsg($"Opened Certificate Store {store.Name}");
            try
            {
                X509KeyStorageFlags flags = X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet;

                //Set to be exportable
                StatusMsg($" Set private key exportable");
                StatusMsg("Set private key exportable");
                flags |= X509KeyStorageFlags.Exportable;

                // See http://paulstovell.com/blog/x509certificate2
                certificate = new X509Certificate2(pfxFilename, PFXPassword, flags);

                certificate.FriendlyName = CertUtil.GetCertFriendlyName(CertUtil.ExtractCommonName(certificate.Subject), certificate.NotBefore, certificate.NotAfter);
                StatusMsg(certificate.FriendlyName);

                StatusMsg("Common Name: " + certificate.SubjectName.Name);
                StatusMsg("Certificate hash: " + ByteArrayToHexString(certificate.GetCertHash()));



                StatusMsg("Adding Certificate to Store");
                store.Add(certificate);



                retVal = certificate.GetCertHash();

                StatusMsg("Closing Certificate Store");
            }
            catch (Exception ex)
            {
                StatusMsg($"Error saving certificate {ex.Message}");
            }
            store.Close();
            return retVal;
        }
    }

    public class CertInfo
    {
        public String FriendlyName { get; set; } = "";
        public CertHash Hash { get; } = new CertHash();
        public String CommonName { get; set; } = "";
        public String Subject { get; set; } = "";
        public DateTime NotBefore { get; set; }
        public DateTime NotAfter { get; set; }
        public String Issuer { get; set; } = "";
    }

    public class CertHash
    {
        public String MD5 { get; set; } = "";
        public String SHA1 { get; set; } = "";
        public String SHA256 { get; set; } = "";
    }
}

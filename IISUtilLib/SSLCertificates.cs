using ACMEClientLib;
using ACMEClientLib.Crypto;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.OpenSsl;
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

        public static void UpdateCertInfo(String alias,  List<Org.BouncyCastle.X509.X509Certificate> certs, CertInfo ci)
        {
            foreach (Org.BouncyCastle.X509.X509Certificate c in certs)
            {
                if (c.GetBasicConstraints() == -1)
                {
                    ci.FriendlyName = alias;
                    ci.Hash.SHA1 = ByteArrayToHexString(DigestUtilities.CalculateDigest("SHA_1", c.GetEncoded()));
                    ci.Hash.SHA256 = ByteArrayToHexString(DigestUtilities.CalculateDigest("SHA_256", c.GetEncoded()));
                    ci.Hash.MD5 = ByteArrayToHexString(DigestUtilities.CalculateDigest("MD5", c.GetEncoded()));
                    ci.CommonName = CertUtil.ExtractCommonName(c.SubjectDN.ToString());
                    ci.NotAfter = c.NotAfter.ToUniversalTime();
                    ci.NotBefore = c.NotBefore.ToUniversalTime();
                    ci.Subject = c.SubjectDN.ToString();
                    ci.Issuer = c.IssuerDN.ToString();
                    break;
                }
            }

        }


        //A good default for certificateStore is WebHosting
        public static CertInfo GetCertInfo(string FileName, string Password, Action<string> StatusMsg)
        {
            CertInfo retVal = new CertInfo();
            String alias = "";
            List<Org.BouncyCastle.X509.X509Certificate> certs = new List<Org.BouncyCastle.X509.X509Certificate>();
            using (var fs = new FileStream(FileName, FileMode.Open))
            {
                fs.Seek(0, SeekOrigin.Begin);

                if (Path.GetExtension(FileName).ToLower() == ".pfx")
                {
                    Pkcs12Store cert = new Pkcs12Store(fs, Password.ToCharArray());
                    foreach (object a in cert.Aliases)
                    {
                        alias = (String)a;
                        X509CertificateEntry[] ces = cert.GetCertificateChain(alias);
                        foreach (X509CertificateEntry ce in ces) certs.Add(ce.Certificate);
                        break;
                    }
                }
                else if (Path.GetExtension(FileName).ToLower() == ".pem")
                {
                    using (var tr = new StreamReader(fs))
                    {
                        Org.BouncyCastle.OpenSsl.PemReader pr = new Org.BouncyCastle.OpenSsl.PemReader(tr, new PasswordFinder(Password));

                        Object certObj = pr.ReadObject();
                        while (certObj != null)
                        {
                            if (certObj is Org.BouncyCastle.X509.X509Certificate) certs.Add(certObj as Org.BouncyCastle.X509.X509Certificate);
                            certObj = pr.ReadObject();
                        }
                    }
                }
                else
                {
                    throw new Exception($"Invalid extension in file: {FileName}");
                }

            }
            UpdateCertInfo(alias, certs, retVal);

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

    public class PasswordFinder : IPasswordFinder
    {
        private string password;

        public PasswordFinder(string password)
        {
            this.password = password;
        }


        public char[] GetPassword()
        {
            return password.ToCharArray();
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

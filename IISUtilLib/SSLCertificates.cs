using ACMEClientLib;
using System;
using System.Collections.Generic;
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
        public static byte[] InstallCertificate(string pfxFilename, string PFXPassword, DNSList DnsIdentifiers, String certificateStore, Action<string> StatusMsg)
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

                StringBuilder certName = new StringBuilder();
                certName.Append(DnsIdentifiers.First());
                if (DnsIdentifiers.Count > 1) certName.Append($"(+{DnsIdentifiers.Count - 1})");
                certName.Append(" Expires: ").Append(certificate.NotAfter.ToString("yyyy/MM/dd HH:mm:ss"));
                certificate.FriendlyName = certName.ToString();
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
}

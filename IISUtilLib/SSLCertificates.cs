using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}

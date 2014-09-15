using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IISUtil
{
    public delegate void SSLOutputMessage(String msg);
    public class SSLCertificates
    {
        public void PrintOutAllCerts(SSLOutputMessage msgOutput)
        {
            var store2 = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
            msgOutput("TrustedPublisher:");
            PrintCerts(store2, msgOutput);

            msgOutput("Personal:");
            store2 = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            PrintCerts(store2, msgOutput);

            msgOutput("CertificateAuthority:");
            store2 = new X509Store(StoreName.CertificateAuthority, StoreLocation.LocalMachine);
            PrintCerts(store2, msgOutput);
        }

        public static String ByteArrayToHexString(byte[] byteArray)
        {
            return BitConverter.ToString(byteArray).Replace("-", "").ToLower();
        }
        public static byte[] HexStringToByteArray(String hexString)
        {
            List<byte> retVal = new List<byte>();
            StringBuilder sb = new StringBuilder();
            for (int idx = 0; idx < hexString.Length; idx++)
            {
                //Get the character if it is a letter or digit
                char chr = hexString[idx];
                if (char.IsLetterOrDigit(chr))
                {
                    sb.Append(chr);
                }

                //Now do the conversion
                //If it is time to do another conversion
                if ((sb.Length > 0) && ((idx == (hexString.Length - 1) || (sb.Length == 2))))
                {
                    retVal.Add(byte.Parse(sb.ToString(), System.Globalization.NumberStyles.HexNumber));
                    //Reset it back to zero
                    sb.Length = 0;
                }
            }
            return retVal.ToArray();
        }


        public void PrintCerts(X509Store store, SSLOutputMessage msgOutput)
        {
            store.Open(OpenFlags.OpenExistingOnly);
            foreach (var cert in store.Certificates)
            {
                msgOutput(String.Format("{0} - {1}", cert.FriendlyName, ByteArrayToHexString(cert.GetCertHash())));
            }
        }

        public static void GetInstalledCertificates(SSLOutputMessage msgOutput)
        {
            (new SSLCertificates()).PrintOutAllCerts(msgOutput);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IISConfigLib
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

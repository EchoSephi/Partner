using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Bill.Helper
{
    public class Tool
    {
        public static IConfigurationRoot ReadFromAppSettings()
        {
            return new ConfigurationBuilder()
                // todo 本機測試時下行要解除mark
                // .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();
        }

        public static string MD5code(string str)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            Byte[] data =
                md5Hasher.ComputeHash((new ASCIIEncoding()).GetBytes(str));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}
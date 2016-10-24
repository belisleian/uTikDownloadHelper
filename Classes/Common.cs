using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;

namespace uTikDownloadHelper
{
    public static class Common
    {
        public static Settings Settings = new Settings();

        public static String SettingsPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AssemblyName);
            }
        }

        public static string AssemblyName
        {
            get
            {
                return typeof(Program).Assembly.GetName().Name;
            }
        }

        public static Version AsssemblyVersion
        {
            get
            {
                return typeof(Program).Assembly.GetName().Version;
            }
        }

        public static string ExecutingAssembly
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                return Uri.UnescapeDataString(uri.Path);
            }
        }

        public static string AssemblyDirectory
        {
            get
            {
                return Path.GetDirectoryName(ExecutingAssembly);
            }
        }
        public static string getUniqueTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), AssemblyName + "." + Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public static string getMD5Hash(string input)
        {
            MD5 md5Hash = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}

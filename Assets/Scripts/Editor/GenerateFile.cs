using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    
    public class GenerateFile
    {
        public List<AssetBundle> assetBundles;
        private static StringBuilder _stringBuilder;

        [MenuItem("FileMD5/Create")]
        public static void GenerateFileMD5()
        {
            _stringBuilder = new StringBuilder();
            var ab = new FileStream(Application.streamingAssetsPath + "/WebGL",FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            var retVal = md5.ComputeHash(ab);
            ab.Close();
            foreach (var t in retVal)
            {
                _stringBuilder.Append(t.ToString("X"));
            }
        }
    }
}
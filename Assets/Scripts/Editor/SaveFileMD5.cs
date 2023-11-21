using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using FileMode = System.IO.FileMode;
using LitJson;

namespace Editor
{
    public class SaveFileMD5 : ScriptableWizard
    {
        public string versions;
        public List<DefaultAsset> assetBundles=new List<DefaultAsset>();
        public List<DefaultAsset> assetDLLs = new List<DefaultAsset>();

        [MenuItem("FileMD5/Generate")]
        public static void GenerateFileMD5()
        {
            DisplayWizard("存储AB包的MD5", typeof(SaveFileMD5), "确定", "取消");
        }

        private void OnWizardCreate()
        {
            var fileMd5 = new FileMD5();
            var stringBuilder = new StringBuilder();
            fileMd5.versions = versions;
            foreach (var path in assetBundles)
            {
                var ab = new FileStream(Application.streamingAssetsPath + $"/{path.name}",FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                var retVal = md5.ComputeHash(ab);
                ab.Close();
                foreach (var t in retVal)
                {
                    stringBuilder.Append(t.ToString("X"));
                }
                
                fileMd5.fileMD5Infos.Add(path.name,stringBuilder.ToString());
            }
            foreach (var path in assetDLLs)
            {
                var ab = new FileStream(Application.streamingAssetsPath + $"/{path.name}.bytes",FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                var retVal = md5.ComputeHash(ab);
                ab.Close();
                foreach (var t in retVal)
                {
                    stringBuilder.Append(t.ToString("X"));
                }
                
                fileMd5.fileMD5Infos.Add(path.name,stringBuilder.ToString());
            }

            var str = JsonMapper.ToJson(fileMd5);
            File.WriteAllText(Application.streamingAssetsPath + "/File.json",str);
        }

        private void OnWizardOtherButton()
        {
            Close();
        }
        
    }
}
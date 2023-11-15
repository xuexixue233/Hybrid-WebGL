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
        public List<DefaultAsset> assetBundles=new List<DefaultAsset>();

        [MenuItem("FileMD5/Generate")]
        public static void GenerateFileMD5()
        {
            DisplayWizard("存储AB包的MD5", typeof(SaveFileMD5), "确定", "取消");
        }

        private void OnWizardCreate()
        {
            var fileMd5 = new FileMD5();
            var stringBuilder = new StringBuilder();
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

                var md5Info = new FileMD5Info
                {
                    fileName = path.name,
                    fileMd5 = stringBuilder.ToString()
                };
                fileMd5.fileMD5Infos.Add(md5Info);
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
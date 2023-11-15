using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;

public class LoadDll : Singleton<LoadDll>
{
    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();
    
    private static Assembly _hotUpdateAss;
    
    private string url = "http://101.37.20.94/StreamingAssets/File.json";
    
    void Start()
    {
        StartCoroutine(DownLoadAssets(this.StartGame));
    }

    void StartGame()
    {
        // 先补充元数据
        LoadMetadataForAOTAssemblies();
        // Editor环境下，HotUpdate.dll.bytes已经被自动加载，不需要加载，重复加载反而会出问题。
#if !UNITY_EDITOR
        _hotUpdateAss = Assembly.Load(ReadBytesFromStreamingAssets("HotUpdate.dll.bytes"));
#else
        _hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
#endif
        
    }

    private static byte[] ReadBytesFromStreamingAssets(string dllName)
    {
        return s_assetDatas[dllName];
    }

    private static void LoadMetadataForAOTAssemblies()
    {
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTMetaAssemblyFiles)
        {
            byte[] dllBytes = ReadBytesFromStreamingAssets(aotDllName);
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
        }
    }
    
    private string GetWebRequestPath(string asset)
    {
        var path = $"{Application.streamingAssetsPath}/{asset}";
        if (!path.Contains("://"))
        {
            path = "file://" + path;
        }
        return path;
    }
    private static List<string> AOTMetaAssemblyFiles { get; } = new List<string>()
    {

    };


    private IEnumerator DownLoadAssets(Action onDownloadComplete)
    {
        var assets = new List<string>
        {
            "WebGL",
            "prefab",
            "HotUpdate.dll.bytes",
        }.Concat(AOTMetaAssemblyFiles);
        
        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        UnityWebRequest localRequest = UnityWebRequest.Get(GetWebRequestPath("File.json"));

        yield return webRequest.SendWebRequest();
        yield return localRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error downloading file: " + webRequest.error);
        }
        else
        {
            string fileContent = webRequest.downloadHandler.text;
            string localFileContent = localRequest.downloadHandler.text;
            // 在这里你可以处理文件内容，比如解析文本或执行其他操作
            var fileMd5 = JsonMapper.ToObject<FileMD5>(fileContent);
            var localMD5=JsonMapper.ToObject<FileMD5>(localFileContent);
            if (fileMd5.versions==localMD5.versions)
            {
                foreach (var asset in assets)
                {
                    string dllPath = GetWebRequestPath(asset);
                    Debug.Log($"start download asset:{dllPath}");
                    UnityWebRequest www = UnityWebRequest.Get(dllPath);
                    yield return www.SendWebRequest();
            
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        // Or retrieve results as binary data
                        byte[] assetData = www.downloadHandler.data;
                        Debug.Log($"dll:{asset}  size:{assetData.Length}");
                        s_assetDatas[asset] = assetData;
                    }
                }
            }
            else
            {
                foreach (var asset in assets)
                {
                    string dllPath = GetWebRequestPath($"http://101.37.20.94/StreamingAssets/{asset}");
                    Debug.Log($"start download asset:{dllPath}");
                    UnityWebRequest www = UnityWebRequest.Get(dllPath);
                    yield return www.SendWebRequest();
            
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        // Or retrieve results as binary data
                        byte[] assetData = www.downloadHandler.data;
                        Debug.Log($"dll:{asset}  size:{assetData.Length}");
                        s_assetDatas[asset] = assetData;
                    }
                }
            }
        }
        

        onDownloadComplete();
    }
}

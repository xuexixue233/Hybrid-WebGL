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

    private string url = $"{Application.streamingAssetsPath}/File.json";

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
        RunMain();
    }

    private static void RunMain()
    {
        AssetBundle ab = AssetBundle.LoadFromMemory(ReadBytesFromStreamingAssets("prefab"));
        GameObject go = ab.LoadAsset<GameObject>("Canvas");
        GameObject.Instantiate(go);
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

        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error downloading file: " + webRequest.error);
        }
        
        foreach (var asset in assets)
        {
            string dllPath = $"{Application.streamingAssetsPath}/{asset}";
            Debug.Log($"start download asset:{dllPath}");
            UnityWebRequest www = UnityWebRequest.Get(dllPath);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                byte[] assetData = www.downloadHandler.data;
                Debug.Log($"dll:{asset}  size:{assetData.Length}");
                s_assetDatas[asset] = assetData;
            }
        }

        onDownloadComplete();
    }
}
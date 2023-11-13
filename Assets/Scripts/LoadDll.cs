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

public class LoadDll : MonoBehaviour
{
    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();
    
    private static Assembly _hotUpdateAss;
    
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
    
        Type type = _hotUpdateAss.GetType("Hello");
        type.GetMethod("Run").Invoke(null, null);

        RunMain();
    }
    
    public static byte[] ReadBytesFromStreamingAssets(string dllName)
    {
        return s_assetDatas[dllName];
    }

    private static void RunMain()
    {
        AssetBundle ab = AssetBundle.LoadFromMemory(ReadBytesFromStreamingAssets("prefab"));
        GameObject go = ab.LoadAsset<GameObject>("Canvas");
        GameObject.Instantiate(go);
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

    IEnumerator DownLoadAssets(Action onDownloadComplete)
    {
        var assets = new List<string>
        {
            "WebGL",
            "prefab",
            "HotUpdate.dll.bytes",
        }.Concat(AOTMetaAssemblyFiles);

        foreach (var asset in assets)
        {
            string dllPath = GetWebRequestPath(asset);
            Debug.Log($"start download asset:{dllPath}");
            UnityWebRequest www = UnityWebRequest.Get(dllPath);
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
#else
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.Log(www.error);
            }
#endif
            else
            {
                // Or retrieve results as binary data
                byte[] assetData = www.downloadHandler.data;
                Debug.Log($"dll:{asset}  size:{assetData.Length}");
                s_assetDatas[asset] = assetData;
            }
        }

        onDownloadComplete();
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Write : MonoBehaviour
{
    private string url = "http://101.37.20.94/StreamingAssets/File.json";
    private UnityWebRequest request;
    private AssetBundle ab;

    void Start()
    {
        StartCoroutine(MyUnityWebRequest());
    }


    void Update()
    {
    }

    //获取服务器上的资源
    IEnumerator MyUnityWebRequest()
    {
        string uri = "http://101.37.20.94/StreamingAssets/WebGL"; //服务器地址

        request = UnityWebRequest.Get(uri); //要使用这种方式获取资源,才能访问到他的字节流

        //这个方法主要是用来获取AssetBundle资源的,但他下载到的数据是不给访问的,如果想访问或者保存下载回来的数据用上面的UnityWebRequest.Get()
        //request = UnityWebRequestAssetBundle.GetAssetBundle(pathMP);

        yield return request.SendWebRequest();

        //保存下载回来的文件
        if (request.isDone)
        {
            //构造文件流
            FileStream fs = File.Create(Application.persistentDataPath + "/WebGL");
            //将字节流写入文件里,request.downloadHandler.data可以获取到下载资源的字节流
            fs.Write(request.downloadHandler.data, 0, request.downloadHandler.data.Length);

            fs.Flush(); //文件写入存储到硬盘
            fs.Close(); //关闭文件流对象
            fs.Dispose(); //销毁文件对象
        }

        //这里我们直接使用异步加载的一个变形同步加载,获取下载回来的资源
        //ab = AssetBundle.LoadFromMemory(request.downloadHandler.data);

        //ab = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
    }
    
    IEnumerator MyData()
    {
        //加载保存在本地的资源
        request = UnityWebRequestAssetBundle.GetAssetBundle(Application.persistentDataPath + "/res.ab");

        yield return request.SendWebRequest();

        ab = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;

       
    }
}
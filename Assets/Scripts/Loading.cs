using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public TMP_Text loadingText;
    public Slider slider;

    private string url = "http://101.37.20.94/StreamingAssets/File.json";
    
    private void Awake()
    {
        
    }

    void Start()
    {
        StartCoroutine(DownloadFile(url));
    }

    
    void Update()
    {
        
    }
    
    IEnumerator DownloadFile(string fileURL)
    {
        UnityWebRequest request = UnityWebRequest.Get(fileURL);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error downloading file: " + request.error);
        }
        else
        {
            // 请求成功，获取文件内容
            string fileContent = request.downloadHandler.text;
            Debug.Log("Downloaded file content: " + fileContent);

            // 在这里你可以处理文件内容，比如解析文本或执行其他操作
        }
    }
}

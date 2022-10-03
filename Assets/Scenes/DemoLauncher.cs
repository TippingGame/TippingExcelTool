using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DemoLauncher : MonoBehaviour
{
    public Text text;
    void Start()
    {
        //读取二进制文件
        DataManager.Instance.LoadAll();
        text.text += DataManager.Instance.GetfasdffByID(1).name;
        Debug.Log(DataManager.Instance.GetfasdffByID(1).name);
        foreach (var VARIABLE in DataManager.Instance.GetfasdffByID(33).llliststr)
        {
            foreach (var VARIABLE2 in VARIABLE)
            {
                text.text += VARIABLE2;
                Debug.Log(VARIABLE2);
            }
        }
    }
    // IEnumerator Start()
    // {
    //     //由于安卓资源都在包内，需要先复制到可读写文件夹1
    //     string assetPath = URLSetting.STREAMINGASSETS_URL + "config";
    //     string[] paths = null;
    //     WWW www = new WWW(assetPath + "/fileindex.txt");
    //     yield return www;
    //     if (www.error != null)
    //     {
    //         Debug.Log(www.error);
    //         yield return null;
    //     }
    //     else
    //     {
    //         string ss = www.text;
    //         paths = ss.Split("\n", StringSplitOptions.RemoveEmptyEntries);
    //     }
    //
    //     for (int i = 0; i < paths.Length; i++)
    //     {
    //         yield return CopyAssets(paths[i].Replace("\r", ""));
    //     }
    //     //读取Excel文件
    //     ReadExcel.Instance.LoadAllExcelData();
    //     text.text += DataManager.Instance.GetfasdffByID(115).name;
    //     Debug.Log(DataManager.Instance.GetfasdffByID(115).name);
    //     foreach (var VARIABLE in DataManager.Instance.GetfasdffByID(113).llliststr)
    //     {
    //         foreach (var VARIABLE2 in VARIABLE)
    //         {
    //             text.text += VARIABLE2;
    //             Debug.Log(VARIABLE2);
    //         }
    //     }
    // }
    // //由于安卓资源都在包内，需要先复制到可读写文件夹2
    // IEnumerator CopyAssets(string paths)
    // {
    //     string assetPath = URLSetting.STREAMINGASSETS_URL + "config";
    //     string sdCardPath = Application.persistentDataPath + "/config";
    //     WWW www = new WWW(assetPath + "/" + paths);
    //     yield return www;
    //     if(www.error != null)
    //     {
    //         Debug.Log(www.error);
    //         yield return null;
    //     }
    //     else
    //     {
    //         FileTools.SafeWriteAllBytes(sdCardPath + "/" + paths, www.bytes);
    //     }
    // }
}

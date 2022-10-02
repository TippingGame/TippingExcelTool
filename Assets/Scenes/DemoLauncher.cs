using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DemoLauncher : MonoBehaviour
{
    private DataManager DataManager = DataManager.Instance;
    private ReadExcel ReadExcel = ReadExcel.Instance;
    public Text text;
    // void Start()
    // {
        //读取二进制文件
        // DataManager.LoadAll();
        // text.text += DataManager.GetfasdffByID(1).name;
        // Debug.Log(DataManager.GetfasdffByID(1).name);
        // foreach (var VARIABLE in DataManager.GetfasdffByID(33).llliststr)
        // {
        //     foreach (var VARIABLE2 in VARIABLE)
        //     {
        //         text.text += VARIABLE2;
        //         Debug.Log(VARIABLE2);
        //     }
        // }
        
    // }
    IEnumerator Start()
    {
        string assetPath = URLSetting.STREAMINGASSETS_URL + "config";
        string[] paths = null;
        WWW www = new WWW(assetPath + "/fileindex.txt");
        yield return www;
        if (www.error != null)
        {
            Debug.Log(www.error);
            yield return null;
        }
        else
        {
            string ss = www.text;
            paths = ss.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        }

        for (int i = 0; i < paths.Length; i++)
        {
            yield return CopyAssets(paths[i].Replace("\r", ""));
        }
        //读取Excel文件
        ReadExcel.LoadAllExcelData();
        text.text += DataManager.GetfasdffByID(115).name;
        Debug.Log(DataManager.GetfasdffByID(115).name);
        foreach (var VARIABLE in DataManager.GetfasdffByID(113).llliststr)
        {
            foreach (var VARIABLE2 in VARIABLE)
            {
                text.text += VARIABLE2;
                Debug.Log(VARIABLE2);
            }
        }
    }
    //由于安卓资源都在包内，需要先复制到可读写文件夹
    IEnumerator CopyAssets(string paths)
    {
        string assetPath = URLSetting.STREAMINGASSETS_URL + "config";
        string sdCardPath = Application.persistentDataPath + "/config";
        WWW www = new WWW(assetPath + "/" + paths);
        yield return www;
        if(www.error != null)
        {
            Debug.Log(www.error);
            yield return null;
        }
        else
        {
            FileTools.SafeWriteAllBytes(sdCardPath + "/" + paths, www.bytes);
        }
    }
}

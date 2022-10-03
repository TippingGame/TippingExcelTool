# TippingExcelTool

[![license](http://img.shields.io/badge/license-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![Unity Version](https://img.shields.io/badge/unity-2021.3.8f1-blue)](https://unity.com)
[![Platform](https://img.shields.io/badge/platform-Win%20%7C%20Android%20%7C%20iOS%20%7C%20Mac%20%7C%20Linux-orange)]()

# 简介
Unity读取Excel工具，自动生成C#索引字段，能读取二进制文件和Excel文件，游戏运行时修改excel数据，支持Win/Android/iOS/Mac/Linux

## 使用流程
导入文件,打开SampleScene场景，点击菜单栏->开发工具->导入配置表  
游戏运行时修改excel数据，点击菜单栏->开发工具->重新读取Excel  

### 读取二进制文件
```C#
        DataManager.Instance.LoadAll();
```
### 读取Excel文件
```C#
        ReadExcel.Instance.LoadAllExcelData();
```
### 打印数据
```C#
        Debug.Log(DataManager.Instance.GetfasdffByID(115).name);
        foreach (var VARIABLE in DataManager.Instance.GetfasdffByID(113).llliststr)
        {
            foreach (var VARIABLE2 in VARIABLE)
            {
                Debug.Log(VARIABLE2);
            }
        }
```

## Excel格式
基础类型
|int| long | float | double | str |
| - | - | - | - | - |
| id | long1 | float1 | double1 | str1 |
|1|9935434343|2.725412|1.346655321|读取Excel工具|
|2|9935434343|2.725412|1.346655321|读取Excel工具|

拓展类型
| l_int | l_float | l_str | l_obj | l_l_int | l_l_float | l_l_str |
| - | - | - | - | - | - | - |
| l_int1 | l_float1 | l_str1 | l_obj1 | l_l_int1 | l_l_float1 | l_l_str1 |
|[1,5]|[1.5,5.8]|[文件,支持]|["生成",656,1.235999]|{[1,6],[2,8]}|{[6.215,6.12],[2.5,14.556]}|{[自动,格式],[tipping,excel]}|
|[1,5]|[1.5,5.8]|[文件,支持]|["生成",656,1.235999]|{[1,6],[2,8]}|{[6.215,6.12],[2.5,14.556]}|{[自动,格式],[tipping,excel]}|

## 目录结构
---Assets  
----Editor（编辑器代码）  
----Plugins（库文件/生成的字段索引库）  
----Resources（生成的二进制文件）  
----Scenes（场景Demo）  
----Scripts（游戏代码）  
----StreamingAssets（Excel存放位置）  

## 使用到的库
Excel.dll  
I18N.CJK.dll  
I18N.dll  
I18N.MidEast.dll  
I18N.Other.dll  
I18N.Rare.dll  
I18N.West.dll  
ICSharpCode.SharpZipLib.dll  

## 注意  
#### 由于Android资源都在包内，在Android上使用，需要先复制到可读写文件夹中再进行读取
```C#
    IEnumerator Start()
    {
        //由于安卓资源都在包内，需要先复制到可读写文件夹1
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
        ReadExcel.Instance.LoadAllExcelData();
        text.text += DataManager.Instance.GetfasdffByID(115).name;
        Debug.Log(DataManager.Instance.GetfasdffByID(115).name);
        foreach (var VARIABLE in DataManager.Instance.GetfasdffByID(113).llliststr)
        {
            foreach (var VARIABLE2 in VARIABLE)
            {
                text.text += VARIABLE2;
                Debug.Log(VARIABLE2);
            }
        }
    }
    //由于安卓资源都在包内，需要先复制到可读写文件夹2
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
```

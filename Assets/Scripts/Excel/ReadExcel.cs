using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Excel;

public class SupportType
{
    public const string INT = "int";
    public const string LONG = "long";
    public const string FLOAT = "float";
    public const string DOUBLE = "double";
    public const string STRING = "str";
    public const string LIST_OBJ = "l_obj";
    public const string LIST_INT = "l_int";
    public const string LIST_FLOAT = "l_float";
    public const string LIST_STRING = "l_str";
    public const string LIST_LIST_INT = "l_l_int";
    public const string LIST_LIST_FLOAT = "l_l_float";
    public const string LIST_LIST_STRING = "l_l_str";
}
public class ReadExcel : Singleton<ReadExcel>
{
    private const string CODE_NAMESPACE = "ExcelDataClass"; //由表生成的数据类型均在此命名空间内
    private const string ExcelPath = "config"; //需要导表的目录
    private Dictionary<string, List<ConfigData[]>> dataDict; //存放所有数据表内的数据，key：类名  value：数据
    public void LoadAllExcelData()
    {
        string INPUT_PATH = Application.persistentDataPath + "/" + ExcelPath;

        if (string.IsNullOrEmpty(INPUT_PATH))
        {
            throw new Exception("请先设置数据表路径！");
        }

        var files = Directory.GetFiles(INPUT_PATH, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".xls") || s.EndsWith(".xlsx")).ToArray();
        if (files == null || files.Length == 0)
        {
            throw new Exception("暂无可以导入的数据表！");
        }

        if (dataDict == null)
        {
            dataDict = new Dictionary<string, List<ConfigData[]>>();
        }
        else
        {
            dataDict.Clear();
        }

        float step = 1f;
        foreach (string item in files)
        {
            step++;
            GetExcelData(item);
        }
        var assembly = Assembly.Load(CODE_NAMESPACE);
        step = 1;
        Dictionary<String, System.Object> objs = new Dictionary<string, object>();
        foreach (KeyValuePair<string, List<ConfigData[]>> each in dataDict)
        {
            step++;
            Type temp = assembly.GetType(CODE_NAMESPACE + "." + each.Key + "Item");
            object container = assembly.CreateInstance(CODE_NAMESPACE + "." + each.Key);
            //序列化数据
            Serialize(container, temp, each.Value);
            objs.Add(each.Key,container);
        }
        DataManager.Instance.RuntimeLoadAll(objs);
        Debug.Log("<color=green>导表成功!</color>");
    }
    
    //数据表内每一格数据
    class ConfigData
    {
        public string Type; //数据类型
        public string Name; //字段名
        public string Data; //数据值
    }

    private void GetExcelData(string inputPath)
    {
        FileStream stream = null;
        try
        {
            stream = File.Open(inputPath, FileMode.Open, FileAccess.ReadWrite);
        }
        catch
        {
            throw new Exception("请关闭 " + inputPath + " 后再导表！");
        }

        IExcelDataReader excelReader = null;
        if (inputPath.EndsWith(".xls")) excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
        else if (inputPath.EndsWith(".xlsx")) excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        if (!excelReader.IsValid)
        {
            throw new Exception("无法读取的文件:  " + inputPath);
        }
        else
        {
            // do //暂时只读第一个sheet
            // {
            // sheet name
            string className = excelReader.Name;
            string[] types = null; //数据类型
            string[] names = null; //字段名
            List<ConfigData[]> dataList = new List<ConfigData[]>();
            int index = 1;

            //开始读取
            while (excelReader.Read())
            {
                //这里读取的是每一行的数据
                string[] datas = new string[excelReader.FieldCount];
                for (int j = 0; j < excelReader.FieldCount; ++j)
                {
                    datas[j] = excelReader.GetString(j);
                }

                //空行不处理
                if (datas.Length == 0 || string.IsNullOrEmpty(datas[0]))
                {
                    ++index;
                    continue;
                }

                //第4行表示类型
                if (index == 1) types = datas;
                //第5行表示变量名
                else if (index == 2) names = datas;
                //后面的表示数据
                else if (index > 2)
                {
                    //把读取的数据和数据类型,名称保存起来,后面用来动态生成类
                    List<ConfigData> configDataList = new List<ConfigData>();
                    for (int j = 0; j < datas.Length; ++j)
                    {
                        ConfigData data = new ConfigData();
                        data.Type = types[j];
                        data.Name = names[j];
                        data.Data = datas[j];
                        if (string.IsNullOrEmpty(data.Type) || string.IsNullOrEmpty(data.Data)) continue; //空的数据不处理
                        configDataList.Add(data);
                    }

                    dataList.Add(configDataList.ToArray());
                }

                ++index;
            }

            if (string.IsNullOrEmpty(className))
            {
                throw new Exception("空的类名（excel页签名）, 路径:  " + inputPath);
            }

            if (names != null && types != null)
            {
                if (dataDict.ContainsKey(className))
                {
                    throw new Exception("类名重复: " + className + " ,路径:  " + inputPath);
                }
                dataDict.Add(className, dataList);
                Debug.Log(className);
            }
            // }
            // while (excelReader.NextResult());//excelReader.NextResult() Excel表下一个sheet页有没有数据
        }

        stream.Dispose();
        stream.Close();
    }
    
 //序列化对象
    private static void Serialize(object container, Type temp, List<ConfigData[]> dataList)
    {
        //设置数据
        foreach (ConfigData[] datas in dataList)
        {
            //Type.FullName 获取该类型的完全限定名称，包括其命名空间，但不包括程序集。
            object t = temp.Assembly.CreateInstance(temp.FullName);
            foreach (ConfigData data in datas)
            {
                //Type.GetField(String) 搜索Type内指定名称的公共字段。
                FieldInfo info = temp.GetField(data.Name);
                // FieldInfo.SetValue 设置对象内指定名称的字段的值
                if (info != null)
                {
                    info.SetValue(t, ParseValue(data.Type, data.Data, temp.Name));
                }
                else
                {
                    //2019.4.28f1,2020.3.33f1都出现的BUG（2021.3.8f1测试通过），编译dll后没及时刷新，导致修改name或id后读取失败，需要二次编译
                    Debug.Log("info是空的：" + data.Name);
                }
            }
            
            // FieldInfo.GetValue 获取对象内指定名称的字段的值
            object id = temp.GetField("id").GetValue(t); //获取id
            FieldInfo dictInfo = container.GetType().GetField("Dict");
            object dict = dictInfo.GetValue(container);

            bool isExist = (bool) dict.GetType().GetMethod("ContainsKey").Invoke(dict, new System.Object[] {id});
            if (isExist)
            {
                throw new Exception("ID重复：" + id + "，类型： " + container.GetType().Name);
            }
            dict.GetType().GetMethod("Add").Invoke(dict, new System.Object[] {id, t});
        }
    }
    
    private static void DebugError(string type, string data, string classname)
    {
        Debug.LogError(string.Format("数据类型错误：{0}==={1}==={2}", type, data, classname));
    }

    public static object ParseValue(string type, string data, string classname)
    {
        object o = null;
        try
        {
            switch (type)
            {
                case SupportType.INT:
                    int INT_int;
                    if (int.TryParse(data, out INT_int) == false)
                    {
                        DebugError(type, data, classname);
                        o = 0;
                    }

                    o = INT_int;
                    break;
                case SupportType.LONG:
                    long LONG_long;
                    if (long.TryParse(data, out LONG_long) == false)
                    {
                        DebugError(type, data, classname);
                        o = 0;
                    }

                    o = LONG_long;
                    break;
                case SupportType.FLOAT:
                    float FLOAT_float;
                    if (float.TryParse(data, out FLOAT_float) == false)
                    {
                        DebugError(type, data, classname);
                        o = 0;
                    }

                    o = FLOAT_float;
                    break;
                case SupportType.DOUBLE:
                    double DOUBLE_double;
                    if (double.TryParse(data, out DOUBLE_double) == false)
                    {
                        DebugError(type, data, classname);
                        o = 0;
                    }

                    o = DOUBLE_double;
                    break;
                case SupportType.STRING:
                    o = data;
                    break;
                case SupportType.LIST_OBJ:
                    data = data.Substring(1, data.Length - 2); //移除 '['   ']'
                    string[] ts = data.Split(','); //逗号分隔
                    System.Collections.ArrayList arlist = new System.Collections.ArrayList();
                    foreach (var item in ts)
                    {
                        if (item.EndsWith("\"") && item.StartsWith("\""))
                        {
                            string _str = item.TrimEnd('\"').TrimStart('\"');
                            arlist.Add(_str);
                        }
                        else if (item.Contains("."))
                        {
                            arlist.Add((float) ParseValue(SupportType.FLOAT, item, classname));
                        }
                        else
                        {
                            arlist.Add((int) ParseValue(SupportType.INT, item, classname));
                        }
                    }

                    o = arlist;
                    break;
                case SupportType.LIST_INT:
                    data = data.Substring(1, data.Length - 2); //移除 '['   ']'
                    string[] ints = data.Split(','); //逗号分隔
                    List<int> list = new List<int>();
                    foreach (var item in ints)
                    {
                        list.Add((int) ParseValue(SupportType.INT, item, classname));
                    }

                    o = list;
                    break;
                case SupportType.LIST_FLOAT:
                    data = data.Substring(1, data.Length - 2); //移除 '['   ']'
                    string[] floats = data.Split(','); //逗号分隔
                    List<float> list2 = new List<float>();
                    foreach (var item in floats)
                    {
                        list2.Add((float) ParseValue(SupportType.FLOAT, item, classname));
                    }

                    o = list2;
                    break;
                case SupportType.LIST_STRING:
                    data = data.Substring(1, data.Length - 2); //移除 '['   ']'
                    string[] strs = data.Split(','); //逗号分隔
                    List<string> list3 = new List<string>();
                    foreach (var item in strs)
                    {
                        list3.Add(item);
                    }

                    o = list3;
                    break;
                case SupportType.LIST_LIST_INT:
                    data = data.Substring(1, data.Length - 2); //移除 '['   ']'
                    var arr4 = Regex.Matches(data, @"\[[^\[\]]+?\]")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();
                    List<List<int>> list4 = new List<List<int>>();
                    foreach (var item in arr4)
                    {
                        list4.Add((List<int>) ParseValue(SupportType.LIST_INT, item, classname));
                    }

                    o = list4;
                    break;
                case SupportType.LIST_LIST_FLOAT:
                    data = data.Substring(1, data.Length - 2); //移除 '['   ']'
                    var arr5 = Regex.Matches(data, @"\[[^\[\]]+?\]")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();
                    List<List<float>> list5 = new List<List<float>>();
                    foreach (var item in arr5)
                    {
                        list5.Add((List<float>) ParseValue(SupportType.LIST_FLOAT, item, classname));
                    }

                    o = list5;
                    break;
                case SupportType.LIST_LIST_STRING:
                    data = data.Substring(1, data.Length - 2); //移除 '['   ']'
                    var arr6 = Regex.Matches(data, @"\[[^\[\]]+?\]")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();
                    List<List<string>> list6 = new List<List<string>>();
                    foreach (var item in arr6)
                    {
                        list6.Add((List<string>) ParseValue(SupportType.LIST_STRING, item, classname));
                    }

                    o = list6;
                    break;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("\n错误的数据值:" + data + "\n位于:" + classname, ex);
        }

        return o;
    }
}
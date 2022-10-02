using System.Collections.Generic;
using System.Collections;
using System.Text;
using System;
using UnityEngine;
using System.IO;
using System.Reflection;

//脚本生成器
public class ScriptGenerator
{
    private string[] Names;
    private string[] Types;
    private string ClassName;
    private string InputPath;

    public ScriptGenerator(string inputPath, string className, string[] fileds, string[] types)
    {
        InputPath = inputPath;
        ClassName = className;
        Names = fileds;
        Types = types;
    }

    //开始生成脚本
    public string Generate()
    {
        if (Types == null || Names == null || ClassName == null)
            throw new Exception("表名:" + ClassName +
                                "\n表名为空:" + (ClassName == null) +
                                "\n字段类型为空:" + (Types == null) +
                                "\n字段名为空:" + (Names == null));
        return CreateCode(ClassName, Types, Names);
    }

    //创建代码。   
    private string CreateCode(string ClassName, string[] types, string[] fields)
    {
        //生成类
        StringBuilder classSource = new StringBuilder();
        classSource.Append("/*Auto create\n");
        classSource.Append("Don't Edit it*/\n");
        classSource.Append("\n");
        classSource.Append("using System;\n");
        classSource.Append("using System.Reflection;\n");
        classSource.Append("using System.Collections.Generic;\n");
        classSource.Append("namespace " + ExcelDataTool.CODE_NAMESPACE + "\n");
        classSource.Append("{\n");
        classSource.Append("[Serializable]\n");
        classSource.Append("public class " + ClassName + "Item\n"); //表里每一条数据的类型名为表类型名加Item
        classSource.Append("{\n");
        //设置成员
        for (int i = 0; i < fields.Length; ++i)
        {
            classSource.Append(PropertyString(types[i], fields[i]));
        }

        classSource.Append("}\n");

        //生成Container
        classSource.Append("\n");
        classSource.Append("[Serializable]\n");
        classSource.Append("public class " + ClassName + "\n");
        classSource.Append("{\n");
        string idType = "";
        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i] == "id" || fields[i] == "ID" || fields[i] == "iD" || fields[i] == "Id")
            {
                idType = GetTrueType(types[i]);
                break;
            }
        }

        classSource.Append("\tpublic " + "Dictionary<" + idType + ", " + ClassName + "Item" + " > " + " Dict" +
                           " = new Dictionary<" + idType + ", " + ClassName + "Item" + ">();\n");
        classSource.Append("}\n");
        classSource.Append("}\n");
        return classSource.ToString();
        /*  //生成的条目数据类
            namespace ExcelDataClass
            {
                public class testItem
                {
                    public int id;
                    public float m_float;
                    public string str;
                    public test();
                }
            }
            //生成的表数据类
            using System.Collections.Generic;
            {
                public class test
                {
                    public Dictionary<int, test> Dict;
                    public testContainer();
                }
            }
         */
    }

    private string PropertyString(string type, string propertyName)
    {
        if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(propertyName))
            return null;

        type = GetTrueType(type);
        if (!string.IsNullOrEmpty(type))
        {
            StringBuilder sbProperty = new StringBuilder();
            if (type.EndsWith("[]"))
            {
                string[] stypestr = type.Split(' ');
                sbProperty.Append("\tpublic " + propertyName + " " + type + ";\n");
            }
            else
            {
                sbProperty.Append("\tpublic " + type + " " + propertyName + ";\n");
            }

            return sbProperty.ToString();
        }
        else
        {
            return "";
        }
    }

    private string GetTrueType(string type)
    {
        switch (type)
        {
            case SupportType.INT:
                type = "int";
                break;
            case SupportType.LONG:
                type = "long";
                break;
            case SupportType.FLOAT:
                type = "float";
                break;
            case SupportType.DOUBLE:
                type = "double";
                break;
            case SupportType.STRING:
                type = "string";
                break;
            case SupportType.LIST_OBJ:
                type = "System.Collections.ArrayList";
                break;
            case SupportType.LIST_INT:
                type = "List<int>";
                break;
            case SupportType.LIST_FLOAT:
                type = "List<float>";
                break;
            case SupportType.LIST_STRING:
                type = "List<string>";
                break;
            case SupportType.LIST_LIST_INT:
                type = "List<List<int>>";
                break;
            case SupportType.LIST_LIST_FLOAT:
                type = "List<List<float>>";
                break;
            case SupportType.LIST_LIST_STRING:
                type = "List<List<string>>";
                break;
            default:
                ProgressBar.HideBarWithFailInfo("\n输入了错误的数据类型: " + type + ", 类名: " + ClassName + ", 位于: " + InputPath);
                throw new Exception("输入了错误的数据类型:  " + type + ", 类名:  " + ClassName + ", 位于:  " + InputPath);
        }

        return type;
    }

    //创建数据管理器脚本
    public static void CreateDataManager(Assembly assembly)
    {
        List<Type> list = new List<Type>();
        list.AddRange(assembly.GetTypes());
        IEnumerable types = list.FindAll(t => { return !t.Name.Contains("Item"); });

        StringBuilder source = new StringBuilder();
        source.Append("/*\n");
        source.Append(" *   This file was generated by a tool.\n");
        source.Append(" *   Do not edit it, otherwise the changes will be overwritten.\n");
        source.Append(" */\n");
        source.Append("\n");

        source.Append("using System;\n");
        source.Append("using System.Collections.Generic;\n");
        source.Append("using UnityEngine;\n");
        source.Append("using System.Runtime.Serialization;\n");
        source.Append("using System.Runtime.Serialization.Formatters.Binary;\n");
        source.Append("using System.IO;\n");
        source.Append("using " + ExcelDataTool.CODE_NAMESPACE + ";\n\n");
        source.Append("[Serializable]\n");
        source.Append("public class DataManager : Singleton<DataManager>\n");
        source.Append("{\n");

        //定义变量
        foreach (Type t in types)
        {
            source.Append("\tprivate " + t.Name + " p_" + t.Name + ";\n");
        }

        source.Append("\n");

        //定义方法
        foreach (Type t in types)
        {
            string typeName = t.Name + "Item"; //类型名
            string typeNameNotItem = t.Name; //类型名没item
            string funcName = typeName.Remove(1).ToUpper() + typeName.Substring(1); //将类型名第一个字母大写
            List<FieldInfo> fields = new List<FieldInfo>();
            fields.AddRange(list.Find(temp => temp.Name == typeName).GetFields()); //获取数据类的所有字段信息
            string idType = fields.Find(f => f.Name == "id" || f.Name == "ID" || f.Name == "iD" || f.Name == "Id")
                .FieldType.Name; //获取id的数据类型
            source.Append("\tpublic " + typeName + " Get" + typeNameNotItem + "ByID" + "(" + idType + " id)\n");
            source.Append("\t{\n");
            source.Append("\t\t" + typeName + " t = null;\n");
            source.Append("\t\tp_" + t.Name + ".Dict.TryGetValue(id, out t);\n");
            source.Append("\t\tif (t == null) Debug.LogError(" + '"' + "can't find the id " + '"' + " + id " + "+ " +
                          '"' + " in " + t.Name + '"' + ");\n");
            source.Append("\t\treturn t;\n");
            source.Append("\t}\n\n");
            
            source.Append("\tpublic Dictionary<int, " + typeName +">" + " Get" + typeNameNotItem + "()\n");
            source.Append("\t{\n");
            source.Append("\t\treturn p_" + t.Name + ".Dict;\n");
            source.Append("\t}\n\n");
        }

        //加载所有配置表
        source.Append("\tpublic void LoadAll()\n");
        source.Append("\t{\n");
        foreach (Type t in types)
        {
            source.Append("\t\tp_" + t.Name + " = Load(" + '"' + t.Name + '"' + ") as " + t.Name + ";\n");
        }

        source.Append("\t}\n\n");
        
        //运行时加载所有配置表
        source.Append("\tpublic void RuntimeLoadAll(Dictionary<String, System.Object> objs)\n");
        source.Append("\t{\n");
        foreach (Type t in types)
        {
            source.Append("\t\tp_" + t.Name + " = objs[" + '"' + t.Name + '"' + "] as " + t.Name + ";\n");
        }

        source.Append("\t}\n\n");

        //反序列化
        source.Append("\tprivate System.Object Load(string name)\n");
        source.Append("\t{\n");
        source.Append("\t\tIFormatter f = new BinaryFormatter();\n");
        source.Append("\t\tTextAsset text = Resources.Load<TextAsset>(" + '"' + ExcelDataTool.BinDataFolderLoadName +
                      "/" + '"' + " + name);\n");
        source.Append("\t\tStream s = new MemoryStream(text.bytes);\n");
        source.Append("\t\tSystem.Object obj = f.Deserialize(s);\n");
        source.Append("\t\ts.Close();\n");
        source.Append("\t\treturn obj;\n");
        source.Append("\t}\n");
        source.Append("}\n");

        //保存脚本
        string path = Application.dataPath + ExcelDataTool.DataManagerFolder;
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        StreamWriter sw = new StreamWriter(path + "/" + ExcelDataTool.DataManagerName);
        sw.WriteLine(source.ToString());
        Debug.Log("已生成 " + path + "/<color=#FFFF00>" + ExcelDataTool.DataManagerName + "</color>");
        sw.Close();

        /*  //生成的数据管理类如下
            using System;
            using UnityEngine;
            using System.Runtime.Serialization;
            using System.Runtime.Serialization.Formatters.Binary;
            using System.IO;
            using ExcelConfigClass;
            [Serializable]
            public class DataManager : Singleton<DataManager>
            {
                public test p_test;
                public test2 p_test2;
                public testItem GetTestByID(Int32 id)
                {
                    testItem t = null;
                    p_test.Dict.TryGetValue(id, out t);
                    if (t == null) Debug.LogError("can't find the id " + id + " in test");
                    return t;
                }
                public test2Item GetTest2ByID(String id)
                {
                    test2Item t = null;
                    p_test2.Dict.TryGetValue(id, out t);
                    if (t == null) Debug.LogError("can't find the id " + id + " in test2");
                    return t;
                }
                public void LoadAll()
                {
                    p_test = Load("test") as test;
                    p_test2 = Load("test2") as test2;
                }
                private System.Object Load(string name)
                {
                    IFormatter f = new BinaryFormatter();
                    TextAsset text = Resources.Load<TextAsset>("BinConfigData/" + name);
                    Stream s = new MemoryStream(text.bytes);
                    System.Object obj = f.Deserialize(s);
                    s.Close();
                    return obj;
                }
            }
        */
    }
}
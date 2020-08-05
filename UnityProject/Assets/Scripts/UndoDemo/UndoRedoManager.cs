#region Copyright © 2018 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    UndoRedoManager.cs
 Author:      Zeng Zhiwei
 Time:        2019/11/7 10:07:40
=====================================================
*/
#endregion

//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using UnityEngine;

public class UndoRedoManager
{
    #region fields
    private int selectIndex;
    private static CommandPool commandPool;
    //private string m_strFilePath = "Assets/CommandLog.json";
    private List<Command> m_allCommands;
    //private int m_intMaxCommandCount;
    #endregion

    #region properties
    public static CommandPool CommandPool
    {
        get
        {
            return commandPool;
        }
    }
    public int SelectIndex { get { return selectIndex; } }
    public int TotalCommandCount
    {
        get
        {
            return commandPool.TotalCommandCount;
        }
    }

    // 获取撤销字符串列表
    public string[] GetUndoCommandStringList()
    {
        if (TotalCommandCount <= 0)
        {
            return null;
        }
        string[] s = new string[TotalCommandCount + 1];
        s[0] = "打开文件";
        int i = 1;
        foreach (Command item in commandPool.UndoStack)
        {
            s[i] = item.ToString();
            i++;
        }

        foreach (Command item in commandPool.RedoStack)
        {
            s[i] = item.ToString();
            i++;
        }
        return s;
    }
    #endregion

    #region methods
    public UndoRedoManager(int maxCommandCount)
    {
        //this.m_intMaxCommandCount = maxCommandCount;

        commandPool = new CommandPool(maxCommandCount);
        m_allCommands = new List<Command>();
        Deserialization();
    }

    public int AddCommand(Command cmd)
    {
        commandPool.Register(cmd);
        m_allCommands.Add(cmd);
        selectIndex = TotalCommandCount;
        return selectIndex;
    }

    public void Undo()
    {
        Command cmd = commandPool.GetNextUndoCommand();
        Debug.Log("撤销："+cmd.ToString());

        commandPool.Undo();
        m_allCommands.Remove(cmd);
        Debug.Log(commandPool.ToString());
        selectIndex -= 1;
    }

    public void Redo()
    {
        Command cmd = commandPool.GetNextRedoCommand();
        Debug.Log("重做：" + cmd.ToString());

        commandPool.Redo();
        m_allCommands.Add(cmd);

        Debug.Log(commandPool.ToString());
        selectIndex += 1;
    }

    // 通过名字拿到类型
    //public Type GetTypeByName(string typeName)
    //{
    //    //普遍的命名空间和程序集
    //    var resolvedTypeName = string.Format("{0}, Assembly-CSharp-Editor", typeName);
    //    return Type.GetType(resolvedTypeName, true);
    //}

    //public System.Object GetClassByName(string strClass)
    //{
    //    Type type = Type.GetType(strClass);                             // 通过类名获取同名类
    //    System.Object obj = System.Activator.CreateInstance(type);      // 创建实例
    //    return obj;
    //    //string strMethod = "Method";                                  // 方法名
    //    //MethodInfo method = type.GetMethod(strMethod, new Type[] { });// 获取方法信息

    //    //method.Invoke(obj, parameters);                               // 调用方法，参数为空
    //}

    // 反序列化，然后塞入历史记录列表,需要实现用命令还原操作
    public void Deserialization()
    {
        //string text = FileHelper.ReadTextFromFile(m_strFilePath);
        //if (!string.IsNullOrEmpty(text))
        //{
        //    //m_allCommands = JsonConvert.DeserializeObject(text);
        //    //m_allCommands = JsonConvert.DeserializeObject<List<Command>>(text,jsonSettings);
        //    try
        //    {
        //        var jArray = (JArray)JsonConvert.DeserializeObject(text);

        //        foreach (JObject jObject in jArray)
        //        {
        //            Command c;
        //            TerrainItemParam param = new TerrainItemParam
        //            {
        //                x = jObject.Value<int>("parentX"),
        //                y = jObject.Value<int>("parentY"),
        //                name = jObject.Value<string>("prefabName"),
        //                rotationY = jObject.Value<int>("rotationIndex") * -45,
        //                itemType = jObject.Value<int>("itemType"),
        //            };

        //            List<Grid2D> coverGrids = new List<Grid2D>();
        //            foreach (JObject item in jObject["coverGrids"])
        //            {
        //                coverGrids.Add(new Grid2D(item.Value<int>("x"), item.Value<int>("y")));
        //            }
        //            switch (jObject["Name"].Value<string>())
        //            {
        //                case "AddObjCommand":
        //                    c = new AddObjCommand(terrData, terrainItemMgr, jObject.Value<int>("gridX"), jObject.Value<int>("gridY"),
        //                        coverGrids, param);
        //                    break;
        //                case "DeleteObjCommand":
        //                    c = new DeleteObjCommand(terrData, terrainItemMgr, jObject.Value<int>("gridX"), jObject.Value<int>("gridY"),
        //                        coverGrids, param);
        //                    break;
        //                default:
        //                    throw new Exception();
        //            }
        //            AddCommand(c);
        //        }
        //        Debug.Log("反序列化历史记录条数："+ m_allCommands.Count);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError(e.Message);
        //        throw;
        //    }
        //}
    }

    // 序列化
    public void Serialization()
    {
        //var result = JsonConvert.SerializeObject(m_allCommands);

        //Debug.Log(result);
        //Debug.Log(m_strFilePath);
        //FileHelper.SaveTextToFile(result, m_strFilePath);
        //AssetDatabase.Refresh();
    }
    #endregion
}

//http://blog.skydev.cc/Article/View/25c9a7f7-e5be-4419-b0b4-7f147b96617a

//public class TypeNameSerializationBinder : ISerializationBinder
//{
//    public void BindToName(Type serializedType, out string assemblyName, out string typeName)
//    {
//        assemblyName = null;
//        typeName = serializedType.Name;
//    }

//    public Type BindToType(string assemblyName, string typeName)
//    {
//        //普遍的命名空间和程序集
//        string TypeFormat = "{0}, Assembly-CSharp-Editor";
//        var resolvedTypeName = string.Format(TypeFormat, typeName);
//        return Type.GetType(resolvedTypeName, true);
//    }
//}

//public class BaseSpecifiedConcreteClassConverter : DefaultContractResolver
//{
//    protected override JsonConverter ResolveContractConverter(Type objectType)
//    {
//        if (typeof(Command).IsAssignableFrom(objectType) && !objectType.IsAbstract)
//            return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
//        return base.ResolveContractConverter(objectType);
//    }
//}

//class CommandConverter : JsonConverter
//{
//    //TerrainItemParam param = new TerrainItemParam();
//    //param.name = "test";
//    //    param.x = 1f;
//    //    param.y = 1f;
//    //    param.itemType = 1;
//    //    param.rotationY = 0f;

//    //    var coverGrids = new List<Grid2D>() { new Grid2D(1, 1) };
//    //DeleteObjCommand deleteCmd = new DeleteObjCommand(terrData, terrainItemMgr, 1, 1, param, coverGrids);

//    static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings()
//                            { ContractResolver = new BaseSpecifiedConcreteClassConverter() };

//    public override bool CanConvert(Type objectType)
//    {
//        return (objectType == typeof(Command));
//    }
//    public override bool CanWrite
//    {
//        get { return false; }
//    }

//    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//    {
//        JObject jo = JObject.Load(reader);
//        switch (jo["Name"].Value<string>())
//        {
//            case "AddObjCommand":
//                return JsonConvert.DeserializeObject<AddObjCommand>(jo.ToString(), SpecifiedSubclassConversion);
//            case "DeleteObjCommand":
//                return JsonConvert.DeserializeObject<DeleteObjCommand>(jo.ToString(), SpecifiedSubclassConversion);
//            default:
//                throw new Exception();
//        }
//        throw new NotImplementedException();
//    }

//    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//    {
//        serializer.Serialize(writer, value, typeof(Command));
//        throw new NotImplementedException(); // won't be called because CanWrite returns false
//    }
//}
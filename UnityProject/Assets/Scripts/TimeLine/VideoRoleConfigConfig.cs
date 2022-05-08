using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;

public partial class VideoRoleConfigConfig {

    public readonly string id;
	public readonly Vector3 startPos;
	public readonly Vector3 endPos;
	public readonly float startTime;
	public readonly float duration;
	public readonly string defaultActionSign;
	public readonly string[] actionSigns;
	public readonly float[] actionStartTimes;
	public readonly float[] actionDurings;
	public readonly string[] effectSigns;

    

    public VideoRoleConfigConfig(string input) {
   //      try {
   //          var tables = input.Split('\t');
   //          id = tables[0];
			// startPos=tables[1].Vector3Parse();
			// endPos=tables[2].Vector3Parse();
			// float.TryParse(tables[3],out startTime);
			// float.TryParse(tables[4],out duration);
			// defaultActionSign = tables[5];
			// actionSigns = tables[6].Trim().Split(StringUtil.splitSeparator,StringSplitOptions.RemoveEmptyEntries);
			// string[] actionStartTimesStringArray = tables[7].Trim().Split(StringUtil.splitSeparator,StringSplitOptions.RemoveEmptyEntries);
			// actionStartTimes = new float[actionStartTimesStringArray.Length];
			// for (int i=0;i<actionStartTimesStringArray.Length;i++)
			// {
			// 	 float.TryParse(actionStartTimesStringArray[i],out actionStartTimes[i]);
			// }
			// string[] actionDuringsStringArray = tables[8].Trim().Split(StringUtil.splitSeparator,StringSplitOptions.RemoveEmptyEntries);
			// actionDurings = new float[actionDuringsStringArray.Length];
			// for (int i=0;i<actionDuringsStringArray.Length;i++)
			// {
			// 	 float.TryParse(actionDuringsStringArray[i],out actionDurings[i]);
			// }
			// effectSigns = tables[9].Trim().Split(StringUtil.splitSeparator,StringSplitOptions.RemoveEmptyEntries);
   //      } catch (Exception ex) {
   //          DebugEx.LogError(ex);
   //      }
    }

    static Dictionary<string, VideoRoleConfigConfig> configs = null;
    public static VideoRoleConfigConfig Get(string id) {
        if (!Inited) {
            Init(true);
        }

        if (string.IsNullOrEmpty(id)) {
            return null;
        }

        if (configs.ContainsKey(id)) {
            return configs[id];
        }

        VideoRoleConfigConfig config = null;
        if (rawDatas.ContainsKey(id)) {
            config = configs[id] = new VideoRoleConfigConfig(rawDatas[id]);
            rawDatas.Remove(id);
        }

        if (config == null) {
            Debug.LogFormat("获取配置失败 VideoRoleConfigConfig id:{0}", id);
        }

        return config;
    }

    public static VideoRoleConfigConfig Get(int id) {
        return Get(id.ToString());
    }

    public static bool Has(string id) {
        if (!Inited) {
            Init(true);
        }

        return configs.ContainsKey(id) || rawDatas.ContainsKey(id);
    }

    public static List<string> GetKeys() {
        if (!Inited) {
            Init(true);
        }

        var keys = new List<string>();
        keys.AddRange(configs.Keys);
        keys.AddRange(rawDatas.Keys);
        return keys;
    }

    public static bool Inited { get; set; }
    protected static Dictionary<string, string> rawDatas = null;
    public static void Init(bool sync = false) {
        // Inited = false;
        // var lines = ConfigManager.GetConfigRawDatas("VideoRoleConfig");
        // configs = new Dictionary<string, VideoRoleConfigConfig>();
        //
        // if (sync) {
        //     rawDatas = new Dictionary<string, string>(lines.Length - 3);
        //     for (var i = 3; i < lines.Length; i++) {
        //         var line = lines[i];
        //         var index = line.IndexOf("\t");
        //         var id = line.Substring(0, index);
        //
        //         rawDatas.Add(id, line);
        //     }
        //     Inited = true;
        // } else {
        //     ThreadPool.QueueUserWorkItem((object @object) => {
        //         rawDatas = new Dictionary<string, string>(lines.Length - 3);
        //         for (var i = 3; i < lines.Length; i++) {
        //             var line = lines[i];
        //             var index = line.IndexOf("\t");
        //             var id = line.Substring(0, index);
        //
        //             rawDatas.Add(id, line);
        //         }
        //
        //         Inited = true;
        //     });
        // }
    }

}



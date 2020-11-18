#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    AudioProcess.cs
 Author:      Zeng Zhiwei
 Time:        2020/11/18 20:21:57
=====================================================
*/
#endregion

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AudioPostProcess : AssetPostprocessor
{
    //导入后处理
    void OnPostprocessAudio(AudioClip clip)
    {
        Debug.Log("[AudioPostProcess]音频后处理！" + assetPath);
        foreach(KeyValuePair<string, AudioDetail> item in ImporterAudio.AudioDetailMap)
        {
            if(this.assetPath.Contains(item.Key))
            {
                AudioImporter importer = (AudioImporter)assetImporter;
                AudioDetail audioDetail = item.Value;
                ImporterAudio.DealAudioImporter(clip, importer, audioDetail.forceMono, audioDetail.bSensitive, audioDetail.compressSampleRate);
                break;
            }
        }
    }
}
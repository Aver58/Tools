using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public struct AudioDetail
{
    public AudioDetail(bool forceMono, bool bSensitive = false, bool compressSampleRate = true)
    {
        this.forceMono = forceMono;
        this.bSensitive = bSensitive;
        this.compressSampleRate = compressSampleRate;
    }
    public bool forceMono;
    public bool bSensitive;
    public bool compressSampleRate;
}

public class ImporterAudio
{
    public static Dictionary<string, AudioDetail> AudioDetailMap = new Dictionary<string, AudioDetail> {
        {"Assets/Data/sound/new",new AudioDetail(true,true) },
        {"Assets/Data/sound/voice",new AudioDetail(true,false) },
        {"Assets/Data/sound/music",new AudioDetail(false,false,false) },
        {"Assets/Data/sound/voice/cg",new AudioDetail(false,false) },
    };

    public static void ImportAll()
    {
        foreach(KeyValuePair<string, AudioDetail> item in AudioDetailMap)
        {
            AudioDetail audioDetail = item.Value;
            DealAudioPath(item.Key, audioDetail.forceMono,audioDetail.bSensitive, audioDetail.compressSampleRate);
        }
        Debug.Log("所有音频设置完成");
    }

    // forceMono 是否单声道
    // bSensitive 时延是否敏感
    private static void DealAudioPath(string strPath, bool forceMono, bool bSensitive = false, bool compressSampleRate = true)
    {
        string[] arrFiles = FileHelper.GetAllChildFiles (strPath, ".ogg", SearchOption.AllDirectories);
        int len = arrFiles.Length;
        
		for(int i=0; i<len; i++)
		{
			string assetPath = arrFiles [i];
            EditorUtility.DisplayProgressBar(strPath, assetPath, ((float)i+1)/len);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            AudioImporter importer = AudioImporter.GetAtPath(assetPath) as AudioImporter;

            DealAudioImporter(clip, importer,forceMono, bSensitive, compressSampleRate);
        }
        EditorUtility.ClearProgressBar();
    }

    public static void DealAudioImporter(AudioClip clip, AudioImporter importer, bool forceMono, bool bSensitive = false, bool compressSampleRate = true)
    {
        bool bChange = false;
        if(clip == null)
            return;

        AudioImporterSampleSettings androidSetting = importer.GetOverrideSampleSettings("Android");
        AudioImporterSampleSettings iosSetting = importer.GetOverrideSampleSettings("iPhone");

        if(clip.length >= 10)
        {
            if(androidSetting.loadType != AudioClipLoadType.Streaming)
            {
                androidSetting.loadType = AudioClipLoadType.Streaming;
                iosSetting.loadType = AudioClipLoadType.Streaming;
                bChange = true;
            }

            AudioCompressionFormat newFmt = AudioCompressionFormat.Vorbis;
            if(newFmt != androidSetting.compressionFormat)
            {
                androidSetting.compressionFormat = newFmt;
                iosSetting.compressionFormat = newFmt;
                androidSetting.quality = 0.01f;
                iosSetting.quality = 0.01f;
                bChange = true;
            }

        }
        else if(clip.length < 10 && clip.length >= 1)
        {
            AudioClipLoadType newLoadType = AudioClipLoadType.CompressedInMemory;
            if(newLoadType != androidSetting.loadType)
            {
                androidSetting.loadType = newLoadType;
                iosSetting.loadType = newLoadType;
                bChange = true;
            }
            AudioCompressionFormat newFmt = bSensitive ? AudioCompressionFormat.ADPCM : AudioCompressionFormat.Vorbis;
            if(newFmt != androidSetting.compressionFormat)
            {
                androidSetting.compressionFormat = newFmt;
                iosSetting.compressionFormat = newFmt;
                bChange = true;
                if(newFmt == AudioCompressionFormat.Vorbis)
                {
                    androidSetting.quality = 0.01f;
                    iosSetting.quality = 0.01f;
                }
            }
        }
        else if(clip.length < 1)
        {
            AudioClipLoadType newLoadType = bSensitive ? AudioClipLoadType.DecompressOnLoad : AudioClipLoadType.CompressedInMemory;
            if(androidSetting.loadType != newLoadType)
            {
                androidSetting.loadType = newLoadType;
                iosSetting.loadType = newLoadType;
                bChange = true;
            }
            AudioCompressionFormat newFmt = AudioCompressionFormat.ADPCM;
            if(newFmt != androidSetting.compressionFormat)
            {
                androidSetting.compressionFormat = newFmt;
                iosSetting.compressionFormat = newFmt;
                bChange = true;
            }
        }

        var sampleRate = (uint)(compressSampleRate ? 22050 : 44100);
        if(androidSetting.sampleRateSetting != AudioSampleRateSetting.OverrideSampleRate || androidSetting.sampleRateOverride != sampleRate)
        {
            androidSetting.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
            iosSetting.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
            androidSetting.sampleRateOverride = sampleRate;
            iosSetting.sampleRateOverride = sampleRate;
            bChange = true;
        }

        if(importer.forceToMono != forceMono)
        {
            importer.forceToMono = forceMono;
            bChange = true;
        }
        if(importer.loadInBackground && androidSetting.loadType != AudioClipLoadType.Streaming)
        {
            importer.loadInBackground = false;
            bChange = true;
        }
        if(importer.preloadAudioData)
        {
            importer.preloadAudioData = false;
            bChange = true;
        }

        if(bChange)
        {
            importer.SetOverrideSampleSettings("Android", androidSetting);
            importer.SetOverrideSampleSettings("iPhone", iosSetting);
            importer.SaveAndReimport();
        }
    }
}

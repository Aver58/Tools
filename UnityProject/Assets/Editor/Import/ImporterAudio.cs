using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ImporterAudio
{
    public static void ImportAll()
    {
        DealAudioPath("Assets/Data/sound/new", true, true);
        DealAudioPath("Assets/Data/sound/voice", true, false);
        DealAudioPath("Assets/Data/sound/music", false, false, false);
        DealAudioPath("Assets/Data/sound/voice/cg", false, false);
        Debug.Log("所有音频设置完成");
    }

    public static string[] GetAllChildFiles(string path, string suffix = "", SearchOption option = SearchOption.AllDirectories)
    {
        string strPattner = "*";
        if(suffix.Length > 0 && suffix[0] != '.')
        {
            strPattner += "." + suffix;
        }
        else
        {
            strPattner += suffix;
        }

        string[] files = Directory.GetFiles(path, strPattner, option);

        return files;
    }

    // forceMono 是否单声道
    // bSensitive 时延是否敏感
    private static void DealAudioPath(string strPath, bool forceMono, bool bSensitive = false, bool compressSampleRate = true)
    {
        string[] arrFiles = GetAllChildFiles (strPath, ".ogg", SearchOption.AllDirectories);
        int len = arrFiles.Length;
        
		for(int i=0; i<len; i++)
		{
            bool bChange = false;
			string strFile = arrFiles [i];
            EditorUtility.DisplayProgressBar(strPath, strFile, ((float)i+1)/len);
			AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(strFile);
            AudioImporter importer = AudioImporter.GetAtPath(strFile) as AudioImporter ;

            AudioImporterSampleSettings androidSetting = importer.GetOverrideSampleSettings("Android");
            AudioImporterSampleSettings iosSetting = importer.GetOverrideSampleSettings("iPhone");

            if(clip.length>=10)
            {
                if(androidSetting.loadType!=AudioClipLoadType.Streaming)
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
            else if(clip.length<10 && clip.length>=1)
            {
                AudioClipLoadType newLoadType = AudioClipLoadType.CompressedInMemory;
                if(newLoadType!=androidSetting.loadType)
                {
                    androidSetting.loadType = newLoadType;
                    iosSetting.loadType = newLoadType;
                    bChange = true;
                }
                AudioCompressionFormat newFmt = bSensitive?AudioCompressionFormat.ADPCM:AudioCompressionFormat.Vorbis;
                if(newFmt != androidSetting.compressionFormat)
                {
                    androidSetting.compressionFormat = newFmt;
                    iosSetting.compressionFormat = newFmt;
                    bChange = true;
                    if(newFmt==AudioCompressionFormat.Vorbis)
                    {
                        androidSetting.quality = 0.01f;
                        iosSetting.quality = 0.01f;
                    }
                }
            }
            else if(clip.length<1)
            {
                AudioClipLoadType newLoadType = bSensitive?AudioClipLoadType.DecompressOnLoad:AudioClipLoadType.CompressedInMemory;
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

            if(importer.forceToMono!=forceMono)
            {
                importer.forceToMono = forceMono;
                bChange = true;
            }
            if(importer.loadInBackground && androidSetting.loadType!=AudioClipLoadType.Streaming)
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
                importer.SaveAndReimport ();
            }
		}
        EditorUtility.ClearProgressBar();
    }
	
}

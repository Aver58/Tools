using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorWindowCommon
{
    public class EditorWindowHelper
    {
        private static char[] DirectorySplitChars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        public static List<bool> InitToggleStates(int count)
        {
            List<bool> toggleStates = new List<bool>();
            toggleStates.Add(true);

            for (int i = 1; i < count; i++)
            {
                toggleStates.Add(false);
            }

            return toggleStates;
        }

        public static void ResetToggleStates(List<bool> toggleStates, int ignoreIndex)
        {
            for (int n = 0; n < toggleStates.Count; n++)
            {
                if (n != ignoreIndex) {
                    toggleStates[n] = false;
                }
            }
        }

        // 完整路径转换为Asset路径
        public static string DataPathToAssetPath(string dataPath)
        {
            string path = dataPath.ToLower();

            int index = path.IndexOf("assets\\");
            if (index != -1) {
                return path.Substring(index);
            }
            else
            {
                index = path.IndexOf("assets/");
                if (index != -1) {
                    return path.Substring(index);
                }
            }

            Debug.LogError("路径转换出错！");
            return string.Empty;
        }

        // 创建Unity Asset目录
        // AssetDatabase.CreateFolder API 创建的目录如果父目录没有创建，会出现奇怪的问题，所以这边要循环处理
        public static void TryCreateAssetFolder(string assetPath)
        {
            string target = string.Empty;
            string[] dirArray = assetPath.Split(DirectorySplitChars);
            for (int i = 0; i < dirArray.Length; i++)
            {
                string dir = dirArray[i];
                string parent = target;

                target = Path.Combine(target, dir);
                if (!AssetDatabase.IsValidFolder(target)) {
                    AssetDatabase.CreateFolder(parent, dir);
                }
            }
        }

        // 删除指定目录下空的文件夹
        public static void DeleteEmptyFolders(string targetFolder)
        {
            string[] directories = Directory.GetDirectories(targetFolder);
            for (int i = 0; i < directories.Length; i++) {
                DeleteEmptyFolders(directories[i]);
            }

            string[] tempDirectories = Directory.GetDirectories(targetFolder);
            string[] temFiles = Directory.GetFiles(targetFolder);
            if (tempDirectories.Length == 0 && temFiles.Length == 0)
            {
                if (AssetDatabase.DeleteAsset(targetFolder)) {
                    Debug.LogFormat("成功删除空的文件夹_{0}", targetFolder);
                }
                else {
                    Debug.LogFormat("删除空的文件夹_{0}_失败", targetFolder);
                }
            }
        }

        public static void CopyDirectory(string sourceDirName, string destDirName)
        {
            if (!Directory.Exists(destDirName)) {
                Directory.CreateDirectory(destDirName);
            }

            foreach (string folderPath in Directory.GetDirectories(sourceDirName, "*", SearchOption.AllDirectories))
            {
                if (!Directory.Exists(folderPath.Replace(sourceDirName, destDirName))) {
                    Directory.CreateDirectory(folderPath.Replace(sourceDirName, destDirName));
                }
            }

            foreach (string filePath in Directory.GetFiles(sourceDirName, "*.*", SearchOption.AllDirectories))
            {
                string newFilePath = Path.Combine(Path.GetDirectoryName(filePath).Replace(sourceDirName, destDirName), Path.GetFileName(filePath));
                File.Copy(filePath, newFilePath, true);
            }
        }
    }
}
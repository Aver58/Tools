using System;
using System.Collections;
using System.IO;
using UnityEngine.Networking;

public class HttpDownLoad {
    public float progress { get; private set; }

    public bool isDone { get; private set; }

    private bool isStop;

    public IEnumerator Start(string url, string filePath, Action callBack = null)
    {
        var headRequest = UnityWebRequest.Head(url);

        yield return headRequest.SendWebRequest();
        
        var totalLength = long.Parse(headRequest.GetResponseHeader("Content-Length"));

        var dirPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
        {
            var fileLength = fs.Length;

            if (fileLength < totalLength)
            {
                fs.Seek(fileLength, SeekOrigin.Begin);

                var request = UnityWebRequest.Get(url);
                request.SetRequestHeader("Range", "bytes=" + fileLength + "-" + totalLength);
                request.SendWebRequest();

                var index = 0;
                while (!request.isDone)
                {
                    if (isStop) break;
                    yield return null;
                    var buff = request.downloadHandler.data;
                    if (buff != null)
                    {
                        var length = buff.Length - index;
                        fs.Write(buff, index, length);
                        index += length;
                        fileLength += length;

                        if (fileLength == totalLength)
                        {
                            progress = 1f;
                        }
                        else
                        {
                            progress = fileLength / (float) totalLength;
                        }
                    }
                }
            }
            else
            {
                progress = 1f;
            }

            fs.Close();
            fs.Dispose();
        }

        if (progress >= 1f)
        {
            isDone = true;
            callBack?.Invoke();
        }
    }

    public void Stop()
    {
        isStop = true;
    }
    
    /// <summary>
    /// 检查服务器端对断点续传的支持
    /// </summary>
    private static bool IsAcceptRanges(UnityWebRequest res)
    {
        if (res.GetResponseHeader("Accept-Ranges") == null) 
            return true;
        
        var s = res.GetResponseHeader("Accept-Ranges");
        return s != "none";
    }
    
    // 检查服务器端文件是否变化
    // 对于这个问题，HTTP 响应头为我们提供了不同的选择。ETag 和 Last-Modified 都能完成任务。
    // ETag 就是一个标识当前请求内容的字符串，当请求的资源发生变化后，对应的 ETag 也会变化。
    // Last-Modified 就是所请求的资源在服务器上的最后一次修改时间。使用方法和 ETag 大体相同。
    // 最简单的办法是第一次请求时，把响应头中的 ETag 存下来，下次请求时做比较。
    // 但是你也可以两个都用，做 double check，谁知道web服务器的实现是不是严格遵循了 HTTP 协议！
    private static string GetEtag(UnityWebRequest res)
    {
        return res.GetResponseHeader("ETag");
    }

    private static bool IsEtagChanged(UnityWebRequest res)
    {
        // string newEtag = GetEtag(res);
        // // tempFileName指已经下载到本地的部分文件内容
        // // tempFileInfoName指保存了Etag内容的临时文件
        // if (File.Exists(tempFileName) && File.Exists(tempFileInfoName))
        // {
        //     string oldEtag = File.ReadAllText(tempFileInfoName);
        //     if (!string.IsNullOrEmpty(oldEtag) && !string.IsNullOrEmpty(newEtag) && newEtag == oldEtag)
        //     {
        //         // Etag没有变化，可以断点续传
        //         resumeDowload = true;
        //     }
        // }
        // else
        // {
        //     if (!string.IsNullOrEmpty(newEtag))
        //     {
        //         File.WriteAllText(tempFileInfoName, newEtag);
        //     }
        // }
        return true;
    }
}

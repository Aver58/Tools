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
        
        var totalLength = long.Parse(GetContentLength(headRequest));
        var isAcceptRanges = IsAcceptRanges(headRequest);

        var dirPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write)) {
            
            var fileLength = isAcceptRanges ? fs.Length : 0L;

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

    private static string GetContentLength(UnityWebRequest request) {
        if (null == request) 
            return null;
        
        return request.GetResponseHeader("Content-Length");
    }

    /// <summary>
    /// 检查服务器端对断点续传的支持
    /// </summary>
    private static bool IsAcceptRanges(UnityWebRequest request) {
        var isAcceptRanges = request.GetResponseHeader("Accept-Ranges");
        if (isAcceptRanges == null) 
            return true;
        return isAcceptRanges != "none";
    }
    
    /// 检查服务器端文件是否变化
    /// 当下次要接着下载时，如何确定服务器上的文件还是当初下载了一半的那个文件。如果服务器上的文件已经更新了，那无论如何都需要重新从头开始下载。
    /// 只有在服务器上的文件没有发生变化的情况下，断点续传才有意义。
    /// 对于这个问题，HTTP 响应头为我们提供了不同的选择。ETag 和 Last-Modified 都能完成任务。
    /// ETag 就是一个标识当前请求内容的字符串，当请求的资源发生变化后，对应的 ETag 也会变化。
    /// Last-Modified 就是所请求的资源在服务器上的最后一次修改时间。
    /// 最简单的办法是第一次请求时，把响应头中的 ETag 存下来，下次请求时做比较。
    /// 还有就比较多见的md5，开始下载时，先下载md5文件，比对md5，通过才开始下载。
    
}

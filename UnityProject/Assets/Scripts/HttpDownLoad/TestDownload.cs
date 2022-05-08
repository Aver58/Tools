using UnityEditor;
using UnityEngine;

public class TestDownload : MonoBehaviour {
    private string url = "http://10.30.20.129/sausage_assets_release_android/Video/SeasonVideo.mp4";

    private string fileName = "SeasonVideo.mp4";

    private HttpDownLoad httpDownLoad;
    // Start is called before the first frame update
    void Start()
    {
        httpDownLoad = new HttpDownLoad();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI() {
        if (GUILayout.Button("download",GUILayout.Width(200),GUILayout.Height(50))) {
            StartDownload();
        }

        var progress = httpDownLoad != null ? httpDownLoad.progress : 0f;
        GUILayout.Label(progress.ToString("P"));
    }

    private void StartDownload() {
        var path = EditorUtility.OpenFolderPanel("选择下载文件夹", string.Empty, string.Empty);
        var filePath = $"{path}/{fileName}";
        Debug.Log("下载 路径：" + filePath);
        StartCoroutine(httpDownLoad.Start(url, filePath));
    }
}

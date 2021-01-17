using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Test : MonoBehaviour
{
    private int value;

    private List<string> strs;

    // Start is called before the first frame update
    void Start()
    {

        Application.targetFrameRate = 30;

        strs = new List<string>(10000);
        for (int i = 0; i < 10000; i++)
        {
            strs.Add(i.ToString());
        }
    }

    void Update()
    {
        ReddotMananger.Instance.Update();

        if (Input.GetKeyDown(KeyCode.D))
        {
            //对已存在的节点进行1w次查找操作
            UnityEngine.Profiling.Profiler.BeginSample("1w FindNode");
            for (int i = 0; i < 10000; i++)
            {
                ReddotMananger.Instance.GetTreeNode("First/Second1/Third1");
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            //1w个新节点的创建操作
            UnityEngine.Profiling.Profiler.BeginSample("1w CreateNode");
            for (int i = 0; i < strs.Count; i++)
            {
                ReddotMananger.Instance.GetTreeNode(strs[i]);
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

    }

}

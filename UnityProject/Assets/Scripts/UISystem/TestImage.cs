#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    TestImage.cs
 Author:      Zeng Zhiwei
 Time:        2020/8/6 16:58:06
=====================================================
*/
#endregion

using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TestImage: MonoBehaviour
{
    void Start()
    {
        VertexHelper vp = new VertexHelper();
        vp.Clear();
        //这里采用的添加顶点函数，函数参数分别对应了顶点位置，顶点颜色，纹理坐标
        vp.AddVert(new Vector3(0, 0, 0), Color.red, new Vector2(0, 0));
        vp.AddVert(new Vector3(0, 100, 0), Color.green, new Vector2(0, 100));
        vp.AddVert(new Vector3(100, 100, 0), Color.blue, new Vector2(1, 100));
        vp.AddVert(new Vector3(100, 0, 0), Color.cyan, new Vector2(100, 0));

        //添加三角形
        vp.AddTriangle(0, 1, 2);
        vp.AddTriangle(2, 3, 0);

        MeshFilter meshFilter = this.GetComponent<MeshFilter>();
        if(meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        MeshRenderer render = this.GetComponent<MeshRenderer>();
        if(render == null)
        {
            render = gameObject.AddComponent<MeshRenderer>();
        }

        Mesh mesh = new Mesh();
        vp.FillMesh(mesh);
        meshFilter.mesh = mesh;
        //render.material.color = Color.red;
    }
}
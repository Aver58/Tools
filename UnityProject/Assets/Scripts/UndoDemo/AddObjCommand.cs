#region Copyright © 2018 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    AddObjCmd.cs
 Author:      Zeng Zhiwei
 Time:        2019/11/7 10:26:27
=====================================================
*/
#endregion

using System.Collections.Generic;
using UnityEngine;

public class AddObjCommand : Command
{
    public int gridX { get; set; }
    public int gridY { get; set; }
    public string Name { get; set; }

    public AddObjCommand(int gridX,int gridY)
    {
        this.gridX = gridX;
        this.gridY = gridY;
        Name = this.GetType().Name;
        ModifyTime = System.DateTime.Now.ToString();
    }

    public override void Excute()
    {
    }

    public override void Undo()
    {
    }

    public override string ToString()
    {
        return string.Format("【新增】【{0},{1}】", gridX, gridY);
    }
}


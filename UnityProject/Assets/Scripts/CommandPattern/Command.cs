#region Copyright © 2018 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    Command.cs
 Author:      Zeng Zhiwei
 Time:        2019/11/7 10:03:00
=====================================================
*/
#endregion

public abstract class Command
{
    public string ModifyTime { get; set; }
    #region methods
    public abstract void Excute();
    public abstract void Undo();

    public override string ToString() {
            return "This is a command without info";
    }
    #endregion
}


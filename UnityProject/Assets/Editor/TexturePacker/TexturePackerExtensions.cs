// Decompiled with JetBrains decompiler
// Type: SoarDEditor.Pandora.TexturePackerExtensions
// Assembly: PandoraEditor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 12C36DB3-53EB-40B7-95AC-4F9DF4E8DE46
// Assembly location: C:\Users\onemt\Desktop\PandoraEditor.dll

using System.Collections;
using UnityEngine;

public static class TexturePackerExtensions
{
    public static Rect TPHashtableToRect(this Hashtable table)
    {
        return new Rect((float)table[(object)"x"], (float)table[(object)"y"], (float)table[(object)"w"], (float)table[(object)"h"]);
    }

    public static Vector2 TPHashtableToVector2(this Hashtable table)
    {
        return table.ContainsKey((object)"x") && table.ContainsKey((object)"y") ? new Vector2((float)table[(object)"x"], (float)table[(object)"y"]) : new Vector2((float)table[(object)"w"], (float)table[(object)"h"]);
    }

    public static Vector2 TPVector3toVector2(this Vector3 vec)
    {
        return new Vector2((float)vec.x, (float)vec.y);
    }

    public static bool IsTexturePackerTable(this Hashtable table)
    {
        return table != null && (table.ContainsKey((object)"meta") && ((Hashtable)table[(object)"meta"]).ContainsKey((object)"app"));
    }
}

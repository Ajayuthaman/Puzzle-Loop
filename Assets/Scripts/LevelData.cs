using System;
using System.Collections.Generic;
using UnityEngine;

public enum WireTypeEnum
{
    Empty = 0,
    Power = 1,
    Bulb = 2,
    Straight = 4,
    Corner = 3,
    TJunction = 5,
    Cross = 6
}

public enum WireRotationEnum
{
    Deg0 = 0,
    Deg90 = 1,
    Deg180 = 2,
    Deg270 = 3
}

[Serializable]
public class WireCell
{
    public WireTypeEnum Type;
    public WireRotationEnum Rotation;

    public int GetEncoded()
    {
        return (int)Type + (int)Rotation * 10;
    }
}

[CreateAssetMenu(menuName = "Levels/Power Grid Level")]
public class LevelData : ScriptableObject
{
    public int Rows = 3;
    public int Columns = 3;

    public List<WireCell> Cells = new List<WireCell>();
}

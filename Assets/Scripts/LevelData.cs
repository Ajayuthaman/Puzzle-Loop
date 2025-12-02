using System;
using System.Collections.Generic;
using UnityEngine;

public enum WireType
{
    Empty = 0,
    Power = 1,
    Bulb = 2,
    Straight = 3,
    Corner = 4,
    TJunction = 5,
    Cross = 6
}

public enum WireRotation
{
    Deg0 = 0,
    Deg90 = 1,
    Deg180 = 2,
    Deg270 = 3
}

[Serializable]
public class WireCell
{
    public WireType Type = WireType.Empty;
    public WireRotation Rotation = WireRotation.Deg0;

    public int GetEncodedValue()
    {
        return (int)Type + ((int)Rotation * 10);
    }
}

[CreateAssetMenu(fileName = "Level", menuName = "Levels/New Level Data")]
public class LevelData : ScriptableObject
{
    public int Row = 3;
    public int Column = 3;

    public List<WireCell> Cells = new List<WireCell>();

    private void OnValidate()
    {
        if (Row < 1) Row = 1;
        if (Column < 1) Column = 1;

        int required = Row * Column;

        while (Cells.Count < required) Cells.Add(new WireCell());
        while (Cells.Count > required) Cells.RemoveAt(Cells.Count - 1);
    }
}

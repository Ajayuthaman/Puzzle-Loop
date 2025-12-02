using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wire types mapping (numeric values intentionally match level encoding).
/// </summary>
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

/// <summary>
/// WireCell holds a type and rotation for a single grid cell.
/// GetEncodedValue returns the same encoding used by the Wire.Init method.
/// </summary>
[Serializable]
public class WireCell
{
    public WireType Type = WireType.Empty;
    public WireRotation Rotation = WireRotation.Deg0;

    public int GetEncodedValue()
    {
        // same encoding scheme: type + rotation*10
        return (int)Type + (int)Rotation * 10;
    }
}

/// <summary>
/// LevelData ScriptableObject:
/// - Stores grid dimensions and a flat list of WireCell entries (Row*Column size)
/// - OnValidate ensures the list matches Row*Column
/// </summary>
[CreateAssetMenu(fileName = "Level", menuName = "Levels/New Level Data")]
public class LevelData : ScriptableObject
{
    public int Rows = 3;
    public int Columns = 3;

    public List<WireCell> Cells = new List<WireCell>();

    private void OnValidate()
    {
        if (Rows < 1) Rows = 1;
        if (Columns < 1) Columns = 1;

        if (Cells == null)
            Cells = new List<WireCell>();

        int required = Rows * Columns;
        while (Cells.Count < required)
        {
            Cells.Add(new WireCell());
        }
        while (Cells.Count > required)
        {
            Cells.RemoveAt(Cells.Count - 1);
        }
    }
}

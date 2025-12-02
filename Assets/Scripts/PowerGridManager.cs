using UnityEngine;
using System.Collections.Generic;

public class PowerGridManager : MonoBehaviour
{
    public static PowerGridManager Instance;

    [SerializeField] private LevelData level;
    [SerializeField] private Wire wirePrefab;

    private Wire[,] wires;
    private Camera cam;

    private void Awake()
    {
        Instance = this;
        cam = Camera.main;

        SpawnGrid();
        UpdatePowerFill();
    }

    private void SpawnGrid()
    {
        wires = new Wire[level.Row, level.Column];

        for (int r = 0; r < level.Row; r++)
        {
            for (int c = 0; c < level.Column; c++)
            {
                Vector2 pos = new Vector2(c + 0.5f, r + 0.5f);
                Wire w = Instantiate(wirePrefab, pos, Quaternion.identity);

                int index = r * level.Column + c;
                w.Init(level.Cells[index].GetEncodedValue());

                wires[r, c] = w;
            }
        }

        CenterCamera();
    }

    private void CenterCamera()
    {
        cam.orthographicSize = Mathf.Max(level.Row, level.Column) + 2;
        cam.transform.position = new Vector3(level.Column / 2f, level.Row / 2f, -10);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        int r = Mathf.FloorToInt(mouse.y);
        int c = Mathf.FloorToInt(mouse.x);

        if (r < 0 || c < 0 || r >= level.Row || c >= level.Column) return;

        wires[r, c].Rotate();

        // recalc connections after rotation
        UpdatePowerFill();
    }

    // ---------------------------------------------------------
    // POWER FILL USING BFS
    // ---------------------------------------------------------
    public void UpdatePowerFill()
    {
        // Reset
        foreach (Wire w in wires)
            w.IsPowered = false;

        Queue<Wire> q = new Queue<Wire>();
        HashSet<Wire> visited = new HashSet<Wire>();

        // Add all power sources
        foreach (Wire w in wires)
        {
            if (w.WireType == (int)WireType.Power)
            {
                w.IsPowered = true;
                visited.Add(w);
                q.Enqueue(w);
            }
        }

        // BFS
        while (q.Count > 0)
        {
            Wire current = q.Dequeue();
            current.GetConnectedWires(visited, q);
        }

        // Update visuals
        foreach (Wire w in wires)
            w.UpdateVisual();
    }

}

using UnityEngine;

public class PowerGridManager : MonoBehaviour
{
    public int Rows = 3;
    public int Columns = 3;

    public Wire wirePrefab;
    private Wire[,] wires;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        wires = new Wire[Rows, Columns];

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                Vector2 pos = new Vector2(c + 0.5f, r + 0.5f);
                Wire w = Instantiate(wirePrefab, pos, Quaternion.identity);
                w.Init(0);
                wires[r, c] = w;
            }
        }
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        int r = Mathf.FloorToInt(world.y);
        int c = Mathf.FloorToInt(world.x);

        if (r < 0 || c < 0 || r >= Rows || c >= Columns) return;

        wires[r, c].RotateWire();
    }
}

using UnityEngine;

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
    }
}

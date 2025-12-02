using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// PowerGridManager: manages spawning level, handling player input,
/// running the Color-fill power propagation, and checking win condition.
/// </summary>
public class PowerGridManager : MonoBehaviour
{
    public static PowerGridManager Instance;

    [SerializeField] private LevelData _level; 
    [SerializeField] private Wire _cellPrefab; 
    [SerializeField] private float initialFillDelay = 0.1f; 
    [SerializeField] private float winDelay = 2f;  

    private bool hasGameFinished;
    private Wire[,] wires;
    private List<Wire> powerSources;
    private Camera mainCamera;

    // BFS helpers for color-fill propagation
    private Queue<Wire> checkQueue;
    private HashSet<Wire> visitedWires;

    private void Awake()
    {
        Instance = this;
        hasGameFinished = false;
        mainCamera = Camera.main;

        checkQueue = new Queue<Wire>();
        visitedWires = new HashSet<Wire>();

        SpawnLevel();
    }

    /// <summary>
    /// Spawn grid based on LevelData SO and instantiate each Wire cell.
    /// </summary>
    private void SpawnLevel()
    {
        wires = new Wire[_level.Rows, _level.Columns];
        powerSources = new List<Wire>(_level.Rows * _level.Columns / 4);

        for (int i = 0; i < _level.Rows; i++)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                Vector2 spawnPos = new Vector2(j + 0.5f, i + 0.5f);
                Wire tempWire = Instantiate(_cellPrefab, spawnPos, Quaternion.identity);

                int index = i * _level.Columns + j;
                int encodedValue = _level.Cells[index].GetEncodedValue();
                tempWire.Init(encodedValue);

                wires[i, j] = tempWire;

                // collect power source wires for flood-fill start
                if (tempWire.WireType == 1)
                {
                    powerSources.Add(tempWire);
                }
            }
        }

        SetupCamera();
        StartCoroutine(InitialFillCheck());
    }

    /// <summary>
    /// Adjusting the orthographic camera so the entire grid fits nicely on screen.
    /// </summary>
    private void SetupCamera()
    {
        mainCamera.orthographicSize = Mathf.Max(_level.Rows, _level.Columns) + 2f;
        Vector3 cameraPos = mainCamera.transform.position;
        cameraPos.x = _level.Columns * 0.5f;
        cameraPos.y = _level.Rows * 0.5f;
        mainCamera.transform.position = cameraPos;
    }

    /// <summary>
    /// Handle click input to rotate wires.
    /// Uses world coordinates to find the clicked cell.
    /// </summary>
    private void Update()
    {
        if (hasGameFinished || !Input.GetMouseButtonDown(0)) return;

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        int row = Mathf.FloorToInt(mousePos.y);
        int col = Mathf.FloorToInt(mousePos.x);

        if (row < 0 || col < 0 || row >= _level.Rows || col >= _level.Columns) return;

        wires[row, col].UpdateInput();
    }

    /// <summary>
    /// External call (from Wire) to recalculate power propagation and win state.
    /// </summary>
    public void UpdateFillState()
    {
        CheckFill();
        CheckWin();
    }

    private IEnumerator InitialFillCheck()
    {
        yield return new WaitForSeconds(initialFillDelay);
        UpdateFillState();
    }

    /// <summary>
    /// Recalculate which wires are powered using BFS from all power sources.
    /// </summary>
    private void CheckFill()
    {
        ResetPowerStates();
        FloodFillFromSources();
        UpdateVisuals();
    }

    /// <summary>
    /// Reset powered state for all non-empty cells so flood-fill can recompute.
    /// </summary>
    private void ResetPowerStates()
    {
        for (int i = 0; i < _level.Rows; i++)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                Wire wire = wires[i, j];
                if (wire.WireType != 0)
                {
                    wire.IsPowered = false;
                }
            }
        }
    }

    /// <summary>
    /// BFS starting from all power source wires.
    /// </summary>
    private void FloodFillFromSources()
    {
        checkQueue.Clear();
        visitedWires.Clear();

        foreach (var wire in powerSources)
        {
            checkQueue.Enqueue(wire);
            visitedWires.Add(wire);
        }

        while (checkQueue.Count > 0)
        {
            Wire current = checkQueue.Dequeue();
            current.IsPowered = true;

            current.GetConnectedWires(visitedWires, checkQueue);
        }
    }

    /// <summary>
    /// Tell each wire to update its visuals depending on IsPowered.
    /// </summary>
    private void UpdateVisuals()
    {
        for (int i = 0; i < _level.Rows; i++)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                wires[i, j].UpdateFilled();
            }
        }
    }

    /// <summary>
    /// for Checking if every non-empty cell is powered (win condition).
    /// </summary>
    private void CheckWin()
    {
        for (int i = 0; i < _level.Rows; i++)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                if (!wires[i, j].IsPowered)
                    return;
            }
        }

        hasGameFinished = true;
        StartCoroutine(GameFinished());
    }

    private IEnumerator GameFinished()
    {
        yield return new WaitForSeconds(winDelay);

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}

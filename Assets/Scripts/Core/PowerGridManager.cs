using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PowerGridManager: manages spawning level, handling player input,
/// running the Color-fill power propagation, and checking win condition.
/// </summary>
public class PowerGridManager : MonoBehaviour
{
    [SerializeField] private Wire _cellPrefab;
    [SerializeField] private float initialFillDelay = 0.1f;
    [SerializeField] private float winDelay = 2f;
    [SerializeField] private LevelSceneController sceneController;

    // Current level reference
    private LevelData _currentLevel;

    private bool hasGameFinished;
    private Wire[,] wires;
    private List<Wire> powerSources;
    private Camera mainCamera;

    // BFS helpers for color-fill propagation
    private Queue<Wire> checkQueue;
    private HashSet<Wire> visitedWires;

    // Scene controller reference

    private void OnEnable()
    {
        EventManager.UpdateFillState += UpdateFillState;
    }

    private void OnDisable()
    {
        EventManager.UpdateFillState -= UpdateFillState;
    }

    private void Awake()
    {
        hasGameFinished = false;
        mainCamera = Camera.main;

        checkQueue = new Queue<Wire>();
        visitedWires = new HashSet<Wire>();

        LoadSelectedLevel();
    }

    /// <summary>
    /// Load the level selected from menu
    /// </summary>
    private void LoadSelectedLevel()
    {
        int levelIndex = LevelSaveManager.Instance.GetCurrentLevelIndex();
        LoadLevel(levelIndex);
    }

    /// <summary>
    /// Load a specific level by index
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager not found! Make sure it's in the scene.");
            return;
        }

        LevelData levelData = LevelManager.Instance.GetLevelData(levelIndex);

        if (levelData == null)
        {
            Debug.LogError($"Failed to load level {levelIndex}");
            return;
        }

        _currentLevel = levelData;
        SpawnLevel();

        // Update scene controller
        if (sceneController != null)
        {
            sceneController.SetCurrentLevel(levelIndex);
        }
    }

    /// <summary>
    /// Spawn grid based on current LevelData SO
    /// </summary>
    private void SpawnLevel()
    {
        // Clear existing grid if any
        ClearExistingGrid();

        if (_currentLevel == null)
        {
            Debug.LogError("No level data to spawn!");
            return;
        }

        wires = new Wire[_currentLevel.Rows, _currentLevel.Columns];
        powerSources = new List<Wire>(_currentLevel.Rows * _currentLevel.Columns / 4);

        for (int i = 0; i < _currentLevel.Rows; i++)
        {
            for (int j = 0; j < _currentLevel.Columns; j++)
            {
                Vector2 spawnPos = new Vector2(j + 0.5f, i + 0.5f);
                Wire tempWire = Instantiate(_cellPrefab, spawnPos, Quaternion.identity);

                int index = i * _currentLevel.Columns + j;
                int encodedValue = _currentLevel.Cells[index].GetEncodedValue();
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
    /// Clear existing grid before spawning new one
    /// </summary>
    private void ClearExistingGrid()
    {
        if (wires != null)
        {
            for (int i = 0; i < wires.GetLength(0); i++)
            {
                for (int j = 0; j < wires.GetLength(1); j++)
                {
                    if (wires[i, j] != null)
                    {
                        Destroy(wires[i, j].gameObject);
                    }
                }
            }
        }

        if (powerSources != null)
        {
            powerSources.Clear();
        }

        hasGameFinished = false;
    }

    /// <summary>
    /// Adjusting the orthographic camera so the entire grid fits nicely on screen.
    /// </summary>
    private void SetupCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (_currentLevel != null)
        {
            mainCamera.orthographicSize = Mathf.Max(_currentLevel.Rows, _currentLevel.Columns) + 2f;
            Vector3 cameraPos = mainCamera.transform.position;
            cameraPos.x = _currentLevel.Columns * 0.5f;
            cameraPos.y = _currentLevel.Rows * 0.5f;
            mainCamera.transform.position = cameraPos;
        }
    }

    /// <summary>
    /// Handle click input to rotate wires.
    /// Uses world coordinates to find the clicked cell.
    /// </summary>
    private void Update()
    {
        if (hasGameFinished || !Input.GetMouseButtonDown(0)) return;

        if (_currentLevel == null) return;

        AudioManager.Instance.PlayClick();
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        int row = Mathf.FloorToInt(mousePos.y);
        int col = Mathf.FloorToInt(mousePos.x);

        if (row < 0 || col < 0 || row >= _currentLevel.Rows || col >= _currentLevel.Columns) return;

        if (wires[row, col] != null)
        {
            wires[row, col].UpdateInput();
        }
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
        if (_currentLevel == null) return;

        for (int i = 0; i < _currentLevel.Rows; i++)
        {
            for (int j = 0; j < _currentLevel.Columns; j++)
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
        if (_currentLevel == null) return;

        for (int i = 0; i < _currentLevel.Rows; i++)
        {
            for (int j = 0; j < _currentLevel.Columns; j++)
            {
                wires[i, j].UpdateFilled();
            }
        }
    }

    /// <summary>
    /// Check win condition
    /// </summary>
    private void CheckWin()
    {
        if (_currentLevel == null) return;

        for (int i = 0; i < _currentLevel.Rows; i++)
        {
            for (int j = 0; j < _currentLevel.Columns; j++)
            {
                if (!wires[i, j].IsPowered)
                    return;
            }
        }

        HandleLevelCompletion();
    }

    /// <summary>
    /// Handle level completion with save system
    /// </summary>
    private void HandleLevelCompletion()
    {
        hasGameFinished = true;

        // Get current level index from LevelSaveManager
        int levelIndex = LevelSaveManager.Instance.GetCurrentLevelIndex();

        int score = CalculateScore();

        Debug.Log($"Level {levelIndex} completed with score: {score}");

        // Save progress using LevelSaveManager
        if (LevelSaveManager.Instance != null)
        {
            LevelSaveManager.Instance.CompleteLevel(levelIndex, score);
        }
        AudioManager.Instance.PlayWin();
        StartCoroutine(GameFinished());
    }

    private int CalculateScore()
    {
        return 100;
    }

    private IEnumerator GameFinished()
    {
        yield return new WaitForSeconds(winDelay);

        // Show win panel
        if (sceneController != null)
        {
            sceneController.ShowWinPanel();
        }
    }
}
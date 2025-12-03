using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all LevelData ScriptableObjects and provides level loading functionality
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Data Files")]
    [SerializeField] private List<LevelData> allLevels;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (allLevels.Count == 0)
        {
            Debug.LogWarning("LevelManager has no levels found");
        }
    }

    /// <summary>
    /// Get all available levels
    /// </summary>
    public int GetLevelCount()
    {
        return allLevels.Count;
    }

    /// <summary>
    /// Get specific level data
    /// </summary>
    public LevelData GetLevelData(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= allLevels.Count)
        {
            Debug.LogError($"Level index {levelIndex} out of range! Total levels: {allLevels.Count}");
            // Return first level as fallback
            return allLevels.Count > 0 ? allLevels[0] : null;
        }

        return allLevels[levelIndex];
    }

    /// <summary>
    /// Get all level data for UI display
    /// </summary>
    public List<LevelData> GetAllLevels()
    {
        return allLevels;
    }

    /// <summary>
    /// Debug method to list all levels
    /// </summary>
    public void DebugListLevels()
    {
        Debug.Log($"=== LevelManager Debug ===");
        Debug.Log($"Total levels: {allLevels.Count}");
        for (int i = 0; i < allLevels.Count; i++)
        {
            if (allLevels[i] != null)
            {
                Debug.Log($"Level {i}: {allLevels[i].name} ({allLevels[i].Rows}x{allLevels[i].Columns})");
            }
            else
            {
                Debug.LogError($"Level {i} is NULL!");
            }
        }
    }
}
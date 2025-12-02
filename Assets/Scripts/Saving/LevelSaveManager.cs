using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// Manages saving and loading level progress
/// </summary>
public class LevelSaveManager : MonoBehaviour
{
    public static LevelSaveManager Instance { get; private set; }

    [System.Serializable]
    public class SaveData
    {
        public int highestLevelUnlocked = 1;
        public List<bool> levelCompletion = new List<bool>();
        public List<int> levelScores = new List<int>();

        public SaveData()
        {
            // Initialize with default values
            highestLevelUnlocked = 1;
            levelCompletion = new List<bool>();
            levelScores = new List<int>();
        }
    }

    private SaveData currentSaveData;
    private string savePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "LevelProgress.json");
        LoadGameData();
    }

    /// <summary>
    /// Load saved data or create new if none exists
    /// </summary>
    private void LoadGameData()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                currentSaveData = JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load save data: " + e.Message);
                currentSaveData = new SaveData();
            }
        }
        else
        {
            currentSaveData = new SaveData();
            SaveGameData();
        }
    }

    /// <summary>
    /// Save current progress to file
    /// </summary>
    private void SaveGameData()
    {
        try
        {
            string json = JsonUtility.ToJson(currentSaveData, true);
            File.WriteAllText(savePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save game data: " + e.Message);
        }
    }

    /// <summary>
    /// Get highest unlocked level
    /// </summary>
    public int GetHighestUnlockedLevel()
    {
        return currentSaveData.highestLevelUnlocked;
    }

    /// <summary>
    /// Check if a specific level is unlocked
    /// </summary>
    public bool IsLevelUnlocked(int levelIndex)
    {
        // Level 1 is always unlocked
        if (levelIndex == 0) return true;

        return levelIndex < currentSaveData.highestLevelUnlocked;
    }

    /// <summary>
    /// Check if a level is completed
    /// </summary>
    public bool IsLevelCompleted(int levelIndex)
    {
        if (levelIndex >= currentSaveData.levelCompletion.Count)
            return false;

        return currentSaveData.levelCompletion[levelIndex];
    }

    /// <summary>
    /// Get score for a specific level
    /// </summary>
    public int GetLevelScore(int levelIndex)
    {
        if (levelIndex >= currentSaveData.levelScores.Count)
            return 0;

        return currentSaveData.levelScores[levelIndex];
    }

    /// <summary>
    /// Complete a level and unlock next one
    /// </summary>
    public void CompleteLevel(int levelIndex, int score = 0)
    {
        // Ensure lists are large enough
        while (currentSaveData.levelCompletion.Count <= levelIndex)
        {
            currentSaveData.levelCompletion.Add(false);
        }

        while (currentSaveData.levelScores.Count <= levelIndex)
        {
            currentSaveData.levelScores.Add(0);
        }

        // Mark level as completed and update score if higher
        currentSaveData.levelCompletion[levelIndex] = true;
        if (score > currentSaveData.levelScores[levelIndex])
        {
            currentSaveData.levelScores[levelIndex] = score;
        }

        // Unlock next level
        if (levelIndex + 1 >= currentSaveData.highestLevelUnlocked)
        {
            currentSaveData.highestLevelUnlocked = levelIndex + 2;
        }

        SaveGameData();
    }

    /// <summary>
    /// for testing
    /// </summary>
    public void ResetProgress()
    {
        currentSaveData = new SaveData();
        SaveGameData();
    }
}
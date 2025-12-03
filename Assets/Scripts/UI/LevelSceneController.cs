using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controls level scene UI and navigation
/// </summary>
public class LevelSceneController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button levelSelectButton;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private GameObject winPanel;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    private int currentLevelIndex;

    private void Start()
    {
        // Get current level directly from LevelSaveManager 
        // Store the current level index in LevelSaveManager itself
        currentLevelIndex = LevelSaveManager.Instance.GetCurrentLevelIndex();

        // Update UI
        UpdateLevelText();

        // Setup button listeners
        backButton.onClick.AddListener(ReturnToLevelSelect);
        nextLevelButton.onClick.AddListener(LoadNextLevel);
        retryButton.onClick.AddListener(ReloadLevel);
        levelSelectButton.onClick.AddListener(ReturnToLevelSelect);

        // Hide win panel initially
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Set current level (called by PowerGridManager)
    /// </summary>
    public void SetCurrentLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        UpdateLevelText();
    }

    /// <summary>
    /// Update level text display
    /// </summary>
    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Level " + (currentLevelIndex + 1);
        }
    }

    /// <summary>
    /// Return to level selection menu
    /// </summary>
    public void ReturnToLevelSelect()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    /// <summary>
    /// Show win panel when level is completed
    /// </summary>
    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);

            // Enable/disable next level button based on available levels and unlock status
            if (LevelManager.Instance != null)
            {
                bool hasNextLevel = currentLevelIndex + 1 < LevelManager.Instance.GetLevelCount();
                bool nextLevelUnlocked = LevelSaveManager.Instance.IsLevelUnlocked(currentLevelIndex + 1);
                nextLevelButton.interactable = hasNextLevel && nextLevelUnlocked;
            }
            else
            {
                nextLevelButton.interactable = false;
            }
        }
    }

    /// <summary>
    /// Load next level
    /// </summary>
    public void LoadNextLevel()
    {
        int nextLevel = currentLevelIndex + 1;

        // Check if next level exists and is unlocked
        if (LevelManager.Instance != null &&
            nextLevel < LevelManager.Instance.GetLevelCount() &&
            LevelSaveManager.Instance.IsLevelUnlocked(nextLevel))
        {
            // Set the current level in LevelSaveManager
            LevelSaveManager.Instance.SetCurrentLevel(nextLevel);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    /// <summary>
    /// Reload current level
    /// </summary>
    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
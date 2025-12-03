using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelsMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform levelsContainer;
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button backButton;
    [SerializeField] private MainMenuController mainMenuController;

    private List<LevelButtonItem> levelButtons = new List<LevelButtonItem>();

    private void Start()
    {
        backButton.onClick.AddListener(OnBackClicked);

        CreateLevelButtons();
        RefreshLevelsDisplay();
    }

    /// <summary>
    /// Create buttons based on actual level count
    /// </summary>
    private void CreateLevelButtons()
    {
        // Clear existing
        foreach (Transform c in levelsContainer)
            Destroy(c.gameObject);

        levelButtons.Clear();

        // Get total levels from LevelManager
        int totalLevels = GetTotalLevelCount();

        for (int i = 0; i < totalLevels; i++)
        {
            // Reversed index for bottom-to-top display
            int reversedIndex = totalLevels - 1 - i;

            GameObject obj = Instantiate(levelButtonPrefab, levelsContainer);
            obj.name = "LevelButton_" + (reversedIndex + 1);

            LevelButtonItem item = obj.GetComponent<LevelButtonItem>();

            // Initialize button with save data
            bool unlocked = LevelSaveManager.Instance.IsLevelUnlocked(reversedIndex);
            bool completed = LevelSaveManager.Instance.IsLevelCompleted(reversedIndex);

            item.Initialize(reversedIndex, unlocked, completed);
            item.OnLevelClicked += OnLevelClicked;

            levelButtons.Add(item);
        }

    }

    /// <summary>
    /// Get total number of levels
    /// </summary>
    private int GetTotalLevelCount()
    {
        if (LevelManager.Instance != null)
        {
            return LevelManager.Instance.GetLevelCount();
        }

        Debug.LogWarning("LevelManager not found, using default level count");
        return 10; 
    }

    /// <summary>
    /// Update states of each button
    /// </summary>
    public void RefreshLevelsDisplay()
    {
        foreach (var button in levelButtons)
        {
            int index = button.LevelIndex;
            bool unlocked = LevelSaveManager.Instance.IsLevelUnlocked(index);
            bool completed = LevelSaveManager.Instance.IsLevelCompleted(index);

            button.UpdateState(unlocked, completed);
        }

        StartCoroutine(ScrollToCurrentLevel());
    }

    private void OnLevelClicked(int levelIndex)
    {

        if (LevelSaveManager.Instance == null)
        {
            Debug.LogError("LevelSaveManager is NULL!");
            return;
        }

        bool isUnlocked = LevelSaveManager.Instance.IsLevelUnlocked(levelIndex);

        if (isUnlocked)
        {
            LevelSaveManager.Instance.SetCurrentLevel(levelIndex);
            mainMenuController.LoadLevel(levelIndex);
        }
        else
        {
            Debug.Log($"Level {levelIndex} is locked!");
        }
    }

    private void OnBackClicked()
    {
        if (mainMenuController != null)
        {
            mainMenuController.ReturnToMainMenu();
        }
    }

    /// <summary>
    /// Scroll to highest unlocked level
    /// </summary>
    private System.Collections.IEnumerator ScrollToCurrentLevel()
    {
        yield return new WaitForEndOfFrame();

        int totalLevels = GetTotalLevelCount();
        int highest = LevelSaveManager.Instance.GetHighestUnlockedLevel() - 1;
        highest = Mathf.Clamp(highest, 0, totalLevels - 1);

        // Convert to reversed index for bottom-to-top display
        int reversedIndex = totalLevels - 1 - highest;

        float normalized = 1f - ((float)reversedIndex / (totalLevels - 1));
        scrollRect.verticalNormalizedPosition = normalized;
    }
}
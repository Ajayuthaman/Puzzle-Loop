using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Controls the levels selection screen with chain layout
/// </summary>
public class LevelsMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform levelsContainer;
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button backButton;
    [SerializeField] private MainMenuController mainMenuController;

    [Header("Level Settings")]
    [SerializeField] private int totalLevels = 20;
    [SerializeField] private float verticalSpacing = 120f;
    [SerializeField] private float chainLinkOffset = 40f;

    [Header("Visuals")]
    [SerializeField] private Sprite lockedLevelSprite;
    [SerializeField] private Sprite unlockedLevelSprite;
    [SerializeField] private Sprite completedLevelSprite;
    [SerializeField] private Color lockedTextColor = Color.gray;
    [SerializeField] private Color unlockedTextColor = Color.white;
    [SerializeField] private Color completedTextColor = Color.yellow;

    private List<LevelButton> levelButtons = new List<LevelButton>();

    [System.Serializable]
    public class LevelButton
    {
        public GameObject gameObject;
        public Button button;
        public TMP_Text levelText;
        public Image background;
        public Image chainLinkTop;
        public Image chainLinkBottom;
        public int levelIndex;
    }

    private void Start()
    {
        backButton.onClick.AddListener(OnBackClicked);

        CreateLevelButtons();
        RefreshLevelsDisplay();

        // Auto scroll to highest unlocked level
        StartCoroutine(ScrollToCurrentLevel());
    }

    /// <summary>
    /// Create all level buttons in a vertical chain layout
    /// </summary>
    private void CreateLevelButtons()
    {
        // Clear existing buttons
        foreach (Transform child in levelsContainer)
        {
            Destroy(child.gameObject);
        }
        levelButtons.Clear();

        for (int i = 0; i < totalLevels; i++)
        {
            // Create level button
            GameObject buttonObj = Instantiate(levelButtonPrefab, levelsContainer);
            buttonObj.name = "LevelButton_" + (i + 1);

            // Position vertically (from bottom to top)
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, i * verticalSpacing);

            // Get references
            Button button = buttonObj.GetComponent<Button>();
            TMP_Text levelText = buttonObj.transform.Find("LevelText")?.GetComponent<TMP_Text>();
            Image background = buttonObj.GetComponent<Image>();
            Image chainLinkTop = buttonObj.transform.Find("ChainLinkTop")?.GetComponent<Image>();

            // Create LevelButton data
            LevelButton levelButton = new LevelButton
            {
                gameObject = buttonObj,
                button = button,
                levelText = levelText,
                background = background,
                chainLinkTop = chainLinkTop,
                levelIndex = i
            };

            // Set up button click
            int levelIndex = i; // Capture for closure
            button.onClick.AddListener(() => OnLevelClicked(levelIndex));

            // Set level number text
            if (levelText != null)
            {
                levelText.text = (i + 1).ToString();
            }

            levelButtons.Add(levelButton);
        }

        // Set container height based on number of levels
        RectTransform containerRect = levelsContainer.GetComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, totalLevels * verticalSpacing);
    }

    /// <summary>
    /// Refresh level states based on save data
    /// </summary>
    public void RefreshLevelsDisplay()
    {
        if (LevelSaveManager.Instance == null) return;

        for (int i = 0; i < levelButtons.Count; i++)
        {
            LevelButton levelButton = levelButtons[i];
            bool isUnlocked = LevelSaveManager.Instance.IsLevelUnlocked(i);
            bool isCompleted = LevelSaveManager.Instance.IsLevelCompleted(i);

            // Update button interactivity
            levelButton.button.interactable = isUnlocked;

            // Update visuals
            if (levelButton.background != null)
            {
                if (!isUnlocked)
                {
                    levelButton.background.sprite = lockedLevelSprite;
                    levelButton.background.color = Color.gray;
                }
                else if (isCompleted)
                {
                    levelButton.background.sprite = completedLevelSprite;
                    levelButton.background.color = Color.white;
                }
                else
                {
                    levelButton.background.sprite = unlockedLevelSprite;
                    levelButton.background.color = Color.white;
                }
            }

            // Update text color
            if (levelButton.levelText != null)
            {
                if (!isUnlocked)
                {
                    levelButton.levelText.color = lockedTextColor;
                }
                else if (isCompleted)
                {
                    levelButton.levelText.color = completedTextColor;
                }
                else
                {
                    levelButton.levelText.color = unlockedTextColor;
                }
            }

            // Update chain links visibility
            if (levelButton.chainLinkTop != null)
            {
                levelButton.chainLinkTop.gameObject.SetActive(i < levelButtons.Count - 1);
            }

            if (levelButton.chainLinkBottom != null)
            {
                levelButton.chainLinkBottom.gameObject.SetActive(i > 0);
            }
        }
    }

    /// <summary>
    /// Called when a level button is clicked
    /// </summary>
    private void OnLevelClicked(int levelIndex)
    {
        if (LevelSaveManager.Instance != null && LevelSaveManager.Instance.IsLevelUnlocked(levelIndex))
        {
            Debug.Log("Loading level " + (levelIndex + 1));
            mainMenuController.LoadLevel(levelIndex);
        }
    }

    /// <summary>
    /// Called when Back button is clicked
    /// </summary>
    private void OnBackClicked()
    {
        mainMenuController.ReturnToMainMenu();
    }

    /// <summary>
    /// Auto-scroll to the highest unlocked level
    /// </summary>
    private System.Collections.IEnumerator ScrollToCurrentLevel()
    {
        yield return new WaitForEndOfFrame();

        if (LevelSaveManager.Instance != null && levelButtons.Count > 0)
        {
            int highestUnlocked = LevelSaveManager.Instance.GetHighestUnlockedLevel() - 1;
            highestUnlocked = Mathf.Clamp(highestUnlocked, 0, levelButtons.Count - 1);

            float normalizedPosition = 1f - ((float)highestUnlocked / (levelButtons.Count - 1));
            normalizedPosition = Mathf.Clamp01(normalizedPosition);

            scrollRect.verticalNormalizedPosition = normalizedPosition;
        }
    }
}
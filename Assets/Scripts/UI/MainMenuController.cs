using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls main menu UI interactions
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelsMenuPanel;
    [SerializeField] private LevelsMenuController levelsController;

    [Header("Scene References")]
    [SerializeField] private string gameSceneName = "GameScene";

    private void Start()
    {
        // Initialize button listeners
        playButton.onClick.AddListener(OnPlayClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        // Show main menu, hide levels menu
        mainMenuPanel.SetActive(true);
        levelsMenuPanel.SetActive(false);
    }

    /// <summary>
    /// Called when Play button is clicked
    /// </summary>
    private void OnPlayClicked()
    {
        mainMenuPanel.SetActive(false);
        levelsMenuPanel.SetActive(true);

        if (levelsController != null)
        {
            levelsController.RefreshLevelsDisplay();
        }
    }

    /// <summary>
    /// Called when Quit button is clicked
    /// </summary>
    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    /// <summary>
    /// Called from LevelsMenu to return to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        mainMenuPanel.SetActive(true);
        levelsMenuPanel.SetActive(false);
    }

    /// <summary>
    /// Load the game scene with a specific level
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
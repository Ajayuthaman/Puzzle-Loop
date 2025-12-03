using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Self-contained level button that stores all its data and handles its own visuals
/// </summary>
public class LevelButtonItem : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image background;
    [SerializeField] private Image chainLinkTop;
    [SerializeField] private Button button;

    [Header("Current Level Indicator")]
    [SerializeField] private GameObject currentLevelIndicator;
    [SerializeField] private ParticleSystem unlockParticles;

    [Header("Visual Settings")]
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite completedSprite;
    [SerializeField] private Color lockedTextColor = Color.gray;
    [SerializeField] private Color unlockedTextColor = Color.white;
    [SerializeField] private Color completedTextColor = Color.yellow;

    // Level data
    private int levelIndex;
    private bool isUnlocked;
    private bool isCompleted;

    // Events
    public System.Action<int> OnLevelClicked;

    /// <summary>
    /// Initialize the level button with data
    /// </summary>
    public void Initialize(int index, bool unlocked, bool completed)
    {
        levelIndex = index;
        isUnlocked = unlocked;
        isCompleted = completed;

        UpdateVisuals();
    }

    /// <summary>
    /// Update all visuals based on current state
    /// </summary>
    public void UpdateVisuals()
    {
        // Update level number text
        if (levelText != null)
        {
            levelText.text = (levelIndex + 1).ToString();

            // Update text color based on state
            if (!isUnlocked)
                levelText.color = lockedTextColor;
            else if (isCompleted)
                levelText.color = completedTextColor;
            else
                levelText.color = unlockedTextColor;
        }

        // Update background sprite
        if (background != null)
        {
            if (!isUnlocked)
            {
                background.sprite = lockedSprite;
                background.color = Color.gray;
            }
            else if (isCompleted)
            {
                background.sprite = completedSprite;
                background.color = Color.white;
            }
            else
            {
                background.sprite = unlockedSprite;
                background.color = Color.white;
            }
        }

        // Update button interactivity
        if (button != null)
        {
            button.interactable = isUnlocked;
        }
    }

    /// <summary>
    /// Set chain link visibility
    /// </summary>
    public void SetChainLinks(bool showTop, bool showBottom)
    {
        if (chainLinkTop != null)
            chainLinkTop.gameObject.SetActive(showTop);
    }

    /// <summary>
    /// Update the button state
    /// </summary>
    public void UpdateState(bool unlocked, bool completed)
    {
        isUnlocked = unlocked;
        isCompleted = completed;
        UpdateVisuals();
    }

    /// <summary>
    /// Handle click via IPointerClickHandler for better control
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isUnlocked && OnLevelClicked != null)
        {
            OnLevelClicked(levelIndex);
        }
    }

    /// <summary>
    /// Public getters for the button data
    /// </summary>
    public int LevelIndex => levelIndex;
    public bool IsUnlocked => isUnlocked;
    public bool IsCompleted => isCompleted;

    /// <summary>
    /// Set position for bottom-to-top layout
    /// </summary>
    /// <summary>
    /// Set position for bottom-to-top layout
    /// </summary>
    public void SetPosition(float yPosition)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        // Ensure proper anchor for bottom-to-top
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);

        rectTransform.anchoredPosition = new Vector2(0, yPosition);
    }

    /// <summary>
    /// Direct method to trigger level load (for button onClick event)
    /// </summary>
    public void OnButtonClicked()
    {
        if (isUnlocked && OnLevelClicked != null)
        {
            OnLevelClicked(levelIndex);
        }
    }

    /// <summary>
    /// Highlight as current level
    /// </summary>
    public void SetAsCurrentLevel(bool isCurrent)
    {
        if (currentLevelIndicator != null)
        {
            currentLevelIndicator.SetActive(isCurrent);

            if (isCurrent && unlockParticles != null)
            {
                unlockParticles.Play();
            }
        }
    }

    /// <summary>
    /// Play unlock animation
    /// </summary>
    public void PlayUnlockAnimation()
    {
        // Add animation here (scale pulse, color change, etc.)
        StartCoroutine(UnlockAnimationRoutine());
    }

    private System.Collections.IEnumerator UnlockAnimationRoutine()
    {
        Vector3 originalScale = transform.localScale;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Pulse animation
            float scale = Mathf.Lerp(1f, 1.2f, Mathf.Sin(t * Mathf.PI));
            transform.localScale = originalScale * scale;

            yield return null;
        }

        transform.localScale = originalScale;
    }
}
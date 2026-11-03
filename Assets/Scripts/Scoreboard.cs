using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages player HUD: Score display, Player Lives/Health display,
/// Game Over overlay, and Victory/Mission Complete screen.
/// Directly updates Canvas TextMeshPro elements.
/// </summary>
public class Scoreboard : MonoBehaviour
{
    public static Scoreboard Instance { get; private set; }

    [Header("UI Text References")]
    [SerializeField] private TMP_Text scoreboardText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text victoryScoreText;

    private int score = 0;
    private int lives = 3;
    private int maxLives = 3;
    private bool isGameOver = false;
    private bool isVictory = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LocateCanvasElements();
    }

    void Start()
    {
        UpdateScoreDisplay();
        UpdateLivesDisplay();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    public void IncreaseScore(int amount)
    {
        score += amount;
        UpdateScoreDisplay();
    }

    public int GetScore() => score;

    public void SetLives(int currentLives)
    {
        lives = Mathf.Clamp(currentLives, 0, maxLives);
        UpdateLivesDisplay();
    }

    public void DecreaseLife()
    {
        lives = Mathf.Max(0, lives - 1);
        UpdateLivesDisplay();
    }

    public int GetLives() => lives;

    private void UpdateScoreDisplay()
    {
        if (scoreboardText != null)
        {
            scoreboardText.text = $"SCORE: {score:D4}";
        }
    }

    private void UpdateLivesDisplay()
    {
        if (livesText != null)
        {
            string hearts = "";
            for (int i = 0; i < lives; i++) hearts += "★ ";
            livesText.text = $"LIVES: {lives} {hearts.TrimEnd()}";
        }
    }

    public void ShowGameOver()
    {
        isGameOver = true;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (gameOverText != null)
        {
            gameOverText.text = $"GAME OVER\n<size=50%>Final Score: {score}</size>";
        }
    }

    public void ShowVictory()
    {
        isVictory = true;
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        if (victoryScoreText != null)
        {
            victoryScoreText.text = $"MISSION ACCOMPLISHED!\n<size=50%>Final Score: {score}</size>";
        }
    }

    /// <summary>
    /// Renders overlays (Game Over banner and Victory banner) or fallback HUD
    /// if Canvas elements are absent. Never duplicates visible Canvas HUD.
    /// </summary>
    void OnGUI()
    {
        // Only draw top HUD if Canvas text was not found
        if (scoreboardText == null || livesText == null)
        {
            GUIStyle scoreBoxStyle = new GUIStyle(GUI.skin.box);
            scoreBoxStyle.fontSize = 20;
            scoreBoxStyle.fontStyle = FontStyle.Bold;
            scoreBoxStyle.normal.textColor = new Color(0.2f, 1f, 0.45f, 1f);
            scoreBoxStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Box(new Rect(20, 20, 210, 44), $"SCORE: {score:D4}", scoreBoxStyle);

            GUIStyle livesBoxStyle = new GUIStyle(GUI.skin.box);
            livesBoxStyle.fontSize = 20;
            livesBoxStyle.fontStyle = FontStyle.Bold;
            livesBoxStyle.normal.textColor = new Color(1f, 0.35f, 0.35f, 1f);
            livesBoxStyle.alignment = TextAnchor.MiddleCenter;
            string stars = "";
            for (int i = 0; i < lives; i++) stars += "★ ";
            GUI.Box(new Rect(Screen.width - 230, 20, 210, 44), $"LIVES: {lives}  {stars.TrimEnd()}", livesBoxStyle);
        }

        // Center Screen: GAME OVER modal
        if (isGameOver && gameOverPanel == null)
        {
            GUIStyle goStyle = new GUIStyle(GUI.skin.box);
            goStyle.fontSize = 28;
            goStyle.fontStyle = FontStyle.Bold;
            goStyle.normal.textColor = new Color(1f, 0.25f, 0.25f, 1f);
            goStyle.alignment = TextAnchor.MiddleCenter;

            float boxW = 460;
            float boxH = 170;
            GUI.Box(new Rect((Screen.width - boxW) * 0.5f, (Screen.height - boxH) * 0.5f, boxW, boxH),
                $"GAME OVER\n\nFinal Score: {score}\n\nRestarting level...", goStyle);
        }

        // Center Screen: VICTORY modal
        if (isVictory && victoryPanel == null)
        {
            GUIStyle vicStyle = new GUIStyle(GUI.skin.box);
            vicStyle.fontSize = 28;
            vicStyle.fontStyle = FontStyle.Bold;
            vicStyle.normal.textColor = new Color(0.3f, 1f, 0.5f, 1f);
            vicStyle.alignment = TextAnchor.MiddleCenter;

            float boxW = 500;
            float boxH = 170;
            GUI.Box(new Rect((Screen.width - boxW) * 0.5f, (Screen.height - boxH) * 0.5f, boxW, boxH),
                $"MISSION ACCOMPLISHED!\n\nFinal Score: {score}\n\nCongratulations!", vicStyle);
        }
    }

    private void LocateCanvasElements()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        if (scoreboardText == null)
        {
            Transform existing = canvas.transform.Find("ScoreText");
            if (existing != null) scoreboardText = existing.GetComponent<TMP_Text>();
        }

        if (livesText == null)
        {
            Transform existing = canvas.transform.Find("LivesText");
            if (existing != null) livesText = existing.GetComponent<TMP_Text>();
        }
    }
}

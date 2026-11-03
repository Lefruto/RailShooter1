using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls level transitions, game over sequences, and scene restarts
/// using robust Coroutines as required by homework specifications.
/// </summary>
public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    [SerializeField] private float restartDelay = 2.5f;

    private bool isRestarting = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ReloadLevel(float delay = 1.5f)
    {
        if (isRestarting) return;
        StartCoroutine(ReloadLevelRoutine(delay));
    }

    public void TriggerGameOver()
    {
        if (isRestarting) return;
        StartCoroutine(GameOverRoutine());
    }

    public void TriggerVictory()
    {
        if (isRestarting) return;
        StartCoroutine(VictoryRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        isRestarting = true;

        if (Scoreboard.Instance != null)
        {
            Scoreboard.Instance.ShowGameOver();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOver(0.8f);
        }

        yield return new WaitForSeconds(restartDelay);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private IEnumerator VictoryRoutine()
    {
        isRestarting = true;

        if (Scoreboard.Instance != null)
        {
            Scoreboard.Instance.ShowVictory();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWin(0.8f);
        }

        yield return new WaitForSeconds(5.0f);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private IEnumerator ReloadLevelRoutine(float delay)
    {
        isRestarting = true;
        yield return new WaitForSeconds(delay);
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}

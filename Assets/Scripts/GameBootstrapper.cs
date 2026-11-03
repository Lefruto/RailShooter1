using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Runtime bootstrapper that automatically guarantees all core gameplay singletons,
/// managers, HUD components, and wave systems exist and are fully wired at runtime,
/// both in the Unity Editor and in standalone builds.
/// Also includes F12 shortcut to instantly capture submission screenshots.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    private static int screenshotIndex = 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitCoreManagers()
    {
        // 1. Ensure AudioManager exists
        if (Object.FindFirstObjectByType<AudioManager>() == null)
        {
            GameObject amGo = new GameObject("AudioManager");
            amGo.AddComponent<AudioManager>();
            Object.DontDestroyOnLoad(amGo);
        }

        // 2. Ensure GameSceneManager exists
        if (Object.FindFirstObjectByType<GameSceneManager>() == null)
        {
            GameObject gsmGo = new GameObject("GameSceneManager");
            gsmGo.AddComponent<GameSceneManager>();
            Object.DontDestroyOnLoad(gsmGo);
        }

        // 3. Ensure Screenshot Helper exists
        if (Object.FindFirstObjectByType<GameBootstrapper>() == null)
        {
            GameObject helper = new GameObject("GameBootstrapperHelper");
            helper.AddComponent<GameBootstrapper>();
            Object.DontDestroyOnLoad(helper);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitSceneSystems()
    {
        // 4. Ensure Canvas has Scoreboard HUD
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            if (canvas.GetComponent<Scoreboard>() == null)
            {
                canvas.gameObject.AddComponent<Scoreboard>();
            }
        }

        // 5. Ensure Enemy Manager has WaveSpawner
        GameObject enemyMgr = GameObject.Find("Enemy Maneger") ?? GameObject.Find("Enemy Manager");
        if (enemyMgr != null)
        {
            if (enemyMgr.GetComponent<WaveSpawner>() == null)
            {
                enemyMgr.AddComponent<WaveSpawner>();
            }
        }
        else
        {
            GameObject newMgr = new GameObject("Enemy Maneger");
            newMgr.AddComponent<WaveSpawner>();
        }
    }

    void Update()
    {
        // Press F12 to capture assignment screenshots with exact naming convention
        bool f12Pressed = false;
        if (Keyboard.current != null)
        {
            f12Pressed = Keyboard.current.f12Key.wasPressedThisFrame;
        }
        else
        {
            f12Pressed = Input.GetKeyDown(KeyCode.F12);
        }

        if (f12Pressed)
        {
            string filename = $"RailShooter_Ali_Build_S{screenshotIndex:D2}.png";
            ScreenCapture.CaptureScreenshot(filename);
            Debug.Log($">>> Captured homework screenshot: {filename} <<<");
            screenshotIndex = (screenshotIndex >= 3) ? 1 : screenshotIndex + 1;
        }
    }
}

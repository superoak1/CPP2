using UnityEngine;
using UnityEngine.UI;

public class GameOverUIBinder : MonoBehaviour
{
    // Change these to your actual child object names if different
    private const string RespawnButtonName = "RespawnButton";
    private const string TryAgainButtonName = "TryAgainButton";
    private const string MainMenuButtonName = "MainMenuButton";
    private const string QuitButtonName = "QuitButton";

    [SerializeField] private GameOverManager manager; // optional; auto-fills
    [SerializeField] private bool autoFindButtonsByName = true;

    // Optional manual references (used if autoFindButtonsByName = false or fallback)
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (!manager) manager = FindFirstObjectByType<GameOverManager>(FindObjectsInactive.Include);
        if (!manager)
        {
            Debug.LogError("GameOverUIBinder: No GameOverManager found in scene.");
            return;
        }

        if (autoFindButtonsByName)
        {
            // Try to find by child names first
            respawnButton = respawnButton ? respawnButton : FindButtonDeep(transform, RespawnButtonName);
            tryAgainButton = tryAgainButton ? tryAgainButton : FindButtonDeep(transform, TryAgainButtonName);
            mainMenuButton = mainMenuButton ? mainMenuButton : FindButtonDeep(transform, MainMenuButtonName);
            quitButton = quitButton ? quitButton : FindButtonDeep(transform, QuitButtonName);
        }

        WireAll();
    }

    private void WireAll()
    {
        // Clear old listeners (prevents double-binding if panel is re-enabled)
        Clear(respawnButton);
        Clear(tryAgainButton);
        Clear(mainMenuButton);
        Clear(quitButton);

        // Bind if method exists on the manager
        // (Merged manager)
        SafeBind(respawnButton, manager, nameof(GameOverManager.Respawn));
        SafeBind(tryAgainButton, manager, nameof(GameOverManager.TryAgain));
        SafeBind(mainMenuButton, manager, nameof(GameOverManager.GoToMainMenu));
        SafeBind(quitButton, manager, nameof(GameOverManager.QuitGame));

        // (Legacy manager fallback where only TryAgain/EndGame exist)
        if (!HasListener(tryAgainButton)) SafeBind(tryAgainButton, manager, nameof(GameOverManager.TryAgain));
        if (!HasListener(quitButton)) SafeBind(quitButton, manager, nameof(GameOverManager.EndGame));
    }

    // --- Helpers ---

    private static Button FindButtonDeep(Transform root, string name)
    {
        var t = FindDeep(root, name);
        return t ? t.GetComponent<Button>() : null;
    }

    private static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindDeep(root.GetChild(i), name);
            if (found) return found;
        }
        return null;
    }

    private static void Clear(Button b)
    {
        if (!b) return;
        b.onClick.RemoveAllListeners();
    }

    private static bool HasListener(Button b) => b && b.onClick.GetPersistentEventCount() > 0;

    private static void SafeBind(Button b, GameOverManager mgr, string methodName)
    {
        if (!b || mgr == null) return;

        // Use reflection to check if the method exists and is public/instance/no params
        var mi = typeof(GameOverManager).GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (mi == null || mi.GetParameters().Length != 0) return;

        b.onClick.AddListener(() => mi.Invoke(mgr, null));
        b.interactable = true;
    }
}

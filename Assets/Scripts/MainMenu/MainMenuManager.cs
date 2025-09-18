using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;      // Play, Settings, Exit
    [SerializeField] private GameObject settingsPanel;  // Volume slider, Back

    [Header("Audio Settings")]
    [SerializeField] private Slider volumeSlider;

    [Header("Game Settings")]
    [SerializeField] private string gameplaySceneName = "GameScene"; // change to your scene name

    private void Start()
    {
        ShowMain();

        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            AudioListener.volume = volumeSlider.value;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        ShowMain();
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ShowMain()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    private void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }
}

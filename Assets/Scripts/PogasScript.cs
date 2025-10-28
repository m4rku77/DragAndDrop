using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PogasScript : MonoBehaviour
{
    public Button RESTARTBUTTON;
    public Button MENU;

    void Start()
    {
        // Assign listeners to buttons
        RESTARTBUTTON.onClick.AddListener(RestartScene);
        MENU.onClick.AddListener(GoToMenu);
    }

    void RestartScene()
    {
        // Reloads the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    void GoToMenu()
    {
        // Loads the menu scene — make sure "MainMenu" matches your menu scene name
        SceneManager.LoadScene("SakumsScene");
    }
}

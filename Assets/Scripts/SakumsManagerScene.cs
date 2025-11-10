using UnityEngine;
using UnityEngine.SceneManagement;

public class SakumsManagerScene : MonoBehaviour
{
    public AudioClip click; // assign your sound in Inspector
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void PlayClickSound()
    {
        if (audioSource != null && click != null)
        {
            audioSource.PlayOneShot(click);
        }
    }

    public void LoadLimenis1()
    {
        PlayClickSound();
        SceneManager.LoadScene("CityScene"); // name of your first level scene
    }

    public void LoadLimenis2()
    {
        PlayClickSound();
        SceneManager.LoadScene("HanojasTornis"); // name of your second level scene
    }

    public void Iziet()
    {
        PlayClickSound();
        Debug.Log("Exiting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

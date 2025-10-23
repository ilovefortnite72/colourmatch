using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsMenu;

    public void PlayButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OptionsButton()
    {
        optionsMenu.SetActive(true);
    }


    public void QuitButton()
    {
        Application.Quit();
    }
}

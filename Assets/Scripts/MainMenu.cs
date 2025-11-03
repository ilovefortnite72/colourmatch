using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject levelMenu;

    public void PlayButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void levelButton()
    {
        levelMenu.SetActive(true);
    }

    public void BackButton()
    {
        levelMenu.SetActive(false);
    }

    public void QuitButton()
    {
        Application.Quit();
    }


    public void SelectLevel()
    {
        GameObject buttonPressed = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (buttonPressed == null)
        {
            return;
        }


        string btnName = buttonPressed.name;
        int levelIndex = GetLevelNumber(btnName);
    }

    private int GetLevelNumber(string btnName)
    {
        string prefix = "BTN_SelectLevel";

        if(btnName.StartsWith(prefix))
        {
            string levelStr = btnName.Substring(prefix.Length);
            if(int.TryParse(levelStr, out int levelNum))
            {
                SceneManager.LoadScene(levelNum); // Assuming level scenes start from index 1
                return levelNum;
            }
        }
        return -1;
    }


    
}

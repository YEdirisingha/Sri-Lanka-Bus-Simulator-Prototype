using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject[] toSetActive;
    public void PlayGame()
    {
        foreach (GameObject obj in toSetActive)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    public void Exit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void Close()
    {
        foreach (GameObject obj in toSetActive)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    public void ColomboMap()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}

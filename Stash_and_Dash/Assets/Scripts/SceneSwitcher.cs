using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        Debug.Log("Button pressed! Loading scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}

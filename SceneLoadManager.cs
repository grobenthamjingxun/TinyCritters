using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    public int sceneIndex = 2;

    public void LoadScene(int sceneIndex)
    {
        Debug.Log("Loading scene index: " + sceneIndex);
        SceneManager.LoadScene(sceneIndex);
    }
}

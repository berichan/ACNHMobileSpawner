using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_ToggleUiMode : MonoBehaviour
{
    public string targetSceneName;

    public void OnClick()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}

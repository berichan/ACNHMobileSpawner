using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FixScroll : MonoBehaviour
{
    [MenuItem("ACNHMS/Set all scroll sensitivity to 200")]
    public static void FixScrollOfRects()
    {
        var allScrollRects = FindObjectsOfTypeSceneSafe<ScrollRect>();
        foreach (ScrollRect sr in allScrollRects)
            if (sr.scrollSensitivity == 1)
                sr.scrollSensitivity = 200;
    }

    public static List<T> FindObjectsOfTypeSceneSafe<T>()
    {
        List<T> results = new List<T>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                var allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {
                    var go = allGameObjects[j];
                    results.AddRange(go.GetComponentsInChildren<T>(true));
                }
            }
        }
        return results;
    }
}

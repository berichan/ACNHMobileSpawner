using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicSetter : MonoBehaviour
{
    private float lastWidth;
    private float lastHeight;

    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;
#if UNITY_STANDALONE
        int height = (int) (Screen.currentResolution.height * 0.75f);
        int width = (int)(height * (9f / 16f));
        Screen.SetResolution(width, height, false, 60);
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }
}

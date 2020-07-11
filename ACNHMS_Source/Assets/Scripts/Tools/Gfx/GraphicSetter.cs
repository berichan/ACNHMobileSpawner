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
        Screen.SetResolution((int)(Screen.height * (9f / 16f)), Screen.height, false, 60);
        Screen.SetResolution(Screen.width, (int)(Screen.width * (16f / 9f)), false, 60);
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }
}

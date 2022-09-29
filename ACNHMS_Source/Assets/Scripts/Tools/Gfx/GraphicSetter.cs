using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GraphicSetter : MonoBehaviour
{
    public float lastWidth;
    public float lastHeight;
    public bool scaleWindow;
    private string configPath = Path.Combine(Directory.GetCurrentDirectory(), "config.txt");

    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;
#if UNITY_STANDALONE
        float screenPercent = 1.0f;
        if (scaleWindow) screenPercent = 0.75f;

        if (File.Exists(configPath))
        {
            try
            {
                var pc = File.ReadAllText(configPath);
                var nSize = float.Parse(pc);
                if (nSize > 1)
                    nSize *= 0.01f;
                screenPercent = nSize;
            }
            catch { }
        }

        int height = (int) (lastHeight * screenPercent);
        int width = (int)(lastWidth * screenPercent);
        Screen.SetResolution(width, height, false, 60);
#endif
    }
}

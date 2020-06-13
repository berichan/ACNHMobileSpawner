using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class BuildTime : MonoBehaviour
{
    public string LastEditorDateTime;

    public Text TextToSet;
    // Start is called before the first frame update
    void Start()
    {
        if (!Application.isPlaying)
            return;

        TextToSet.text = LastEditorDateTime;
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        LastEditorDateTime = String.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now); 
#endif
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        // Ensure continuous Update calls.
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
#endif
    }
}

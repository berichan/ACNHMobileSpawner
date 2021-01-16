using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SysBotACOrderMode : MonoBehaviour
{
    public GameObject[] ToTurnOn;
    public GameObject[] ToTurnOff;

    public Renderer ToSwitchTexture;
    public Texture2D SwitchTex, OrigTex;

    public static SysBotACOrderMode CurrentInstance { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        CurrentInstance = this;
        var cMode = UI_Settings.GetCatalogueMode();
        Execute(!cMode);
    }

    public void Execute(bool flip = false)
    {
        ToSwitchTexture.material.mainTexture = flip ? OrigTex : SwitchTex;
        foreach (var on in ToTurnOn)
            on.gameObject.SetActive(!flip);
        foreach (var off in ToTurnOff)
            off.gameObject.SetActive(flip);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AdditionalButton : MonoBehaviour
{
    public Button Butt;
    public Text Label;
    public GameObject RootNeedsConnection;
    public IUI_Additional AssociatedPanel;

    public void SetActiveForConnection(bool connectionActive)
    {
        RootNeedsConnection.gameObject.SetActive(!connectionActive);
        Butt.interactable = connectionActive;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Butt == null)
            Butt = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

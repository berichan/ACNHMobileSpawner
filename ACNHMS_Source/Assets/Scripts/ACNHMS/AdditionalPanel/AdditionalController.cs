using NHSE.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdditionalController : MonoBehaviour
{
    public float PaddingX = 2, PaddingY = 1;
    public int MinButtonsPerRow = 3;
    public int MaxColumns = 2;

    public AdditionalButton TemplateButton;
    public RectTransform ButtonsWindow;

    private List<AdditionalButton> spawnedButtons;
    private IUI_Additional[] addPanels;

    private bool initialized = false;

    // Start is called before the first frame update
    void Start()
    {
        //spawn
        spawnedButtons = new List<AdditionalButton>();
        addPanels = GetComponentsInChildren<IUI_Additional>(true);
        for (int i = 0; i < addPanels.Length; ++i)
        {
            AdditionalButton newButt = Instantiate(TemplateButton.gameObject).GetComponent<AdditionalButton>();
            newButt.gameObject.SetActive(true);
            newButt.transform.SetParent(TemplateButton.transform.parent);
            newButt.transform.localScale = TemplateButton.transform.localScale;

            newButt.AssociatedPanel = addPanels[i];
            newButt.Label.text = addPanels[i].PanelName;
            int tempIByValue = i;
            newButt.Butt.onClick.AddListener(delegate { SetAdditionalOn(tempIByValue); });

            spawnedButtons.Add(newButt);
        }

        //set rects
        RectTransform TemplateRect = TemplateButton.GetComponent<RectTransform>();
        float buttonWidth = (ButtonsWindow.sizeDelta.x - (PaddingX * (MinButtonsPerRow - 1))) / MinButtonsPerRow;
        float buttonHeight = (ButtonsWindow.sizeDelta.y - (PaddingY * (MaxColumns - 1))) / MaxColumns;
        float buttonStartX = ButtonsWindow.anchoredPosition.x; float buttonStartY = ButtonsWindow.anchoredPosition.y;

        for (int i = 0; i < spawnedButtons.Count; ++i)
        {
            float newX = buttonStartX + (buttonWidth * (i % MinButtonsPerRow)) + ((i % MinButtonsPerRow) * PaddingX);
            int currentColumn = i / MinButtonsPerRow;
            Rect newRect = new Rect(newX,
                                    buttonStartY - (buttonHeight * currentColumn) - (PaddingY * currentColumn),
                                    buttonWidth,
                                    buttonHeight);
            spawnedButtons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(newRect.x, newRect.y) ;
            spawnedButtons[i].GetComponent<RectTransform>().sizeDelta = new Vector2(newRect.width, newRect.height);
        }


        initialized = true;
        OnEnable();
    }

    private void OnEnable()
    {
        if (!initialized)
            return;

        UI_ACItemGrid currentlyLoadedGrid = UI_ACItemGrid.LastInstanceOfItemGrid;

        if (currentlyLoadedGrid == null)
            return;

        IRAMReadWriter currentWriter = currentlyLoadedGrid.GetCurrentlyActiveReadWriter();
        
        foreach (AdditionalButton b in spawnedButtons)
        {
            if (b.AssociatedPanel.RequiresActiveConnection)
            {
                if (currentWriter != null)
                    b.AssociatedPanel.CurrentConnection = currentWriter;

                b.SetActiveForConnection(currentWriter != null);
            }
            else
                b.SetActiveForConnection(true);
        }

    }

    public void SetAdditionalOn(int indexOfAdditionalPanel)
    {
        foreach (IUI_Additional additional in addPanels)
            additional.gameObject.SetActive(false);

        addPanels[indexOfAdditionalPanel].gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

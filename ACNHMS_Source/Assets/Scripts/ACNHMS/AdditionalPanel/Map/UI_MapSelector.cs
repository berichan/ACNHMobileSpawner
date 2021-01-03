using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_MapSelector : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public delegate void onSelectorValueChanged(Vector2 absPosition);

    public readonly Vector4 SelectionBounds = new Vector4(-((32 * 7 * 4) / 2) + 32,
                                                 ((32 * 6 * 4) / 2) - 32,
                                                 ((32 * 7 * 4) / 2) - 32,
                                                 -((32 * 6 * 4) / 2) + 32);
    public Vector2 SelectorQuarter = new Vector2(16, 16);
    public MaskableGraphic Selector;

    public onSelectorValueChanged OnSelectorChanged;

    // Start is called before the first frame update
    void Start()
    {
        EventSystem.current.pixelDragThreshold = 1;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void updatePositionWorldSpace(Vector2 nPos)
    {
        // set in world space, keep z
        Vector2 pos = Selector.transform.position;
        pos.x = nPos.x;
        pos.y = nPos.y;
        Selector.transform.position = pos;

        // clamp in canvas space
        pos = Selector.rectTransform.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x, SelectionBounds.x, SelectionBounds.z);
        pos.y = Mathf.Clamp(pos.y, SelectionBounds.w, SelectionBounds.y);
        Selector.rectTransform.anchoredPosition = pos;

        Vector2 absPos = new Vector2();
        absPos.x = (pos.x / SelectionBounds.z) * -1;
        absPos.y = pos.y / SelectionBounds.y;
        absPos.x = 1 - ((absPos.x + 1) / 2);
        absPos.y = 1 - ((absPos.y + 1) / 2);
        Debug.Log($"{absPos.x},{absPos.y}");
        OnSelectorChanged(absPos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.dragging)
            updatePositionWorldSpace(eventData.position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        updatePositionWorldSpace(eventData.position);
    }
}

using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_MapSelector : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    private readonly Vector4 SelectionBounds = new Vector4(-((32 * 7 * 4) / 2) + 64,
                                                 ((32 * 6 * 4) / 2) - 64,
                                                 ((32 * 7 * 4) / 2) - 64,
                                                 -((32 * 6 * 4) / 2) + 64);
    public Vector2 SelectorQuarter = new Vector2(32, 32);
    public MaskableGraphic Selector;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void updatePosition(Vector2 nPos)
    {
        nPos *= 1f/Selector.canvas.scaleFactor;
        nPos.x += SelectionBounds.x - (SelectorQuarter.x * 1f/Selector.canvas.scaleFactor);
        nPos.y += SelectionBounds.w - (SelectorQuarter.y * 1f/Selector.canvas.scaleFactor) - (SelectorQuarter.y*Selector.transform.localScale.y);
        var pos = Selector.rectTransform.anchoredPosition;
        pos.x = Mathf.Clamp(nPos.x, SelectionBounds.x, SelectionBounds.z);
        pos.y = Mathf.Clamp(nPos.y, SelectionBounds.w, SelectionBounds.y);
        Selector.rectTransform.anchoredPosition = pos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.dragging)
            updatePosition(eventData.position);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        updatePosition(eventData.position);
    }
}

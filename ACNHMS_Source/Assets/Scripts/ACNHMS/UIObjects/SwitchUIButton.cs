using UnityEngine.EventSystems;
using UnityEngine;
using NHSE.Injection;

public class SwitchUIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public SwitchUIController Controller;
    public SwitchButton ToPress;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Controller.Press(ToPress, false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Controller.Press(ToPress, true);
    }
}

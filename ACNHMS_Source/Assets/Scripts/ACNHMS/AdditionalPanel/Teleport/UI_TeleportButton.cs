using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_TeleportButton : MonoBehaviour
{
    public PosRotAnchor AssignedAnchor;
    public UI_Teleport Parent;
    public Text TeleportName;

    public void TeleportToMe()
    {
        Parent.SendAnchorToGame(AssignedAnchor);
    }
    
    public void DeleteMe()
    {
        Parent.DeleteAnchor(AssignedAnchor);
    }

    public void LoadMe()
    {

    }
}

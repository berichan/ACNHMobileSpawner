using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NHSE.Core;
using UnityEngine.UI;

public class UI_MapItemTile : MonoBehaviour
{
    private Item mainItem;
    private Item extRightItem;
    private Item extDiagItem;
    private Item extDownItem;

    public RawImage Image;
    public RawImage Background;

    public void SetItem(Item main, Item right, Item diag, Item down, Color bg)
    {
        mainItem = main;
        extRightItem = right;
        extDiagItem = diag;
        extDownItem = down;
        var tex = SpriteBehaviour.ItemToTexture2D(main, out var c);
        Image.texture = tex;
        Background.color = main.ItemId == Item.NONE ? bg : c;
        Image.gameObject.SetActive(main.ItemId != Item.NONE);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

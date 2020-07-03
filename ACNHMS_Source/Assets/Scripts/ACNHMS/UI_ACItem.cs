using NHSE.Core;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class UI_ACItem : MonoBehaviour
{
	public RawImage ImageComponent;
	public Button ButtonComponent;
	public Text[] FiveInts;
	public bool Dummy;

	[HideInInspector]
	public Item ItemAssigned;

	private void Start()
	{
		if (!Dummy)
		{
			//Assign(Item.NO_ITEM);
		}
    }

	private void Update()
	{
	}

	public void Assign(Item item)
	{
        //IL_0026: Unknown result type (might be due to invalid IL or missing references)
        //IL_006f: Unknown result type (might be due to invalid IL or missing references)
        ItemAssigned = item;
        Texture2D imageToAssign = SpriteBehaviour.ItemToTexture2D(ItemAssigned, out var c);
        ImageComponent.texture = imageToAssign;
        ImageComponent.color = c;
        
        FiveInts[0].text = item.Count.ToString();
        FiveInts[1].text = item.SystemParam.ToString();
        FiveInts[2].text = item.AdditionalParam.ToString();
        FiveInts[3].text = 0.ToString();
        FiveInts[4].text = item.UseCount.ToString();
        Text[] fiveInts = FiveInts;
        foreach (Text val in fiveInts)
            if (val.text == 0.ToString())
                val.text = "";
    }

    // for downloading acnh images
    public void DownloadImages()
    {
        System.IO.Compression.ZipFile.ExtractToDirectory("", "");
    }
    
}

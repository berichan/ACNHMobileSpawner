using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FlowerEditor : MonoBehaviour
{
	public Toggle R1;
	public Toggle R2;
	public Toggle Y1;
	public Toggle Y2;
	public Toggle W1;
	public Toggle W2;
	public Toggle S1;
	public Toggle S2;

	public Toggle Watered;
	public Toggle GoldCanWatered;
	public InputField DaysWatered;

	public Toggle V1;
	public Toggle V2;
	public Toggle V3;
	public Toggle V4;
	public Toggle V5;
	public Toggle V6;
	public Toggle V7;
	public Toggle V8;
	public Toggle V9;
	public Toggle V10;

	public bool[] GetVisitorWatered()
	{
		return new List<bool>
		{
			V1.isOn,
			V2.isOn,
			V3.isOn,
			V4.isOn,
			V5.isOn,
			V6.isOn,
			V7.isOn,
			V8.isOn,
			V9.isOn,
			V10.isOn
        }.ToArray();
	}

	public void SetVisitorWatered(bool[] indexedVillagerArray)
	{
		if (indexedVillagerArray.Length != 10)
		{
			Debug.LogError((object)"Villager watered array is no the expected length.");
		}
		V1.isOn = indexedVillagerArray[0];
		V2.isOn = indexedVillagerArray[1];
		V3.isOn = indexedVillagerArray[2];
		V4.isOn = indexedVillagerArray[3];
		V5.isOn = indexedVillagerArray[4];
		V6.isOn = indexedVillagerArray[5];
		V7.isOn = indexedVillagerArray[6];
		V8.isOn = indexedVillagerArray[7];
		V9.isOn = indexedVillagerArray[8];
		V10.isOn = indexedVillagerArray[9];
	}

	public void ResetToZero() // another bad il rebuild
	{
		Toggle r = R1;
		Toggle r2 = R2;
		Toggle y = Y1;
		Toggle y2 = Y2;
		Toggle w = W1;
		Toggle w2 = W2;
		Toggle s = S1;
		bool flag;
		S2.isOn = flag = false;
		bool flag2;
		s.isOn = flag2 = flag;
		bool flag3;
		w2.isOn = flag3 = flag2;
		bool flag4;
		w.isOn = flag4 = flag3;
		bool flag5;
		y2.isOn = flag5 = flag4;
		bool flag6;
		y.isOn = flag6 = flag5;
		bool isOn;
		r2.isOn = isOn = flag6;
		r.isOn = isOn;
		Toggle watered = Watered;
		GoldCanWatered.isOn = isOn = false;
		watered.isOn = isOn;
		DaysWatered.text=0.ToString();
		Toggle v = V1;
		Toggle v2 = V2;
		Toggle v3 = V3;
		Toggle v4 = V4;
		Toggle v5 = V5;
		Toggle v6 = V6;
		Toggle v7 = V7;
		Toggle v8 = V8;
		Toggle v9 = V9;
		bool flag7;
		V10.isOn = flag7 = false;
		bool flag8;
		v9.isOn = flag8 = flag7;
		v8.isOn = flag = flag8;
		v7.isOn = flag2 = flag;
		v6.isOn = flag3 = flag2;
		v5.isOn = flag4 = flag3;
		v4.isOn = flag5 = flag4;
		v3.isOn = flag6 = flag5;
		v2.isOn = isOn = flag6;
		v.isOn = isOn;
	}
    
}

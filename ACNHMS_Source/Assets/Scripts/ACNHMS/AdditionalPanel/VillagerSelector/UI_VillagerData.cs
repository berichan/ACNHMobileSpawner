using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NHSE.Core;
using System;
using UnityEngine.UI;

public class UI_VillagerData : MonoBehaviour
{
    public UI_Villager RootUIVillager;

    public Text VillagerName;
    public RawImage VillagerImg;

    public void LoadVillagerData()
    {
        doFileHandlerCheck();

        UI_NFSOACNHHandler.LastInstanceOfNFSO.OpenFile("nhv", sendVillagerData, Villager.SIZE);
    }

    public void LoadVillagerHouseData()
    {
        doFileHandlerCheck();

        UI_NFSOACNHHandler.LastInstanceOfNFSO.OpenFile("nhvh", sendVillagerHouseData, VillagerHouse.SIZE);
    }

    public void SaveCurrentVillager()
    {
        doFileHandlerCheck();

        byte[] toSave = RootUIVillager.GetCurrentlyLoadedVillager().Data;
        string name = GameInfo.Strings.GetVillager(RootUIVillager.GetCurrentlyLoadedVillager().InternalName) + "_V_" + DateTime.Now.ToString("yyyyddMM_HHmmss") + ".nhv";
        UI_NFSOACNHHandler.LastInstanceOfNFSO.SaveFile(name, toSave);
    }

    public void SaveCurrentHouse()
    {
        doFileHandlerCheck();

        byte[] toSave = RootUIVillager.GetCurrentLoadedVillagerHouse().Data;
        string name = GameInfo.Strings.GetVillager(RootUIVillager.GetCurrentlyLoadedVillager().InternalName) + "_H_" + DateTime.Now.ToString("yyyyddMM_HHmmss") + ".nhvh";
        UI_NFSOACNHHandler.LastInstanceOfNFSO.SaveFile(name, toSave);
    }

    private void sendVillagerData(byte[] data)
    {
        RootUIVillager.WriteVillagerDataVillager(new Villager(data));
        gameObject.SetActive(false);
    }

    private void sendVillagerHouseData(byte[] data)
    {
        RootUIVillager.WriteVillagerDataHouse(new VillagerHouse(data));
        gameObject.SetActive(false);
    }

    private void doFileHandlerCheck()
    {
        if (UI_NFSOACNHHandler.LastInstanceOfNFSO == null)
            throw new Exception("File handler has been unloaded.");
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

namespace ACNHMS.Sebbett
{
    [System.Serializable]
    public class VillagerData
    {
        public string _id;
        public string _gender;
        public string _name;
        public string _species;

        public VillagerData(string g, string n, string i, string s)
        {
            _id = i;
            _gender = g;
            _name = n;
            _species = s;
        }
    }

    [Serializable]
    public class VillagerLibrary
    {
        public VillagerData[] library;
    }

    public class UI_VillagerDataCompile : MonoBehaviour
    {
        public VillagerLibrary vl;
        public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        private void Awake()
        {
            LoadFiles();
        }

        public void LoadFiles()
        {
            string[] genders = File.ReadAllLines("villager_gender.txt");
            string[] names = File.ReadAllLines("villager_name.txt");
            string[] ids = File.ReadAllLines("villager_id.txt");
            string[] species = File.ReadAllLines("villager_species.txt");

            int len = names.Length;

            List<VillagerData> newVillagerData = new List<VillagerData>();
            for(int i = 0; i < len; i++)
            {
                VillagerData villagerData = new VillagerData(genders[i], names[i], ids[i], species[i]);
                newVillagerData.Add(villagerData);
            }
            vl.library = newVillagerData.ToArray();

            using (StreamWriter stream = new StreamWriter("villagers.json"))
            {
                string json = JsonUtility.ToJson(vl);
                stream.Write(json);
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_VillagerLookup : MonoBehaviour
{
    public GameObject child;
    public GameObject SearchBoxContent;
    public GameObject SearchBoxItemPrefab;
    public Dropdown speciesFilterDD, genderFilterDD;

    public List<GameObject> currentSearchItems = new List<GameObject>();

    public VillagerLibrary villagerLibrary;

    private void Awake()
    {
        InitData();
        InitUI();
        UpdateSearchResults();
    }

    private void InitData()
    {
        using (StreamReader stream = new StreamReader("villagers.json"))
        {
            string str = stream.ReadToEnd();
            VillagerLibrary newVL = JsonUtility.FromJson<VillagerLibrary>(str);
            villagerLibrary = newVL;
        }
    }

    private void InitUI()
    {
        speciesFilterDD.value=0;
        genderFilterDD.value=0;
    }

    string _searchFilter = "";
    SpeciesFilters _speciesFilter = 0;
    GenderFilters _genderFilter = 0;

    //UI Element Calls
    public void Open() => child.SetActive(true);
    public void Close() => child.SetActive(false);
    public void UpdateSearchFilter(string filter) { _searchFilter = filter; UpdateSearchResults(); }
    public void UpdateSpeciesFilter(Dropdown filter) { _speciesFilter = (SpeciesFilters)filter.value; UpdateSearchResults(); }
    public void UpdateGenderFilter(Dropdown filter) { _genderFilter = (GenderFilters)filter.value; UpdateSearchResults(); }

    private void UpdateSearchResults()
    {
        #region compile search
        List<VillagerData> filteredBySearch = new List<VillagerData>();
        foreach(VillagerData vd in villagerLibrary.library)
        {
            if(_searchFilter != "" && _searchFilter != null)
            {
                if(vd._name.ToLower().Contains(_searchFilter.ToLower()))
                    filteredBySearch.Add(vd);
            }
            else
            {
                filteredBySearch.Add(vd);
            }
        }

        List<VillagerData> filteredBySpecies = new List<VillagerData>();
        foreach (VillagerData vd in filteredBySearch)
        {
            if (_speciesFilter != SpeciesFilters.Any)
            {
                if (vd._species == _speciesFilter.ToString())
                    filteredBySpecies.Add(vd);
            }
            else
            {
                filteredBySpecies.Add(vd);
            }
        }

        List<VillagerData> filteredByGender = new List<VillagerData>();
        foreach(VillagerData vd in filteredBySpecies)
        {
            if(_genderFilter != GenderFilters.Any)
            {
                if(vd._gender == _genderFilter.ToString())
                    filteredByGender.Add(vd);
            }
            else
            {
                filteredByGender.Add(vd);
            }
        }

        List<VillagerData> searchResults = new List<VillagerData>();
        foreach(VillagerData vd in filteredByGender)
        {
            searchResults.Add(vd);
        }
        filteredBySearch = new List<VillagerData>();
        filteredBySpecies = new List<VillagerData>();
        filteredByGender = new List<VillagerData>();
        #endregion

        #region manage UI elements
        for(int i = 0; i < currentSearchItems.Count; i++)
        {
            Destroy(currentSearchItems[i]);
        }

        List<GameObject> newSearchItems = new List<GameObject>();
        foreach(VillagerData vd in searchResults)
        {
            GameObject newItem = Instantiate(SearchBoxItemPrefab, SearchBoxContent.transform.position, SearchBoxContent.transform.rotation);
            newItem.transform.parent = SearchBoxContent.transform;
            newItem.transform.localScale = Vector3.one;
            newItem.GetComponent<UI_VillagerSearchButton>().SetData(vd._id, vd._name, vd._species, vd._gender, GetVillagerSprite(vd._id));
            newSearchItems.Add(newItem);
        }
        currentSearchItems = newSearchItems;
        #endregion
    }

    private Sprite GetVillagerSprite(string id)
    {
        return Resources.Load("villagers/" + id, typeof(Sprite)) as Sprite;
    }
}

public enum SpeciesFilters
{
    Any=0,
    Alligator=1,
    Anteater=2,
    Bear=3,
    Bird=4,
    Bull=5,
    Cat=6,
    Chicken=7,
    Cow=8,
    Cub=9,
    Deer=10,
    Dog=11,
    Duck=12,
    Eagle=13,
    Elephant=14,
    Frog=15,
    Goat=16,
    Gorilla=17,
    Hamster=18,
    Hippo=19,
    Horse=20,
    Kangaroo=21,
    Koala=22,
    Lion=23,
    Monkey=24,
    Mouse=25,
    Octopus=26,
    Ostrich=27,
    Penguin=28,
    Pig=29,
    Rabbit=30,
    Rhino=31,
    Sheep=32,
    Squirrel=33,
    Tiger=34,
    Wolf=35
}
public enum GenderFilters
{
    Any=0,
    Male=1,
    Female=2
}
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

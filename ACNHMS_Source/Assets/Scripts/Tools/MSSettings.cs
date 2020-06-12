using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class MSSettings 
{
    private readonly string path = Application.persistentDataPath + "/settings.xml";

    private static MSSettings currentInstance = new MSSettings();
    public static MSSettings CurrentSettings { get { return currentInstance; } }


    public Dictionary<string, string> SettingsDictionary;

    public MSSettings()
    {
        SettingsDictionary = new Dictionary<string, string>();
    }

    public string GetItem(string key, string defaultVal = "")
    {
        if (SettingsDictionary.ContainsKey(key))
            return SettingsDictionary[key];
        else
        {
            SetItem(key, defaultVal);
            return defaultVal;
        }

    }

    public void SetItem(string key, string val)
    {
        if (SettingsDictionary.ContainsKey(key))
            SettingsDictionary[key] = val;
        else
            SettingsDictionary.Add(key, val);
    }

    void load()
    {
        if (!File.Exists(path))
            save();

        var serializer = new XmlSerializer(typeof(SecondarySerializableDictionary<string, string>));
        var stream = new FileStream(path, FileMode.Open);
        var container = serializer.Deserialize(stream) as SecondarySerializableDictionary<string, string>;
        stream.Close();

        if (container != null)
            SettingsDictionary = container;
    }

    void save()
    {
        var serializer = new XmlSerializer(typeof(SecondarySerializableDictionary<string, string>));
        var stream = new FileStream(path, FileMode.Create);
        var container = ToSerializable();
        serializer.Serialize(stream, container);
        stream.Close();
    }

    // dictionaries can't be serialized, but this can
    public SecondarySerializableDictionary<string, string> ToSerializable()
    {
        return (SecondarySerializableDictionary<string, string>)SettingsDictionary;
    }
}

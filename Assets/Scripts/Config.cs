using UnityEngine;
using System.Collections.Generic;
using TinyJSON;

[System.Serializable]
public class Config
{
    public bool validateTags = false;
    public List<string> validTags = new List<string>();

    [TinyJSON.Skip]
    public const string filename = "config.json";

    public static Config Load()
    {
        Config obj = null;
        string jsonString = Utils.LoadTextFile(Application.dataPath + "/../" + filename);
        JSON.MakeInto<Config>(JSON.Load(jsonString), out obj);

        return obj;
    }
}
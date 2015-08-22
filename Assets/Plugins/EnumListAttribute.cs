using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnumListAttribute : PropertyAttribute
{
    System.Type E;
    List<string> valueNames = new List<string>();

    public EnumListAttribute(System.Type T)
    {
        E = T;
        valueNames = new List<string>();

        System.Array stateTypes = System.Enum.GetValues(E);
        for (int i = 0; i < stateTypes.Length; i++)
        {
            valueNames.Add(stateTypes.GetValue(i).ToString());
        }
    }

    public string GetName(int index)
    {
        if (index >= 0 && index < valueNames.Count)
        {
            return valueNames[index];
        }
        return "    "+index.ToString();
    }
}
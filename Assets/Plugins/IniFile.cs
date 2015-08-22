using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;


public class IniFile
{
	public static readonly string BasePath = Application.dataPath + "/../";

	public static Dictionary<string, Hashtable> LoadFile(string filename)
	{
		string path = BasePath + filename;

		if(!File.Exists(path))
		{
			Debug.LogError("File not found: " + path);
			return null;
		}

		var output = new Dictionary<string, Hashtable>();
		var streamReader = new StreamReader(path);

		var sectionHeader = new Regex("^\\[(.+)\\]");
		var pair = new Regex("^(.+)=(.+)");
		var comment = new Regex("^;.+?");

		string sectionName = "";
		Hashtable sectionTable = null;

		while(!streamReader.EndOfStream)
		{
			var line = streamReader.ReadLine();

			if(comment.IsMatch(line))
			{
				continue;
			}
			else if(sectionHeader.IsMatch(line))
			{
				sectionName = sectionHeader.Match(line).Groups[1].Value;
				sectionTable = new Hashtable();
				output.Add(sectionName, sectionTable);
			}
			else if(pair.IsMatch(line))
			{
				var match = pair.Match(line);

				var key = match.Groups[1].Value;

				string  valueString = match.Groups[2].Value;
				int valueInt;
				float valueFloat;
				bool valueBool;

				if(int.TryParse(valueString, out valueInt))
				{
					sectionTable.Add(key, valueInt);
				}
				else if(float.TryParse(valueString, out valueFloat))
				{
					sectionTable.Add(key, valueFloat);
				}
				else if(bool.TryParse(valueString, out valueBool))
				{
					sectionTable.Add(key, valueBool);
				}
				else
				{
					sectionTable.Add(key, valueString);
				}
			}

			// silently ignore everything else
		}

		streamReader.Close();
		
		return output;
	}

	public static string ToIni(Hashtable data)
	{
		StringBuilder output = new StringBuilder("");

		foreach(DictionaryEntry section in data)
		{
			output.AppendLine(string.Format("[{0}]", section.Key));

			foreach(DictionaryEntry pair in section.Value as Hashtable)
			{
				output.AppendLine(string.Format("{0}={1}", pair.Key, pair.Value.ToString()));
			}
		}

		return output.ToString();
	}
}

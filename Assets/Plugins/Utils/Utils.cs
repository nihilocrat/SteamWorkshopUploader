using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Utils
{
	static public object GetPropertyValue(object src, string propName)
	{
		System.Reflection.PropertyInfo i = src.GetType().GetProperty(propName);
		
		// try a field instead of a property
		if(i == null)
		{
			System.Reflection.FieldInfo f = src.GetType().GetField(propName);
			return f.GetValue(src);
		}
		
		return i.GetValue(src, null);
	}
	
	static public void SetPropertyValue(object src, string propName, object val)
	{
		System.Reflection.PropertyInfo i = src.GetType().GetProperty(propName);
		
		// try a field instead of a property
		if(i == null)
		{
			System.Reflection.FieldInfo f = src.GetType().GetField(propName);
			f.SetValue(src, val);
		}
		
		i.SetValue(src, val, null);
	}
	
	public static void CallMethod(object src, string methodName, object arg)
	{
		CallMethod(src, methodName, new object[]{ arg });
	}
	
	public static void CallMethod(object src, string methodName, object[] args)
	{
		System.Reflection.MethodInfo i = src.GetType().GetMethod(methodName);
		
		if(i != null)
		{
			i.Invoke(src, args);
		}
		else
		{
			Debug.LogError(string.Format("CallMethod(): Method not found: {0}.{1}", src.GetType().ToString(), methodName));
		}
	}

	static public float GetLocalAngleTowards(Transform sourceTransform, Vector3 targetPosition)
	{
		var vec2Target = targetPosition - sourceTransform.position;//sourceTransform.TransformDirection(sourceTransform.position - targetTransform.position);
		
		//Debug.DrawLine(sourceTransform.position, sourceTransform.position + vec2Target, Color.cyan);
		
		var localVec2Target = sourceTransform.InverseTransformDirection(vec2Target);
		
		//Debug.DrawLine(targetDirection.position, targetDirection.position + localVec2Target, Color.cyan);

		localVec2Target.z = 0.0f;
		var lookAngle = Vector3.Angle(Vector3.up, localVec2Target);
		if(localVec2Target.x > 0.0f)
		{
			lookAngle *= -1.0f;
		}

		return lookAngle;
	}

	static public Vector3 GetLocalAnglesTowards(Transform sourceTransform, Vector3 targetPosition)
	{
		var vec2Target = targetPosition - sourceTransform.position;//sourceTransform.TransformDirection(sourceTransform.position - targetTransform.position);
		
		//Debug.DrawLine(sourceTransform.position, sourceTransform.position + vec2Target, Color.cyan);
		
		var localVec2Target = sourceTransform.InverseTransformDirection(vec2Target);
		
		//Debug.DrawLine(targetDirection.position, targetDirection.position + localVec2Target, Color.cyan);

		Vector3 flatVec = new Vector3(localVec2Target.x, localVec2Target.y, 0f);
		float lookAngleZ = Vector3.Angle(Vector3.up, flatVec);
		if(flatVec.x > 0.0f)
		{
			lookAngleZ *= -1.0f;
		}

		float lookAngleX = Vector3.Angle(Vector3.forward, localVec2Target);

		return new Vector3(lookAngleX, 0f, lookAngleZ);
	}
	
	public static bool PointInViewport(Camera cam, Vector3 worldPoint)
	{
		var viewPoint = cam.WorldToViewportPoint(worldPoint);
		if(viewPoint.z <= 0f || viewPoint.x < 0f || viewPoint.x > 1f || viewPoint.y < 0f || viewPoint.y > 1f)
		{
			return false;
		}
		
		return true;
	}

	static public bool pointInsideAABB(Vector2 min, Vector2 max, Vector2 point)
	{
		return (point.x >= min.x && point.x <= max.x) &&
			(point.y >= min.y && point.y <= max.y);
	}
	
	static public bool AABBcollide(Vector2 min, Vector2 max, Vector2 min2, Vector2 max2)
	{
		return (min.x <= max2.x && max.x >= min2.x) &&
			(max.y >= min2.y && min.y <= max2.y);
	}

	static public void ScaleToBounds(Renderer renderer, Vector3 maxBounds)
	{
		// need to reset scale so that renderer.bounds.size calc is correct
		renderer.transform.localScale = Vector3.one;

		// if the fighter graphics are too big, make them a little smaller
		Vector3 gfxSize = renderer.bounds.size;
		Vector3 diffVec = maxBounds - gfxSize;

		float diff = 0f;
		
		// diff is in world space
		// convert to local space we can use in a scale vector
		if(diffVec.x < 0f)
		{
			diff = maxBounds.x / gfxSize.x; //diffVec.x;
		}
		else if(diffVec.y < 0f)
		{
			diff = maxBounds.y / gfxSize.y; //diffVec.y; 
		}
		else if(diffVec.z < 0f)
		{
			diff = maxBounds.z / gfxSize.z; //diffVec.z; 
		}

		if(diff != 0f)
		{
			renderer.transform.localScale = Vector3.one * diff;// + (Vector3.one * diff);
		}
	}

	static public float ScaleAnimationSpeed(AnimationState clip, float targetSpeed)
	{
		float clipPercent = targetSpeed / clip.length;
		float targetPercent = clip.length / targetSpeed;
		
		if(targetSpeed > clip.length)
		{
			return Mathf.Min(clipPercent, targetPercent);
		}
		else
		{
			return Mathf.Max(clipPercent, targetPercent);
		}
	}
	
	static public float StepRound(float value, float step)
	{
		float step2 = step / 2;
		float remain = value % step;
		if(remain > step2)
		{
			value += (step - remain);
		}
		else
		{
			value -= remain;
		}
		
		return value;
	}
	
	public static Color FromHTMLColor(string HTMLcolor)
	{
		if(HTMLcolor[0] == '#')
		{
			HTMLcolor = HTMLcolor.Replace("#","");
		}
		int redValue = Convert.ToInt32(HTMLcolor.Substring(0,2), 16);
		int greenValue = Convert.ToInt32(HTMLcolor.Substring(2,2), 16);
		int blueValue = Convert.ToInt32(HTMLcolor.Substring(4,2), 16);
		
		return new Color((float)redValue/255.0f, (float)greenValue/255.0f, (float)blueValue/255.0f);
	}
	
	public static string ToHTMLColor(Color color)
	{
		string htmlColorFormat = "#{0}{1}{2}";
		
		int redValue = (int)(color.r * 255);
		int greenValue = (int)(color.g * 255);
		int blueValue = (int)(color.b * 255);
		
		return string.Format(htmlColorFormat, redValue.ToString("X"), greenValue.ToString("X"), blueValue.ToString("X"));
	}
	
	static public string GetFileNameFromFleetName(string name, string fileType)
	{
		var output = "";
		foreach(string nameBit in name.Split(' '))
		{
			string bit = nameBit.ToLower();
			
			// do some smarter replacement here
			string[] invalidChars = { "'", "\"", ",", "." };
			foreach(var c in invalidChars)
			{
				bit = bit.Replace(c, "");
			}
			
			output += bit + "_";
		}
		output = output.Remove(output.Length-1,1);
		
		output += "."+ fileType +".json";
		
		return output;
	}

	
	static public string Serialize(object pocoObject)
	{
		return LitJson.JsonMapper.ToJson(pocoObject);
	}

	
	static public string LoadTextFile(string path)
	{
		string text;
		
		// try to load first from the filesystem, otherwise use a textasset
		// this allows modding, but also allows us to make a webplayer
		if(File.Exists(path))
		{
			var streamReader = new StreamReader(path, Encoding.UTF8);
			text = streamReader.ReadToEnd();
			streamReader.Close();
		}
		else
		{
			throw new IOException("File not found: "+ path);
		}
		
		return text;
	}

	static public string LoadJsonFromFile(string filename)
	{
		string path = Application.dataPath + "/Resources/Data/" + filename + ".txt";
		string text;
		
		// try to load first from the filesystem, otherwise use a textasset
		// this allows modding, but also allows us to make a webplayer
		if(false)//File.Exists(path))
		{
		    var streamReader = new StreamReader(path, Encoding.UTF8);
			text = streamReader.ReadToEnd();
			streamReader.Close();
		}
		else
		{
			var textAsset = Resources.Load("Data/" + filename) as TextAsset;
			if(textAsset == null)
				throw new IOException("File not found: "+ filename);
			text = textAsset.text;
		}
		
		return text;
	}
	
	static public bool SaveJsonToFile(string filename, string jsonString)
	{
		// this is a bit dumb, I think?
		FileStream fs;
		fs = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None);
		
		System.Byte[] info;
		info = new System.Text.UTF8Encoding(true).GetBytes(jsonString);
		fs.Write(info, 0, info.Length);
		fs.Close();
		
		Debug.Log("Wrote to " + filename);
		return true;
	}

	
	/// convert carinal number (1,2,3,4) to ordinal (1st,2nd,3rd,4th)
	static public string toOrdinal(int number) {
		string ending;
		if(number % 10 == 1) ending = "st";
		else if(number % 10 == 2) ending = "nd";
		else if(number % 10 == 3) ending = "rd";
		else ending = "th";
		
		return number.ToString() + ending;
	}
	
	static public float[,] Gridify(Vector3[] points)
	{
		var min_x =  9999.9f;
		var max_x = -9999.9f;
		
		var min_z =  9999.9f;
		var max_z = -9999.9f;
		
		foreach(var p in points)
		{
			if(p.x < min_x)
			{
				min_x = p.x;
			}
			if(p.x > max_x)
			{
				max_x = p.x;
			}
		
			if(p.z < min_z)
			{
				min_z = p.z;
			}
			if(p.z > max_z)
			{
				max_z = p.z;
			}
		}
		
		// determine origin and size given max, min
		var origin = new Vector3(Mathf.Floor(min_x), 0.0f, Mathf.Floor(min_z));
		//var size = new Vector3(max_x - min_x, 0.0f, max_z - min_z);
		var sizex = (int)(max_x - min_x);
		var sizez = (int)(max_z - min_z);
		
		Debug.Log("origin is " + origin + "   size is (" + sizex + "," + sizez +")");
		var grid = new float[(int)sizex+1, (int)sizez+1];
		
		foreach(var p in points)
		{
			Vector3 realpos = p; //- origin;
			int gridx = (int)Mathf.Floor(realpos.x);
			int gridz = (int)Mathf.Floor(realpos.z);
			//if(realpos.x < 0 || realpos.z < 0) continue;
			grid[gridx,gridz] = p.y;//true;
		}
		
		return grid;
	}

	
	// these two lists serves as building blocks to construt any number
	// just like coin denominations.
	// 1000->"M", 900->"CM", 500->"D"...keep on going 
	static int[] decimalDens={1000,900,500,400,100,90,50,40,10,9,5,4,1};
	static string[] romanDens={"M","CM","D","CD","C","XC","L","XL","X","IX","V","IV","I"};
	
/*
def toRoman(dec):
	"""
	Perform sanity check on decimal and throws exceptions when necessary
	"""		
        if dec <=0:
	  raise ValueError, "It must be a positive"
         # to avoid MMMM
	elif dec>=4000:  
	  raise ValueError, "It must be lower than MMMM(4000)"
  
	return decToRoman(dec,"",decimalDens,romanDens)

def decToRoman(num,s,decs,romans):
	"""
	  convert a Decimal number to Roman numeral recursively
	  num: the decimal number
	  s: the roman numerial string
	  decs: current list of decimal denomination
	  romans: current list of roman denomination
	"""
	if decs:
	  if (num < decs[0]):
	    # deal with the rest denomination
	    return decToRoman(num,s,decs[1:],romans[1:])		  
	  else:
	    # deduce this denomation till num<desc[0]
	    return decToRoman(num-decs[0],s+romans[0],decs,romans)	  
	else:
	  # we run out of denomination, we are done 
	  return s
	*/
	
	public static Texture2D DrawGridOnTexture(float[,] grid)
	{
		int width = grid.GetLength(0);
		int height = grid.GetLength(1);
		var newTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
		newTexture.filterMode = FilterMode.Point;
		
		int x;
		int y;
		// write the grid to the texture
		for(y=0; y<grid.GetLength(1); y++)
		{
			for(x=0; x<grid.GetLength(0); x++)
			{
				float v = grid[x,y];
				newTexture.SetPixel(x, y, new Color (v,v,v,v));
			}
		}
		
		newTexture.Apply();
		return newTexture;
	}
	
	static public float[,] Normalize(float[,] grid)
	{
		int sizex = grid.GetLength(0);
		int sizey = grid.GetLength(1);
		var nGrid = new float[sizex, sizey];
		
		int x;
		int y;
		float closest = Mathf.Infinity;
		float furthest = -1.0f;
		for(y=0; y<sizey; y++)
		{
			for(x=0; x<sizex; x++)
			{
				var value = grid[x,y];
				
				if(value > furthest)
					furthest = value;
				else if(value < closest)
					closest = value;
			}
		}
		
		// normalize grid to values between 0.0 and 1.0
		for(y=0; y<sizey; y++)
		{
			for(x=0; x<sizex; x++)
			{
				nGrid[x,y] = (grid[x,y] - closest) / (furthest - closest);
				if(nGrid[x,y] > 1.0f || nGrid[x,y] < 0.0f)
					Debug.Log("ERROR: normalized to " + nGrid[x,y]);
			}
		}
		
		return nGrid;
	}
	
	static public void TranslateUVs(GameObject obj, Vector2 offset)
	{
		var meshFilter = obj.GetComponent(typeof(MeshFilter)) as MeshFilter;
		Mesh mesh = meshFilter.mesh;
		
		var newUVs = new Vector2[mesh.uv.Length];
		
		for(int i=0; i < mesh.uv.Length; i++)
		{
			newUVs[i] = new Vector2(mesh.uv[i].x + offset.x, mesh.uv[i].y + offset.y);
		}
		
		mesh.uv = newUVs;
	}
	
	static public void SetUVs(GameObject obj, Vector2 offset)
	{
		var meshFilter = obj.GetComponent(typeof(MeshFilter)) as MeshFilter;
		Mesh mesh = meshFilter.mesh;
		
		var newUVs = new Vector2[mesh.uv.Length];
		
		for(int i=0; i < mesh.uv.Length; i++)
		{
			newUVs[i] = new Vector2(offset.x + 0.01f, offset.y - 0.01f);
		}
		
		mesh.uv = newUVs;
	}
	
	
	static public void TranslateWeaponUVs(GameObject obj, Vector2 offset)
	{
		var meshFilter = obj.GetComponent(typeof(MeshFilter)) as MeshFilter;
		Mesh mesh = meshFilter.mesh;
		
		var newUVs = new Vector2[mesh.uv.Length];
		
		for(int i=0; i < mesh.uv.Length; i++)
		{
			if(mesh.uv[i].x > 0.25f)
				newUVs[i] = new Vector2(mesh.uv[i].x + offset.x, mesh.uv[i].y + offset.y);
			else
				newUVs[i] = mesh.uv[i];
		}
		
		mesh.uv = newUVs;
	}
	
	public class ComponentSerializer
	{
		static public string Save(MonoBehaviour obj)
		{
			return LitJson.JsonMapper.ToJson(obj);
		}
		
		static public T Load<T>(string json)
		{
			return LitJson.JsonMapper.ToObject<T>(json);
		}
	}
	
	//List<string> lines = WrapText("Add some text", 300, "Calibri", 11);
    static List<string> WrapText(string text, double pixels, string fontFamily, float emSize)
    {
        string[] originalLines = text.Split(new string[] { " " }, StringSplitOptions.None);
        List<string> wrappedLines = new List<string>();

        StringBuilder actualLine = new StringBuilder();
        double actualWidth = 0;

        foreach (var item in originalLines)
        {
			/*
            FormattedText formatted = new FormattedText(item, CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight,
                                      new Typeface(fontFamily), emSize, Brushes.Black);
            actualLine.Append(item + " ");
            actualWidth += formatted.Width;

            if (actualWidth > pixels)
            {
                wrappedLines.Add(actualLine.ToString());
                actualLine.Clear();
                actualWidth = 0;
            }
            */
        }

        if(actualLine.Length > 0)
            wrappedLines.Add(actualLine.ToString());

        return wrappedLines;
    }
	
	/// <summary>
	/// Word wraps the given text to fit within the specified width.
	/// </summary>
	/// <param name="text">Text to be word wrapped</param>
	/// <param name="width">Width, in characters, to which the text
	/// should be word wrapped</param>
	/// <returns>The modified text</returns>
	public static string WordWrap(string text, int width)
	{
	    int pos, next;
	    StringBuilder sb = new StringBuilder();
	
	    // Lucidity check
	    if (width < 1)
	        return text;
	
	    // Parse each line of text
	    for (pos = 0; pos < text.Length; pos = next)
	    {
	        // Find end of line
	        int eol = text.IndexOf(Environment.NewLine, pos);
	        if (eol == -1)
	            next = eol = text.Length;
	        else
	            next = eol + Environment.NewLine.Length;
	
	        // Copy this line of text, breaking into smaller lines as needed
	        if (eol > pos)
	        {
	            do
	            {
	                int len = eol - pos;
	                if (len > width)
	                    len = BreakLine(text, pos, width);
	                sb.Append(text, pos, len);
	                sb.Append(Environment.NewLine);
	
	                // Trim whitespace following break
	                pos += len;
	                while (pos < eol && Char.IsWhiteSpace(text[pos]))
	                    pos++;
	            } while (eol > pos);
	        }
	        else sb.Append(Environment.NewLine); // Empty line
	    }
	    return sb.ToString();
	}
	
	/// <summary>
	/// Locates position to break the given line so as to avoid
	/// breaking words.
	/// </summary>
	/// <param name="text">String that contains line of text</param>
	/// <param name="pos">Index where line of text starts</param>
	/// <param name="max">Maximum line length</param>
	/// <returns>The modified line length</returns>
	private static int BreakLine(string text, int pos, int max)
	{
	    // Find last whitespace in line
	    int i = max;
	    while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
	        i--;
	
	    // If no whitespace found, break at maximum length
	    if (i < 0)
	        return max;
	
	    // Find start of whitespace
	    while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
	        i--;
	
	    // Return length of text before whitespace
	    return i + 1;
	}
	
	public static string GetMd5Hash(string input)
    {
		MD5 md5Hash = MD5.Create();

        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        StringBuilder sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data  
        // and format each one as a hexadecimal string. 
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        return sBuilder.ToString();
    }
	
	public static Transform FindInHierarchy(Transform current, string name)   
	{
	    // check if the current bone is the bone we're looking for, if so return it
	    if (current.name == name)
	        return current;
	 
	    // search through child bones for the bone we're looking for
	    for (int i = 0; i < current.GetChildCount(); ++i)
	    {
	        // the recursive step; repeat the search one step deeper in the hierarchy
	        Transform found = FindInHierarchy(current.GetChild(i), name);
	 
	        // a transform was returned by the search above that is not null,
	        // it must be the bone we're looking for
	        if (found != null)
	            return found;
	    }
	 
	    // bone with name was not found
	    return null;
	}

	public static string GetFullPath(Transform obj)
	{
		string path = "/" + obj.name;
		while (obj.parent != null)
		{
			obj = obj.parent;
			path = "/" + obj.name + path;
		}
		return path;
	}

	public static Texture2D LoadTextureFromFile(string savedImageName)
	{
		Texture2D tex = new Texture2D(2,2);
		
#if UNITY_WEBPLAYER
#else
        try
        {
            byte[] byteArray = File.ReadAllBytes(savedImageName);
            tex.LoadImage(byteArray);
        }
        catch
        {
            Debug.LogWarning("Tried to load preview file which doesn't exist (yet?): " + savedImageName);
            return null;
        }
#endif
		return tex;
	}

	public static void SetLayer(Transform root, int layer)
	{
		Stack<Transform> moveTargets = new Stack<Transform>();
		moveTargets.Push(root);
		Transform currentTarget;
		while(moveTargets.Count != 0)
		{
			currentTarget = moveTargets.Pop();
			currentTarget.gameObject.layer = layer;
			foreach(Transform child in currentTarget)
				moveTargets.Push(child);
		}
	}

	public static void PrepareVisualPrefab(GameObject obj)
	{
		obj.transform.localScale = Vector3.one;
		SetLayer(obj.transform, LayerMask.NameToLayer("GUI"));

		// disable all trail thingies
		foreach(var trail in obj.GetComponentsInChildren<TrailRenderer>())
		{
			trail.gameObject.SetActive(false);
		}
		// as well as audio sources
		foreach(var audio in obj.GetComponentsInChildren<AudioSource>())
		{
			audio.enabled = false;
		}
	}

	public static void AddBackfacesToMesh(Mesh mesh)
	{
		int j = 0;

		var vertices = mesh.vertices;
		var uv = mesh.uv;
		var normals = mesh.normals;
		var szV = vertices.Length;
		var newVerts = new Vector3[szV*2];
		var newUv = new Vector2[szV*2];
		var newNorms = new Vector3[szV*2];
		for (j=0; j< szV; j++)
		{
			// duplicate vertices and uvs:
			newVerts[j] = newVerts[j+szV] = vertices[j];
			newUv[j] = newUv[j+szV] = uv[j];
			// copy the original normals...
			newNorms[j] = normals[j];
			// and revert the new ones
			newNorms[j+szV] = -normals[j];
		}

		var triangles = mesh.triangles;
		var szT = triangles.Length;
		var newTris = new int[szT*2]; // double the triangles
		for (var i=0; i< szT; i+=3)
		{
			// copy the original triangle
			newTris[i] = triangles[i];
			newTris[i+1] = triangles[i+1];
			newTris[i+2] = triangles[i+2];
			// save the new reversed triangle
			j = i+szT; 
			newTris[j] = triangles[i]+szV;
			newTris[j+2] = triangles[i+1]+szV;
			newTris[j+1] = triangles[i+2]+szV;
		}

		mesh.vertices = newVerts;
		mesh.uv = newUv;
		mesh.normals = newNorms;
		mesh.triangles = newTris; // assign triangles last!
	}
}


public static class ExtensionMethods
{
	// Deep clone
	public static T DeepClone<T>(this T a)
	{
		using (MemoryStream stream = new MemoryStream())
		{
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, a);
			stream.Position = 0;
			return (T) formatter.Deserialize(stream);
		}
	}
}

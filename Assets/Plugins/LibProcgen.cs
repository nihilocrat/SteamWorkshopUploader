using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class LibProcgen {

	static public Vector3[] GenerateGrid(int xsize, int ysize)
	{
		return GenerateGrid(xsize, ysize, 1.0f, 1.0f);
	}
	
	static public Vector3[] GenerateGrid(int xsize, int ysize, float width, float height)
	{
		Vector3[] points = new Vector3[xsize*ysize];
		
		float widthPerPoint = width / xsize;
		float heightPerPoint = height / ysize;
		float x =0.0f, y = 0.0f;
		for(int i=0; i < xsize*ysize; i++)
		{
			if(x < xsize)
			{
				x += 1;
			}
			else
			{
				x = 0;
				y += 1;
			}
			points[i] = new Vector3(x * widthPerPoint, 0.0f, y * heightPerPoint);
		}
		
		return points;
	}
	
	static public List<Vector3[]> GetGridRegions( Vector3[] points, int xsize, int ysize )
	{
		var regions = new List<Vector3[]>();
		
		// find regions (polygons)
		for(int i=0; i < xsize*ysize; i++)
		{
			int x = (int)points[i].x;
			int y = (int)points[i].z;
			if(x < xsize && y < ysize)
			{
				Vector3[] poly = { points[i], points[i+1], points[i+x], points[i+x+1] };
				regions.Add(poly);
			}
		}
		
		return regions;
	}
	
	static public Vector3[] PerturbPoints( Vector3[] points, float perturbation )
	{
		for(var i=0; i<points.Length; i++)
		{
			var randomVec = (Random.insideUnitCircle * perturbation);
			points[i] = points[i] + new Vector3(randomVec.x, 0.0f, randomVec.y);
		}
		
		return points;
	}
	

	static public Vector3[] MakeCorners(Vector3 origin, float islandRadius, float bordersize)
	{
		int numCorners = 8;
		var myCorners = new Vector3[numCorners];
	
		var halfRadius = islandRadius / 2;
		var halfBorder = bordersize / 2;
	
		var width_variation = Random.value;
		var height_variation = 1.0 - width_variation;
		var islandWidth = Random.Range(islandRadius * 0.5f, islandRadius * 1.25f);
		var islandHeight = Random.Range(islandRadius * 0.5f, islandRadius * 1.25f);
		
		for(var i = 0; i < numCorners ; i++)
		{
			float localx = (Random.value - 0.5f) * islandRadius;
			float localy = Random.value * bordersize;
			float extraWidth = (Random.value - 0.5f) * islandWidth;
			float extraHeight = (Random.value - 0.5f) * islandHeight;
			float extraBorder = Random.value * bordersize;
			
			float px = 0.0f, py = 0.0f;
			
			if(i == 0 || i == 7 || i == 6) px = (-islandWidth);
			if(i == 1 || i == 5) px = 0.0f;
			if(i == 2 || i == 3 || i == 4) px = islandWidth;
			
			if(i == 0 || i == 1 || i == 2) py = islandHeight;
			if(i == 3 || i == 7) py = 0.0f;
			if(i == 4 || i == 5 || i == 6) py = (-islandHeight);
			
			// add perturbation
			//px += (Random.value - 0.5f) * bordersize;
			//py += (Random.value - 0.5f) * bordersize;
			/*
			// dumb but it works
			if( i == 0 ){ px = extraWidth; py = islandHeight + extraBorder; }
			if( i == 1 ){ px = islandWidth + extraBorder; py = islandHeight + extraBorder; }
			if( i == 2 ){ px = islandWidth + extraBorder; py = extraHeight; }
			if( i == 3 ){ px = islandWidth + extraBorder; py = (-islandHeight) - extraBorder; }
			if( i == 4 ){ px = extraWidth; py = (-islandHeight) - extraBorder; }
			if( i == 5 ){ px = (-islandWidth) - extraBorder; py = (-islandHeight) - extraBorder; }
			if( i == 6 ){ px = (-islandWidth) - extraBorder; py = extraHeight; }
			if( i == 7 ){ px = (-islandWidth) - extraBorder; py = islandHeight + extraBorder; }
			*/
			var pv = new Vector3(px + origin.x, origin.y, py + origin.z);
			myCorners[i] = pv;
		}
		
		return myCorners;
	}


	static public Vector3[] GenerateCoast(Vector3 corner1, Vector3 corner2, int iterations, float variation) {
		var points = new List<Vector3>();
		points.Add(corner1);
		points.Add(corner2);
		float currentVariation = variation;
	
		int i;
		for(int step = 0; step < iterations ; step++)
		{
			var newPoints = new List<Vector3>();
			for(i = 0; i < points.Count; i++)
			{
				int next_i = i+1;
				if( next_i >= points.Count ) continue;//next_i = 0;
				Vector3 point = points[i];
				Vector3 next_point = points[next_i];
				
				newPoints.Add( Bisect(point, next_point, currentVariation) );
			}
			
			// combine the two arrays
			// first an original point, then its midpoint
			var finalPoints = new List<Vector3>();
			int finalLength = points.Count * 2;
			for(i = 0; i < finalLength-1; i++)
			{
				if(i % 2 == 0){
					finalPoints.Add(points[0]);
					points.RemoveAt(0);
				}
				else {
					finalPoints.Add(newPoints[0]);
					points.RemoveAt(0);
				}
			}
			
			points = finalPoints;
			
			currentVariation /= 2;
		}
		
		return points.ToArray();
	}

	
	static public Vector3 Bisect(Vector3 pt1, Vector3 pt2, float variation) {
		var diff = (pt2 - pt1);
		var midpoint = pt1 + (diff / 2);
		var perp = Vector3.Cross(diff, Vector3.up);
		
		var offset = perp.normalized * (Random.value - 0.5f) * variation;
		
		return midpoint + offset;
	}
	
	static public Vector3 Midpoint(Vector3 pt1, Vector3 pt2)
	{
		//return pt1 + ((pt2 - pt1) / 2);
		return Vector3.Lerp(pt1, pt2, 0.5f);
	}

}

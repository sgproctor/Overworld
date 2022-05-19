using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class MapCells : IQuadTreeObject
{
    public Vector3 coord = new Vector3();
    public List<Vector3> neighbors = new List<Vector3>();
    public List<Vector3> vertices;
    public List<int> triangles;
    public float height = 0f;
    public float flux;
    public float erosionRate = 0f;
    public float slope = 0f;
    public float precipitation = 0f;
    public float temperature;
    public Biome biome;
    public float cityScore;
    public Settlement city;
    
    public MapCells(Vector2 midpoint, List<Vector3> vertices, List<int> triangles, float height)
    {
        this.coord = midpoint;
        this.vertices = vertices;
        this.triangles = triangles;
        this.height = height;
    }
    public MapCells(){}

    public List<float> getNeighborAngles()
    {
        List<float> neighborAngles = new List<float>();
        foreach(Vector2 point in neighbors)
        {
            neighborAngles.Add(Mathf.Atan2(coord.y - point.y, coord.x - point.x));
        }
        return neighborAngles;
    }

	public void setTemp(float equator, float waterLevel, float articY){
        float equatorToPole = articY - equator;
        float latitude = 90f * Mathf.Abs((float) coord.y - equator) / equatorToPole;
        //float latitude = (Mathf.Abs(coord.y-equator)/equatorToPole)*90f;
		float temps = -0.988f * (latitude) + 96.827f;
		temps = temps - 0.0026f * (Mathf.InverseLerp(waterLevel, 1f, height)) * 10000f;
		this.temperature = temps;
	}

	public void setBiome(Biome[] biomes){
		//float normalizedPrecipitation = precipitationLevel / maxPrecipitationVal;
		foreach(Biome biomeIteration in biomes){
			if(precipitation <= biomeIteration.maxPrecipitation 
				&& temperature <= biomeIteration.maxTemperature){
				this.biome = biomeIteration;
				return;
			}
		}
	}


    public Vector2 getNeighborClosestToAngle(float angle)
    {
        float minAngle = 1000f;
        List<float> angles = getNeighborAngles();
        Vector2 returnpoint = new Vector2();
        for (int i = 0; i < angles.Count; i++)
        {
            if(Mathf.Abs(angles[i] - angle) < minAngle)
            {
                minAngle = angles[i];
                returnpoint = neighbors[i];
            }
        }
        return returnpoint;
    }

    public Vector2 getNeighborClosestToPoint(Vector2 point)
    {
        Vector2 returnpoint = new Vector2();
        float distance = 100000f;
        foreach(Vector2 neighbor in neighbors)
        {
            if(Vector2.Distance(neighbor, point) < distance)
            {
                returnpoint = neighbor;
            }
        }
        return returnpoint;
    }

    public Vector2 GetPosition(){
        return coord;
    }

    public bool PointInCell(Vector2 p)
    {
        Vector2 p1, p2;
        bool inside = false;

        if (vertices.Count < 3)
        {
            return inside;
        }

        var oldPoint = vertices.Last();

        for (int i = 0; i < vertices.Count; i++)
        {
            var newPoint = vertices[i];

            if (newPoint.x > oldPoint.x)
            {
                p1 = oldPoint;
                p2 = newPoint;
            }
            else
            {
                p1 = newPoint;
                p2 = oldPoint;
            }

            if ((newPoint.x < p.x) == (p.x <= oldPoint.x)
                && (p.x - (long) p1.x)*(p2.x - p1.x)
                < (p2.x - (long) p1.x)*(p.x - p1.x))
            {
                inside = !inside;
            }

            oldPoint = newPoint;
        }

        return inside;
    }
}

 
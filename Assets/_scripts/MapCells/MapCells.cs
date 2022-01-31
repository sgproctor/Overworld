using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;
using System.Linq;

public class MapEdge
{
    public Vector2 pointA;
    public Vector2 pointB;
}


public class MapCells : MonoBehaviour{
    // --------------public variables--------------
    public int maxNumberOfNeightbors;
    public bool isCoastCell;
    public float precipitation = 0;
    public float height = 0;

    public River river = null;
    public Texture2D heightTexture = null;
    public Texture2D waterTexture = null;
    public Vector2 coord = new Vector2();

    public List<MapEdge> edges = new List<MapEdge>();
    public List<MapCells> neightbors;   
    public List<Vector3> vertices;
    public PolygonCollider2D mapCollider;

    // --------------serialized variables--------------   
    
    [SerializeField] float sigmoidS = 0.5f;
    [SerializeField] float sigmoidP = 0.5f;

    // --------------private variables--------------
    private Color[] col;
    private MeshRenderer meshRenderer;
    
    public void generateOriginalMesh(Vector3[] vertices, int[] triangles, Vector2[] uvs, Vector3 midpoint)
    {
        heightTexture = GenerateHeightTexture();

        Mesh mapMesh = new Mesh();
        mapMesh.vertices = vertices;
        mapMesh.triangles = triangles;
        mapMesh.uv = uvs;
        mapMesh.RecalculateNormals();
        
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        setHeightTexture();
        meshRenderer.material.shader = Shader.Find("Unlit/Texture");
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mapMesh;
        
        mapCollider = gameObject.AddComponent<PolygonCollider2D>();
        mapCollider.points = mapMesh.vertices.Take(mapMesh.vertices.Count() -1 ).ToArray().toVector2Array();
    }

    public void generateWaterTexture()
    {
        waterTexture = GenerateWaterTexture();
    }

    Texture2D GenerateHeightTexture()
    {
        Texture2D texture = new Texture2D((int)MapGenerator.Instance.generationSize, (int)MapGenerator.Instance.generationSize);
        
        Color color = MapGenerator.Instance.HeightColorGradient.Evaluate(height);
        var fillColorArray =  texture.GetPixels();

        for(var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = color;
        }
        texture.SetPixels(fillColorArray);
        texture.Apply();
        return texture;
    }

    Texture2D GenerateWaterTexture()
    {
        Texture2D texture = new Texture2D((int)MapGenerator.Instance.generationSize, (int)MapGenerator.Instance.generationSize);

        //sigmoid graph found here https://www.desmos.com/calculator/3zhzwbfrxd
        float sigmoidMoistureLevel = SigmoidFunction(precipitation/MapGenerator.Instance.weatherManager.maxWaterLevel, sigmoidS, sigmoidP);
        Debug.Log(string.Format("Level {0}, output {1}", precipitation, sigmoidMoistureLevel));

        Color color = MapGenerator.Instance.WaterColorGradient.Evaluate(sigmoidMoistureLevel);
        var fillColorArray =  texture.GetPixels();
        for(var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = color;
        }
        texture.SetPixels(fillColorArray);
        texture.Apply();
        return texture;
    }

    public void setHeightTexture()
    {
        meshRenderer.material.mainTexture = heightTexture;
    }

    public void setWaterTexture()
    {
        meshRenderer.material.mainTexture = waterTexture;
    }

    public Vector2 getVectorToOcean()
    {
        if(!isCoastCell)
        {
            Debug.Log(String.Format("{} is not a costal cell", name));
            return Vector2.zero;
        }    
        else
        {
            Vector2 totalDiff = Vector2.zero;
            if(neightbors.Count > 0)
            {
                foreach(MapCells cell in neightbors)
                {
                    totalDiff += coord - cell.coord;
                }
                //totalDiff += coord;
                return totalDiff.normalized;
            }
            else
            {
                Debug.Log(String.Format("{} has no neighbors", name));
                return totalDiff;
            }
        }
    }

    public MapEdge getSharedEdge(MapCells neighborCell)
    {
        MapEdge touchingEdge = new MapEdge();
        foreach(MapEdge edge in edges)
        {
            MapEdge egde = new MapEdge();
            egde.pointA = edge.pointB;
            egde.pointB = edge.pointA;
            foreach(MapEdge neighborEdge in neighborCell.edges)
            {
                if((neighborEdge.pointA == edge.pointA) && (neighborEdge.pointB == edge.pointB))
                {
                    touchingEdge = edge;
                }
                else if((neighborEdge.pointA == egde.pointA) && (neighborEdge.pointB == egde.pointB))
                {
                    touchingEdge = egde;
                }
            }            
        }        
        return touchingEdge;
    }

    public bool isCellLowest()
    {
        var celllowest = true;
        foreach(MapCells neighbor in neightbors)
        {
            if(height > neighbor.height)
            {
                celllowest = false;
            }
        }
        return celllowest;
    }

    public static float SigmoidFunction(float x, float s, float p)
    {
        float c = (2 / ( 1 - s )) - 1;
        float f;
        if (x <= p)
        {
            f = (Mathf.Pow(x,c))/(Mathf.Pow(p,c-1));
        }
        else{
            f = 1 - (Mathf.Pow(1-x,c))/(Mathf.Pow(1-p,c-1));
        }
        return f;
    }

    public static float Noisefunction(float x, float y, Vector2 Origin)
    {
        float a = 0, noisesize = MapGenerator.Instance.NoiseScale, opacity = MapGenerator.Instance.startOpacity;

        for (int octaves = 0; octaves < MapGenerator.Instance.NoiseOctaves; octaves++)
        {
            float xVal = (x  / (noisesize * MapGenerator.Instance.generationSize)) + Origin.x;
            float yVal = (y / (noisesize * MapGenerator.Instance.generationSize)) - Origin.y;
            float z = noise.snoise(new float2(xVal, yVal));
            a += Mathf.InverseLerp(0, 1, z) / opacity;

            noisesize /= 2f;
            opacity *= 2f;
        }
        a -= FallOffMap(x, y, (int)MapGenerator.Instance.generationSize, MapGenerator.Instance.IslandSize);
        return a;
    }

    private static float FallOffMap(float x, float y, int size, float islandSize)
    {
        float gradient = 1;
        gradient /= (x * y) / (size * size) * (1 - (x / size)) * (1 - (y / size));
        
        gradient -= 16;
        gradient /= islandSize;


        return gradient;
    }

}

 
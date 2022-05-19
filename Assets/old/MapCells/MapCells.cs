// using UnityEngine;
// using System.Collections.Generic;
// using Unity.Mathematics;
// using System;
// using System.Linq;

// public class MapEdge
// {
//     public Vector2 pointA;
//     public Vector2 pointB;

//     public MapEdge(Vector2 pointA, Vector2 pointB)
//     {
//         this.pointA = pointA;
//         this.pointB = pointB;
//     }

//     public MapEdge(){}
// }


// public class MapCells : MonoBehaviour , IQuadTreeObject{
//     // --------------public variables--------------
//     public int maxNumberOfNeightbors;
//     public bool isCoastCell;
//     public float precipitation = 0;
//     public float height = 0;

//     public old.River river = null;
//     public Texture2D heightTexture = null;
//     public Texture2D waterTexture = null;
//     public Vector2 coord = new Vector2();

//     public List<MapEdge> edges = new List<MapEdge>();
//     public List<MapCells> neightbors;   
//     public List<Vector3> vertices;
//     public PolygonCollider2D mapCollider;

//     // --------------serialized variables--------------   
    
//     [SerializeField] float sigmoidS = 0.5f;
//     [SerializeField] float sigmoidP = 0.5f;

//     // --------------private variables--------------
//     private Color[] col;
//     private MeshRenderer meshRenderer;
    
//     public Vector2 GetPosition()
//     {
//         return Vector2.zero;
//     }
//     public void generateOriginalMesh(Vector3[] vertices, int[] triangles, Vector2[] uvs, Vector3 midpoint)
//     {
//         heightTexture = GenerateHeightTexture();

//         Mesh mapMesh = new Mesh();
//         mapMesh.vertices = vertices;
//         mapMesh.triangles = triangles;
//         mapMesh.uv = uvs;
//         mapMesh.RecalculateNormals();
        
//         meshRenderer = gameObject.AddComponent<MeshRenderer>();
//         setHeightTexture();
//         meshRenderer.material.shader = Shader.Find("Unlit/Texture");
//         var meshFilter = gameObject.AddComponent<MeshFilter>();
//         meshFilter.mesh = mapMesh;
        
//         mapCollider = gameObject.AddComponent<PolygonCollider2D>();
//         mapCollider.points = mapMesh.vertices.Take(mapMesh.vertices.Count() -1 ).ToArray().toVector2Array();
//     }

//     public void generateWaterTexture()
//     {
//         waterTexture = GenerateWaterTexture();
//     }

//     Texture2D GenerateHeightTexture()
//     {
//         Texture2D texture = new Texture2D((int)MapManager.Instance.mapSize.x, (int)MapManager.Instance.mapSize.y);
        
//         Color color = MapManager.Instance.HeightColorGradient.Evaluate(height);
//         var fillColorArray =  texture.GetPixels();

//         for(var i = 0; i < fillColorArray.Length; ++i)
//         {
//             fillColorArray[i] = color;
//         }
//         texture.SetPixels(fillColorArray);
//         texture.Apply();
//         return texture;
//     }

//     Texture2D GenerateWaterTexture()
//     {
//         Texture2D texture = new Texture2D((int)MapManager.Instance.mapSize.x, (int)MapManager.Instance.mapSize.y);

//         //sigmoid graph found here https://www.desmos.com/calculator/3zhzwbfrxd
//         float sigmoidMoistureLevel = SigmoidFunction(precipitation/WeatherManager.Instance.maxWaterLevel, sigmoidS, sigmoidP);

//         Color color = MapManager.Instance.WaterColorGradient.Evaluate(sigmoidMoistureLevel);
//         var fillColorArray =  texture.GetPixels();
//         for(var i = 0; i < fillColorArray.Length; ++i)
//         {
//             fillColorArray[i] = color;
//         }
//         texture.SetPixels(fillColorArray);
//         texture.Apply();
//         return texture;
//     }

//     public void setHeightTexture()
//     {
//         meshRenderer.material.mainTexture = heightTexture;
//     }

//     public void setWaterTexture()
//     {
//         meshRenderer.material.mainTexture = waterTexture;
//     }

//     public Vector2 getVectorToOcean()
//     {
//         if(!isCoastCell)
//         {
//             Debug.Log(String.Format("{} is not a costal cell", name));
//             return Vector2.zero;
//         }    
//         else
//         {
//             Vector2 totalDiff = Vector2.zero;
//             if(neightbors.Count > 0)
//             {
//                 foreach(MapCells cell in neightbors)
//                 {
//                     totalDiff += coord - cell.coord;
//                 }
//                 //totalDiff += coord;
//                 return totalDiff.normalized;
//             }
//             else
//             {
//                 Debug.Log(String.Format("{} has no neighbors", name));
//                 return totalDiff;
//             }
//         }
//     }

//     public MapEdge getSharedEdge(MapCells neighborCell)
//     {
//         MapEdge touchingEdge = new MapEdge();
//         foreach(MapEdge edge in edges)
//         {
//             MapEdge egde = new MapEdge();
//             egde.pointA = edge.pointB;
//             egde.pointB = edge.pointA;
//             foreach(MapEdge neighborEdge in neighborCell.edges)
//             {
//                 if((neighborEdge.pointA == edge.pointA) && (neighborEdge.pointB == edge.pointB))
//                 {
//                     touchingEdge = edge;
//                 }
//                 else if((neighborEdge.pointA == egde.pointA) && (neighborEdge.pointB == egde.pointB))
//                 {
//                     touchingEdge = egde;
//                 }
//             }            
//         }        
//         return touchingEdge;
//     }

//     public bool isCellLowest()
//     {
//         var celllowest = true;
//         foreach(MapCells neighbor in neightbors)
//         {
//             if(height > neighbor.height)
//             {
//                 celllowest = false;
//             }
//         }
//         return celllowest;
//     }

//     public static float SigmoidFunction(float x, float s, float p)
//     {
//         float c = (2 / ( 1 - s )) - 1;
//         float f;
//         if (x <= p)
//         {
//             f = (Mathf.Pow(x,c))/(Mathf.Pow(p,c-1));
//         }
//         else{
//             f = 1 - (Mathf.Pow(1-x,c))/(Mathf.Pow(1-p,c-1));
//         }
//         return f;
//     }

//     public static float Noisefunction(float x, float y, float xOffset, float yOffset, float xSize, float ySize, Vector2 Origin)
//     {
//         float a = 0, noisesize = MapManager.Instance.NoiseScale, opacity = MapManager.Instance.startOpacity;

//         for (int octaves = 0; octaves < MapManager.Instance.NoiseOctaves; octaves++)
//         {
//             float xVal = ((x) / (noisesize * xSize)) + Origin.x;
//             float yVal = ((y) / (noisesize * ySize)) - Origin.y;
//             float z = noise.snoise(new float2(xVal, yVal));
//             a += Mathf.InverseLerp(0, 1, z) / opacity;

//             noisesize /= 2f;
//             opacity *= 2f;
//         }
//         a -= FallOffMap(x, y, xOffset, yOffset, xSize * 0.5f, ySize * 0.5f, MapManager.Instance.IslandSize);
//         return a;
//     }

//     private static float FallOffMap(float x, float y, float xOffset, float yOffset, float sizeX, float sizeY, float islandSize)
//     {
//         float gradient = 1;
//         gradient /= ((x - xOffset/2f) * (y - yOffset/2f)) / (sizeX * sizeY) * (1 - ((x - xOffset/2f)/ sizeX)) * (1 - ((y - yOffset/2f) / sizeY));
        
//         gradient -= 16;
//         gradient /= islandSize;


//         return gradient;
//     }

// }

 
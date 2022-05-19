
using UnityEngine;
using System;
using System.Linq;
using DelaunatorSharp;
using System.Collections.Generic;
using DelaunatorSharp.Unity.Extensions;

[RequireComponent (typeof (MeshFilter), typeof(MeshRenderer))]
public class MapDisplayManager : MonoBehaviour
{
    public static MapDisplayManager Instance;
    public enum DisplayEnum { height, biome, temperature, precipitation, city}
    public DisplayEnum displayEnum;
    public Gradient heightGradient;
    public Gradient tempGradient;
    public Gradient precipitationGradient;
    public Gradient cityGradient;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public Material meshMaterial;
    //private VoronoiGenerator vgen = VoronoiGenerator.Instance;
    private IColorable meshColorer;
    public float maxCityScore;

    private void Awake()
    {
        // If Instance is not null (any time after the first time)
        // AND
        // If Instance is not 'this' (after the first time)
        if (Instance != null && Instance != this)
        {
            // ...then destroy the game object this script component is attached to.
            Destroy(gameObject);
        }
        else
        {
            // Tell Unity not to destory the GameObject this
            //  is attached to between scenes.
            //DontDestroyOnLoad(gameObject);
            // Save an internal reference to the first instance of this class
            Instance = this;
        }
    }
    public void GenerateMeshcolorers()
    {
        meshColorer = new HeightMapColor(heightGradient: heightGradient);
    }
	public void UpdateDisplay(){
		switch (displayEnum)
		{
			case DisplayEnum.height:
				colorMapVertices();
				break;
			case DisplayEnum.precipitation:
				colorMapPrecipitation();
				break;
            case DisplayEnum.temperature:
				colorMapTemperature();
				break;
            case DisplayEnum.biome:
				colorMapBiome();
				break;
            case DisplayEnum.city:
                colorCityScores();
                break;
		}
    }

    public void colorMapVertices()
    {
        List<Color> colors = new List<Color>();
        foreach(MapCells cell in VoronoiGenerator.Instance.cellMap.Values)
        {
            foreach(Vector3 vertex in cell.vertices)
            {
                colors.Add(meshColorer.colorAt(cell.height));
            }
            colors.Add(meshColorer.colorAt(cell.height));
        }

        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh.colors = colors.ToArray();
    }

    public void colorMapTemperature()
    {
        List<Color> colors = new List<Color>();
        foreach(MapCells cell in VoronoiGenerator.Instance.cellMap.Values)
        {
            foreach(Vector3 vertex in cell.vertices)
            {
                if(cell.height < VoronoiGenerator.Instance.waterLevel) colors.Add(heightGradient.Evaluate(cell.height));
                else colors.Add(tempGradient.Evaluate(cell.temperature/96.827f));
            }
            if(cell.height < VoronoiGenerator.Instance.waterLevel) colors.Add(heightGradient.Evaluate(cell.height));
            else colors.Add(tempGradient.Evaluate(cell.temperature/96.827f));
        }

        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh.colors = colors.ToArray();
    }

    public void colorMapBiome()
    {
        List<Color> colors = new List<Color>();
        foreach(MapCells cell in VoronoiGenerator.Instance.cellMap.Values)
        {
            foreach(Vector3 vertex in cell.vertices)
            {
                if(cell.height < VoronoiGenerator.Instance.waterLevel) colors.Add(heightGradient.Evaluate(cell.height));
                else colors.Add(cell.biome.colour);
            }
            if(cell.height < VoronoiGenerator.Instance.waterLevel) colors.Add(heightGradient.Evaluate(cell.height));
            else colors.Add(cell.biome.colour);
        }

        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh.colors = colors.ToArray();
    }

    public void colorMapPrecipitation()
    {
        List<Color> colors = new List<Color>();
        //Gradient precipGrad = weatherManager.precipGrad;
        foreach(MapCells cell in VoronoiGenerator.Instance.cellMap.Values)
        {
            foreach(Vector3 vertex in cell.vertices)
            {
                if(cell.height < VoronoiGenerator.Instance.waterLevel) colors.Add(heightGradient.Evaluate(cell.height));
                else colors.Add(precipitationGradient.Evaluate(cell.precipitation));
            }
            if(cell.height < VoronoiGenerator.Instance.waterLevel) colors.Add(heightGradient.Evaluate(cell.height));
            else colors.Add(precipitationGradient.Evaluate(cell.precipitation));
        }

        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh.colors = colors.ToArray();
    }

    public void colorCityScores()
    {
        List<Color> colors = new List<Color>();
        foreach(MapCells cell in VoronoiGenerator.Instance.cellMap.Values)
        {
            foreach(Vector3 vertex in cell.vertices)
            {
                if(cell.height < VoronoiGenerator.Instance.waterLevel) colors.Add(heightGradient.Evaluate(cell.height));
                else colors.Add(cityGradient.Evaluate(cell.cityScore/maxCityScore));
            }
            if(cell.height < VoronoiGenerator.Instance.waterLevel) colors.Add(heightGradient.Evaluate(cell.height));
            else colors.Add(cityGradient.Evaluate(cell.cityScore/maxCityScore));
        }

        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh.colors = colors.ToArray();
    }

    public void CreateMapMesh()
    {
        Mesh mapMesh = new Mesh();
        mapMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        foreach(MapCells cell in VoronoiGenerator.Instance.cellMap.Values.ToList())
        {
            vertices.AddRange(cell.vertices);
            vertices.Add(cell.coord);
            triangles.AddRange(cell.triangles);
            for(int i = 0; i <= cell.vertices.Count(); i++)
            {
                uvs.Add(new Vector2(0,1));
            }
        }
        mapMesh.vertices = vertices.ToArray();
        mapMesh.triangles = triangles.ToArray();
        mapMesh.uv = uvs.ToArray();
        mapMesh.RecalculateNormals();
        meshRenderer.material = meshMaterial;
        meshFilter.mesh = mapMesh;
    }
}
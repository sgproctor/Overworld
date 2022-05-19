using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class UIController : MonoBehaviour
{
    public Button generateMap;
    public SliderInt cellSize;
    public Button heightMap;
    public Button precipitation;
    public Button biomes;
    public Button temperature;

    public TextField cityCount;
    public Button placeCities;


    // Start is called before the first frame update
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        generateMap = root.Q<Button>("generateMap");
        generateMap.clicked += GenerateMap;

        cellSize = root.Q<SliderInt>("cellSize");

        heightMap = root.Q<Button>("heightMap");
        heightMap.clicked += SetHeightMap;
        precipitation = root.Q<Button>("precipitation");
        precipitation.clicked += SetPrecipitation;
        biomes = root.Q<Button>("biomes");
        biomes.clicked += SetBiomes;
        temperature = root.Q<Button>("temperature");
        temperature.clicked += SetTemp;

        cityCount = root.Q<TextField>("cityCount");
        placeCities = root.Q<Button>("placeCities");
        placeCities.clicked += AddCities;
    }

    void GenerateMap()
    {
        
        VoronoiGenerator.Instance.GenerateMap(cellSize.value);
    }

    void SetHeightMap()
    {
        MapDisplayManager.Instance.colorMapVertices();
    }
    void SetPrecipitation()
    {
        MapDisplayManager.Instance.colorMapPrecipitation();
    }
    void SetBiomes()
    {
        MapDisplayManager.Instance.colorMapBiome();
    }
    void SetTemp()
    {
        MapDisplayManager.Instance.colorMapTemperature();
    }

    void AddCities()
    {
        int numCities = int.Parse(cityCount.value);
        Debug.Log(numCities);
        VoronoiGenerator.Instance.PlaceCities(numCities);
    }

}

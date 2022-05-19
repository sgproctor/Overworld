using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Settlement
{
    int id;
    float settlementScore;
    public Settlement(int id, float score)
    {
        this.id = id;
        this.settlementScore = score;
    }
}

public class Cities : MonoBehaviour
{
    public float fluxMod;
    public float tempMod;
    public float precipMod;

    public GameObject citySprite;

    public int idCount = 0;

    public List<Vector2> cityMap = new List<Vector2>();
    public List<float> maxDistances = new List<float>();

    // void Start()
    // {
    //     cityMap = new List<Vector2>();
    //     maxDistances = new List<float>();
    // }
    public void DeleteChildren()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    public float SetCityScores(Dictionary<Vector3, MapCells> cellMap, float waterLevel)
    {
        float maxCityScore = 0;
        List<MapCells> cells = cellMap.Values.ToList();
        cells = cells.Where( cell => cell.height > waterLevel).ToList();
        foreach(MapCells cell in cells)
        {
            float fluxScore = 0.005f*(Mathf.Pow(cell.flux,1f/1.6f));
            float tempScore = (55f - Mathf.Abs(55f - cell.temperature)) / 55f;
            float precipitationScore = 1f - Mathf.Abs(0.75f - cell.precipitation);
            float score = fluxScore * fluxMod + tempScore * tempMod + precipitationScore * precipMod;
            cell.cityScore = score;
            if(score > maxCityScore) maxCityScore = score;
        }
        return maxCityScore;
    }

    public void AddCity(Dictionary<Vector3, MapCells> cellMap)
    {
        List<MapCells> cells = cellMap.Values.ToList();
        cells = cells.Where( cell =>  cell.cityScore > 0 && !cityMap.Contains(cell.coord)).ToList();
        MapCells cityCell = cells.First();
        float maxScore = -1f * Mathf.Infinity;      
        foreach(MapCells cell in cells)
        {
            float score = 0f;
            if(cityMap.Count > 0)
            {
                //float cityDistance = 0f;
                float minDistance = Mathf.Infinity;
                for (int i = 0; i < cityMap.Count; i++)
                {
                    // float xDistance = cell.coord.x + cityMap[i].x;
                    // float yDistance = cell.coord.y + cityMap[i].y;
                    // float maxDistance = maxDistances[i];
                    //float cityDistance = (xDistance*xDistance+yDistance*yDistance)/maxDistance;
                    float cityDistance = Vector2.Distance(cell.coord, cityMap[i])/maxDistances[i];
                    if(cityDistance < minDistance) minDistance = cityDistance;
                }
                // foreach(Vector2 point in cityMap)
                // {
                //     float xDistance = cell.coord.x + point.x;
                //     float yDistance = cell.coord.y + point.y;
                //     float maxDistance = GetMaxCityDistance(point);
                //     float cityDistance = (xDistance*xDistance+yDistance*yDistance)/maxDistance;//GetMaxCityDistance(point);
                //     //if(cityDistance < minDistance) minDistance = cityDistance;
                // }
                //cityDistance /= (float)cityMap.Count;
                score = cell.cityScore * minDistance;
            }
            else
            {
                score = cell.cityScore;
            }
            
            if(score > maxScore)
            {
                cityCell = cell;
                maxScore = score;
            } 
        }

        //cells = cells.OrderByDescending(cell => cell.cityScore).ToList();
        //var cityCell = cells.First();
        cityMap.Add(cityCell.coord);
        maxDistances.Add(GetMaxCityDistance(cityCell.coord));
        
        MapCells cityCell = VoronoiGenerator.Instance.GetCellAtPoint(cityCell.coord);
        cityCell.city = new Settlement(cityMap.Count, maxScore);

        GameObject city = Instantiate(citySprite, new Vector3(cityCell.coord.x, cityCell.coord.y, -10), Quaternion.identity, gameObject.transform);
        float scaleMod = Mathf.Clamp(maxScore, 0.6f, 2f);// Mathf.Max(0.5f, maxScore);
        city.transform.localScale = new Vector3(city.transform.localScale.x * scaleMod, city.transform.localScale.y * scaleMod, 1);

        //SetCityDistanceScores(cells, cityCell.coord, cityCell.cityScore);
    }

    public void SetCityDistanceScores(List<MapCells> cells, Vector2 cityPoint, float maxCityScore)
    {
        float maxCityDistance = GetMaxCityDistance(cityPoint);
        float maxScore = 0;
        foreach(MapCells cell in cells)
        {
            float score = cell.cityScore + Vector2.Distance(cell.coord,cityPoint)/maxCityDistance;
            cell.cityScore = score;
            if(score > maxScore) maxScore = score;
        }
        MapDisplayManager.Instance.maxCityScore = maxScore;
    }

    private float GetMaxCityDistance(Vector2 coord)
    {
        float mapWidth = VoronoiGenerator.Instance.width;
        float mapHeight = VoronoiGenerator.Instance.height;
        float yDistance = 0;
        float xDistance = 0;
        if(coord.y > mapHeight /2f)
        {
            yDistance = coord.y;
        }
        else
        {
            yDistance = mapHeight - coord.y;
        }

        if(coord.x > mapWidth /2f)
        {
            xDistance = coord.x;
        }
        else
        {
            xDistance = mapWidth - coord.x;
        }
        // float yDistance = Mathf.Max(coord.y, mapHeight - coord.y);
        // float xDistance = Mathf.Max(coord.x, mapWidth - coord.x);
        return Mathf.Sqrt(xDistance*xDistance+yDistance*yDistance);
    }
}
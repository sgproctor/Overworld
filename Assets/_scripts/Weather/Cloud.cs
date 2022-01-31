using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;


public class Cloud : MonoBehaviour{
    public Vector3 windDirection;
    public float mapSize = 20;
    public float windSpeeds = 10;
    public float rainAmount = 1000;
    public float rainRate = 1f;
    public float cloudRadius = 1.0f;
    public float mountainRate = 10f;
    public float mountainWindScale = 4f;
    public float dumpRainAtHeight = 0.5f;
    ContactFilter2D emptyFilter;
    public List<Collider2D> collidersInSphere;
    public List<MapCells> mapCells;
    private WeatherManager weatherManager;
    //public List<GameObject> objectList = new List<GameObject>();
    
    private void Start()
    {
        cloudRadius = GetRandomValue(cloudRadius, 1.5f);
        windSpeeds = GetRandomValue(windSpeeds, 1.5f);
        rainAmount = GetRandomValue(rainAmount, 1.5f);
        dumpRainAtHeight = GetRandomValue(dumpRainAtHeight, 1.5f);


        weatherManager = transform.parent.gameObject.GetComponent<WeatherManager>();
        emptyFilter = new ContactFilter2D();
        emptyFilter.NoFilter();
        transform.localScale = new Vector2(cloudRadius*2.0f,cloudRadius*2.0f);       
    }

    private void Update() 
    {
        checkDestroy();
        moveStorm();
        Physics2D.OverlapCircle(transform.position,cloudRadius,emptyFilter, collidersInSphere);
        getCellsFromColliderList();
        rain();
    }

    private float GetRandomValue(float value, float scale)
    {
        return UnityEngine.Random.Range(value / scale, value * scale);
    }

    private void checkDestroy()
    {
        if(rainAmount <= 0f)
        {
            Destroy(gameObject);
        }
        if((Mathf.Abs(transform.position.x) >= mapSize) || (Mathf.Abs(transform.position.y) >= mapSize))
        {
            Destroy(gameObject);
        }
    }

    private void getCellsFromColliderList()
    {
        List<MapCells> temp = new List<MapCells>();
        foreach(Collider2D collider in collidersInSphere)
        {
            temp.Add(collider.gameObject.GetComponent<MapCells>());
        }
        mapCells = temp;
    }

    private void rain()
    {
        float addedHeights = 0.0f;
        float numOfCells = 0.0f;
        foreach(MapCells cell in mapCells)
        {
            numOfCells ++;
            addedHeights += cell.height;
            if(rainAmount > 0f)
            {
                cell.precipitation += rainRate;
                weatherManager.maxWaterLevel = Mathf.Max(cell.precipitation,weatherManager.maxWaterLevel);
                rainAmount -= rainRate;
            }
        }
        float averageHeight = addedHeights / numOfCells;
        if(averageHeight >= dumpRainAtHeight)
        {
            // the taller the terrain the more the rain falls and slower the clouds are
            rainRate = mountainRate * (1+2*(averageHeight - dumpRainAtHeight));
            windSpeeds = windSpeeds * 0.9f;
        }
    }

    private void moveStorm()
    {
        float meanderAngle = UnityEngine.Random.Range(-2.5f,2.5f);
        Vector3 meanderDir = Quaternion.AngleAxis(-meanderAngle, Vector3.forward) * windDirection;
        transform.position += meanderDir * windSpeeds * Time.deltaTime;
    }
}

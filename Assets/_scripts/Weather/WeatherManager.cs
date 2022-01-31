using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Mathematics;

public class WeatherManager : MonoBehaviour
{
    public float maxWaterLevel;
    public GameObject CloudPrefab;
    public int numOfStorms;
    public enum StormOrigin{
        North,
        South,
        East,
        West
    };
    [SerializeField]
    private StormOrigin stormOrigin = StormOrigin.North;
    public float mapSize = 20f;
    public int directionBias = 4;
    public StormOrigin[] randList;

    private void Start() 
    {
        randList = new StormOrigin[4+directionBias];
        randList[0] = StormOrigin.North;
        randList[1] = StormOrigin.South;
        randList[2] = StormOrigin.East;
        randList[3] = StormOrigin.West;
        for(int i = 0; i<directionBias; i++)
        {
            randList[i+4] = stormOrigin;
        }

        GenerateStorms();
    }

    private void GenerateStorms()
    {
        while(numOfStorms >= 0)
        {
            (var origin, var direction) = GetOriginPoint();
            GameObject cloudObject = Instantiate(CloudPrefab, origin, new quaternion(0,0,0,0), gameObject.transform);
            cloudObject.GetComponent<Cloud>().windDirection = direction;
            //cloudObject.GetComponent<Cloud>().mapSize = mapSize;
            numOfStorms--;
        }
        //MapGenerator.Instance.GenerateRivers();
    }

    private (Vector3, Vector3) GetOriginPoint()
    {
        Vector3 origin = new Vector3();
        Vector3 direction = new Vector3();

        // center of map is not 0,0 isntead use half sizes
        float midpoint = mapSize/2f;

        
        StormOrigin randStormOrigin = randList[UnityEngine.Random.Range(0,randList.Length)];
        

        if(randStormOrigin == StormOrigin.North)
        {
            origin = new Vector3(UnityEngine.Random.Range(0f, 1f) * mapSize, mapSize+midpoint, 0);
            direction = new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), -1, 0);
            //direction = new Vector3(0, -1, 0);
        }
        else if(randStormOrigin == StormOrigin.South)
        {
            origin = new Vector3(UnityEngine.Random.Range(0f, 1f) * mapSize, -mapSize, 0);
            direction = new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), 1, 0);
            //direction = new Vector3(0, 1, 0);
        }
        else if(randStormOrigin == StormOrigin.West)
        {
            origin = new Vector3(-mapSize, UnityEngine.Random.Range(0f, 1f) * mapSize, 0);
            direction = new Vector3(1, UnityEngine.Random.Range(-0.1f, 0.1f), 0);
            //direction = new Vector3(1, 0, 0);
        }
        else
        {
            origin = new Vector3(mapSize+midpoint, UnityEngine.Random.Range(0f, 1f) * mapSize, 0);
            direction = new Vector3(-1, UnityEngine.Random.Range(-0.1f, 0.1f), 0);
            //direction = new Vector3(-1, 0, 0);
        }
        return (origin, direction);
    }
}
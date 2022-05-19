// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using DelaunatorSharp;
// using DelaunatorSharp.Unity.Extensions;
// using System.Linq;
// using Unity.Mathematics;

// public class MapManager : MonoBehaviour
// {

//     public static MapManager Instance;

//     public float oceanHeight = 0.2f;

//     // Color Variables
//     public Gradient HeightColorGradient;
//     public Gradient WaterColorGradient;

//     // containers
//     public Transform CellsContainer;
//     public Transform RiversContainer;
//     public Transform PointsContainer;

//     public bool debugMap;
//     public Vector2 mapSize;
//     public float generationMinDistance = .2f;
//     public float NoiseScale, IslandSize;
//     public float startOpacity = 1.3f;

//     public List<MapCells> mapCells;

//     [Range(1, 20)] public int NoiseOctaves;
//     [Range(0, 99999999)] public int Seed;

//     [SerializeField] old.River riverPrefab;
//     [SerializeField] MapGenerator mapGeneratorPrefab;
//     [SerializeField] bool renderMap;


//     // state vars        
//     [SerializeField] SpriteRenderer OceanBackground;
//     [SerializeField] bool setWaterTexture;
//     [SerializeField] bool setGroundTexture;
//     [SerializeField] bool startRivers;
    
//     //misc
//     [SerializeField] 

//     // manager state
//     private bool waterSet = false;
//     private bool groundSet = false;
//     private bool riverSet = false;
//     private float depressIncrease = 0.01f;

//     // latitude vars
//     public (Vector2,Vector2) equatorLine; // 0 degrees
//     public (Vector2,Vector2) TropicLine; //30 degrees
//     public (Vector2,Vector2) ArticLine; //60 degrees

//     private void Awake()
//     {
//         // If Instance is not null (any time after the first time)
//         // AND
//         // If Instance is not 'this' (after the first time)
//         if (Instance != null && Instance != this)
//         {
//             // ...then destroy the game object this script component is attached to.
//             Destroy(gameObject);
//         }
//         else
//         {
//             // Tell Unity not to destory the GameObject this
//             //  is attached to between scenes.
//             DontDestroyOnLoad(gameObject);
//             // Save an internal reference to the first instance of this class
//             Instance = this;
//         }
//     }
//     // Start is called before the first frame update
//     void Start()
//     {
//         CreateLatitudeLines();
//         // set up ocean backdrop
//         if(renderMap)
//         {
//             SpriteRenderer oceanBackground =  Instantiate(OceanBackground);
//             oceanBackground.transform.position = new Vector3(mapSize.x / 2f, mapSize.y / 2f, 0.1f);
//             oceanBackground.transform.localScale = new Vector3(mapSize.x, mapSize.y, 1);
//             oceanBackground.transform.parent = gameObject.transform;
//         }
//         CreateNewContainers();

//         // create delauny object and voronoi cells
//         float debugTime = Time.realtimeSinceStartup;
//         mapCells = new List<MapCells>();

//         MapGenerator continent1 = Instantiate(mapGeneratorPrefab, Vector3.zero, new quaternion(0,0,0,0));
//         continent1.transform.parent = gameObject.transform;
//         continent1.xSize = 8f;
//         continent1.ySize = 8f;
//         continent1.xOffset = 1f;
//         continent1.yOffset = 1f;

//         MapGenerator continent2 = Instantiate(mapGeneratorPrefab, Vector3.zero, new quaternion(0,0,0,0));
//         continent2.transform.parent = gameObject.transform;
//         continent2.xSize = 5f;
//         continent2.ySize = 3f;
//         continent2.xOffset = 0f;
//         continent2.yOffset = 0f;
        
//         //MapGenerator mapGenerator = new MapGenerator(mapSize.x, mapSize.y, 0, 0);
        
//         continent1.SetupDelauny();
//         //continent2.SetupDelauny();
//         if(debugMap) 
//         {
//             Debug.Log(String.Format("Time to generate Delauny Graph {0}", Time.realtimeSinceStartup - debugTime));
//         }

//         debugTime = Time.realtimeSinceStartup;
//         //CreateVoronoi(delaunyGraph1);
//         continent1.CreateVoronoi();
//         //continent2.CreateVoronoi();
//         if(debugMap) 
//         {
//             Debug.Log(String.Format("Time to generate voronoi cells {0}", Time.realtimeSinceStartup - debugTime));
//         }

//         //Debug.Log(String.Format("{0} -- does it contain", mapCells[0].mapCollider.bounds.Contains(new Vector3(mapCells[0].coord.x - 0.0001f, mapCells[0].coord.y + 0.0001f, 0f))));

//         debugTime = Time.realtimeSinceStartup;
//         continent1.SetVoronoiNeighborCells();
//         //continent2.SetVoronoiNeighborCells();
//         if(debugMap) 
//         {
//             Debug.Log(String.Format("Time to set neighbor cells {0}", Time.realtimeSinceStartup - debugTime));
//         }

//         debugTime = Time.realtimeSinceStartup;
//         continent1.SetCoastalCells();
//         //continent2.SetCoastalCells();
//         if(debugMap) 
//         {
//             Debug.Log(String.Format("Time to set coastal cells {0}", Time.realtimeSinceStartup - debugTime));
//         }

//         mapCells.AddRange(continent1.mapCells);
//         //mapCells.AddRange(continent2.mapCells);
        
//     }

//     private void Update() {
//         // TODO -- Convert to states
//         if(setWaterTexture && !waterSet)
//         {
//             groundSet = false;
//             waterSet = true;
//             foreach(MapCells cell in mapCells)
//             {
//                 if(cell.waterTexture == null)
//                 {
//                     cell.generateWaterTexture();
//                 }
//                 cell.setWaterTexture();
//             }
//         }
//         if(setGroundTexture && !groundSet)
//         {
//             waterSet = false;
//             groundSet = true;
//             foreach(MapCells cell in mapCells)
//             {
//                 cell.setHeightTexture();
//             }
//         }
//         if(startRivers && !riverSet)
//         {
//             riverSet = true;
//             GenerateRivers();
//         }
//     }

//     private void Clear()
//     {
//         foreach (Transform child in CellsContainer.transform)
//         {
//             Destroy(child.gameObject);
//         }
//         //delaunator = null;
//     }

//     private void CreateNewContainers()
//     {
//         CreateCellsContainer();
//         CreateRiversContainer();
//     }

//     private void CreateCellsContainer()
//     {
//         if (CellsContainer != null)
//         {
//             Destroy(CellsContainer.gameObject);
//         }

//         CellsContainer = new GameObject(nameof(CellsContainer)).transform;
//     }

//     private void CreateRiversContainer()
//     {
//         if (RiversContainer != null)
//         {
//             Destroy(RiversContainer.gameObject);
//         }

//         RiversContainer = new GameObject(nameof(RiversContainer)).transform;
//     }

//     public void GenerateRivers()
//     {
//         List<MapCells> sortedCellsByHeight = mapCells.OrderBy(e => e.height).ToList();
//         depressionFill(sortedCellsByHeight);
//         for(int i = sortedCellsByHeight.Count-1; i >=0 ; i--)
//         {
//             MapCells cell = sortedCellsByHeight[i];
//             if (!cell.isCoastCell)
//             {                
//                 if (cell.river==null)
//                 {
//                     old.River cellRiver = Instantiate(riverPrefab);
//                     cellRiver.riverID = sortedCellsByHeight.Count - i;
//                     cellRiver.name = String.Format("River-{0}", cellRiver.riverID);
//                     cellRiver.createRiverSegment(cell);
//                     cellRiver.transform.parent = RiversContainer.transform;
//                 }
//             }
//         }
//         foreach(MapCells cell in mapCells)
//         {
//             if(cell.waterTexture == null)
//             {
//                 cell.generateWaterTexture();
//             }
//         }
//     }

//     private void depressionFill(List<MapCells> sortedCells)
//     {
//         float depressStartTime = Time.realtimeSinceStartup;

//         // keep depressing cells until loop goes through without increasing height
//         bool keepDepressing = true;
//         while(keepDepressing)
//         {
//             keepDepressing = false;
//             foreach(MapCells cell in sortedCells)
//             {
//                 if (!cell.isCoastCell)
//                 {
//                     bool keepDepressingCell = true;
//                     while(keepDepressingCell)
//                     {
//                         if(cell.isCellLowest())
//                         {
//                             cell.height += depressIncrease;
//                             keepDepressing = true;
//                         }
//                         else
//                         {
//                             keepDepressingCell = false;
//                         }
//                     }
//                 }
//             }
//         }
//         if(debugMap) 
//         {
//             Debug.Log(String.Format("Time to apply depression alogrithm {0}", Time.realtimeSinceStartup - depressStartTime));
//         }
//     }

//     private void CreateLatitudeLines()
//     {
//         // equator
//         var equatorLineWest = new Vector2(0f,mapSize.y / 2f);
//         var equatorLineEast = new Vector2(mapSize.x,mapSize.y / 2f);
//         equatorLine = (equatorLineWest, equatorLineEast);

//         var lineGameObject = new GameObject("Equator Line");
//         var lineRenderer = lineGameObject.AddComponent<LineRenderer>();
//         lineRenderer.SetPosition(0, new Vector3(equatorLine.Item1.x, equatorLine.Item1.y, -0.01f));
//         lineRenderer.SetPosition(1, new Vector3(equatorLine.Item2.x, equatorLine.Item2.y, -0.01f));
//         lineRenderer.material = new Material(Shader.Find("Standard"));
//         lineRenderer.startColor = new Color(1,1,1);
//         lineRenderer.endColor = new Color(1,1,1);
//         lineRenderer.startWidth = 0.01f;
//         lineRenderer.endWidth = 0.01f;

//         // tropic
//         var northTropLineWest = new Vector2(0f,mapSize.y / 2f + mapSize.y / 6f);
//         var northTropLineEast = new Vector2(mapSize.x,mapSize.y / 2f + mapSize.y / 6f);
//         TropicLine = (northTropLineWest, northTropLineEast);

//         var lineGameObject2 = new GameObject("North Tropic Line");
//         var lineRenderer2 = lineGameObject2.AddComponent<LineRenderer>();
//         lineRenderer2.SetPosition(0, new Vector3(TropicLine.Item1.x, TropicLine.Item1.y, -0.01f));
//         lineRenderer2.SetPosition(1, new Vector3(TropicLine.Item2.x, TropicLine.Item2.y, -0.01f));
//         lineRenderer2.material = new Material(Shader.Find("Standard"));
//         lineRenderer2.startColor = new Color(1,1,1);
//         lineRenderer2.endColor = new Color(1,1,1);
//         lineRenderer2.startWidth = 0.01f;
//         lineRenderer2.endWidth = 0.01f;
//     }
// }

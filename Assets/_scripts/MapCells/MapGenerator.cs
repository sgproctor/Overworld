using System;
using UnityEngine;
using System.Linq;
using DelaunatorSharp;
using Unity.Mathematics;
using System.Collections.Generic;
using DelaunatorSharp.Unity.Extensions;


public class MapGenerator : MonoBehaviour
{
    
    // --------------public variables--------------
    public static MapGenerator Instance;
    // Noise Varibles
    public float generationSize = 3;
    public float generationMinDistance = .2f;
    public float NoiseScale, IslandSize;
    public float startOpacity = 1.3f;
    public float oceanHeight = 0.2f;
    [Range(1, 20)] public int NoiseOctaves;
    [Range(0, 99999999)] public int Seed;

    // Color Variables
    public Gradient HeightColorGradient;
    public Gradient WaterColorGradient;
    public WeatherManager weatherManager;

    // --------------serialized variables--------------
    [SerializeField] MapCells mapCellsPrefab;
    [SerializeField] River riverPrefab;
    [SerializeField] bool renderMap;

    // state vars        
    [SerializeField] SpriteRenderer OceanBackground;
    [SerializeField] bool setWaterTexture;
    [SerializeField] bool setGroundTexture;
    [SerializeField] bool startRivers;

    //misc
    [SerializeField] bool debugMap;


    // --------------private variables--------------
    // delauntator vars
    private Delaunator delaunator;
    private List<IPoint> points = new List<IPoint>();
    private List<MapCells> mapCells = new List<MapCells>();
    private List<Vector2> cellPoints = new List<Vector2>();
    private QuadTree cellQuadTree = new QuadTree();

    // containers
    private Transform CellsContainer;
    private Transform RiversContainer;
    private Transform PointsContainer;

    // manager state
    private bool waterSet = false;
    private bool groundSet = false;
    private bool riverSet = false;
    private float depressIncrease = 0.01f;

    // texture vars
    private Color[] col;
    private Texture2D tex;
    private Vector2 originPoint = new Vector2();

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
            DontDestroyOnLoad(gameObject);
            // Save an internal reference to the first instance of this class
            Instance = this;
        }
        //save vars to memory
        weatherManager = weatherManager.gameObject.GetComponent<WeatherManager>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        // set up ocean backdrop
        if(renderMap)
        {
            SpriteRenderer oceanBackground =  Instantiate(OceanBackground);
            oceanBackground.transform.position = new Vector3(generationSize, generationSize, 0.1f);
            oceanBackground.transform.localScale = Vector2.one * generationSize * 4;
            oceanBackground.transform.parent = gameObject.transform;
        }

        // create delauny object and voronoi cells
        float debugTime = Time.realtimeSinceStartup;
        SetupDelauny();
        if(debugMap) 
        {
            Debug.Log(String.Format("Time to generate Delauny Graph {0}", Time.realtimeSinceStartup - debugTime));
        }

        debugTime = Time.realtimeSinceStartup;
        CreateVoronoi();
        if(debugMap) 
        {
            Debug.Log(String.Format("Time to generate voronoi cells {0}", Time.realtimeSinceStartup - debugTime));
        }

        //Debug.Log(String.Format("{0} -- does it contain", mapCells[0].mapCollider.bounds.Contains(new Vector3(mapCells[0].coord.x - 0.0001f, mapCells[0].coord.y + 0.0001f, 0f))));

        debugTime = Time.realtimeSinceStartup;
        SetVoronoiNeighborCells();
        if(debugMap) 
        {
            Debug.Log(String.Format("Time to set neighbor cells {0}", Time.realtimeSinceStartup - debugTime));
        }

        debugTime = Time.realtimeSinceStartup;
        SetCoastalCells();
        if(debugMap) 
        {
            Debug.Log(String.Format("Time to set coastal cells {0}", Time.realtimeSinceStartup - debugTime));
        }

        
    }

    private void Update() {
        // TODO -- Convert to states
        if(setWaterTexture && !waterSet)
        {
            groundSet = false;
            waterSet = true;
            foreach(MapCells cell in mapCells)
            {
                if(cell.waterTexture == null)
                {
                    cell.generateWaterTexture();
                }
                cell.setWaterTexture();
            }
        }
        if(setGroundTexture && !groundSet)
        {
            waterSet = false;
            groundSet = true;
            foreach(MapCells cell in mapCells)
            {
                cell.setHeightTexture();
            }
        }
        if(startRivers && !riverSet)
        {
            riverSet = true;
            GenerateRivers();
        }
    }

    private void SetupDelauny()
    {
        CreateNewContainers();
        // get random distribution of points
        Vector2 topLeft = new Vector2(0,0);
        Vector2 lowerRight = new Vector2(2*generationSize,2*generationSize);
        var sampler = DelaunatorSharp.Unity.UniformPoissonDiskSampler.SampleRectangle(topLeft, lowerRight, generationMinDistance);
        points = sampler.Select(point => new Vector2(point.x, point.y)).ToPoints().ToList();
        Debug.Log($"Generated Points Count {points.Count}");

        //create delauny object
        delaunator = new Delaunator(points.ToArray());
        originPoint = new Vector2(Mathf.Sqrt(Seed), Mathf.Sqrt(Seed));
    }

    private void Clear()
    {
        foreach (Transform child in CellsContainer.transform)
        {
            Destroy(child.gameObject);
        }
        delaunator = null;
    }
    
    private void CreateVoronoi()
    {
        if (delaunator == null) return;

        //loop through each cell from the delauny diagram and generate voronoi polygons            
        delaunator.ForEachVoronoiCell(cell =>
        {
            // generate cell points
            List<int> triangles = new List<int>();
            List<Vector3> verticies = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            float totalX = 0f;
            float totalY = 0f;
            foreach(IPoint point in cell.Points)
            {
                verticies.Add(point.ToVector3());
                uvs.Add(new Vector2(0,1));
                totalX += (float)point.X;
                totalY += (float)point.Y;
            }

            Vector3 midpoint = new Vector3(totalX/cell.Points.Count(),totalY/cell.Points.Count(),0);
            float height = MapCells.Noisefunction(midpoint.x * 0.5f, midpoint.y * 0.5f, originPoint);
            
            // if the cell is a land cell
            if(height >= oceanHeight)
            {
                CreateVoronoiCell(verticies, triangles, midpoint, uvs, height);
            }
        });
    }

    private void CreateVoronoiCell(List<Vector3> vertices, List<int> triangles, Vector3 midpoint, List<Vector2> uvs, float height)
    {
        cellPoints.Add(new Vector2(midpoint.x,midpoint.y));
        vertices.Add(midpoint);
        uvs.Add(new Vector2(0,1));
        MapCells voronoiCell = Instantiate(mapCellsPrefab,Vector3.zero,new quaternion(0,0,0,0));
        for(int i = 0; i < vertices.Count() - 1 ; i++)
        {   
            triangles.Add(i);
            triangles.Add(vertices.Count()-1);
            if(i + 1 >= vertices.Count() - 1){
                triangles.Add(0);
                MapEdge tempEdge = new MapEdge();
                tempEdge.pointA = vertices[i];
                tempEdge.pointB = vertices[0];
                voronoiCell.edges.Add(tempEdge);
            }
            else{
                triangles.Add(i+1);
                MapEdge tempEdge = new MapEdge();
                tempEdge.pointA = vertices[i];
                tempEdge.pointB = vertices[i+1];
                voronoiCell.edges.Add(tempEdge);
            } 
        }
        
        voronoiCell.maxNumberOfNeightbors = vertices.Count -1;
        voronoiCell.transform.parent = CellsContainer;
        voronoiCell.name = string.Format("cell at ({0},{1})", midpoint.x, midpoint.y);
        voronoiCell.coord = new Vector2(midpoint.x,midpoint.y);
        voronoiCell.height = height;
        voronoiCell.generateOriginalMesh(vertices.ToArray(), triangles.ToArray(), uvs.ToArray(), midpoint);

        vertices.RemoveAt(vertices.Count-1);
        voronoiCell.vertices = vertices;

        cellQuadTree.insert(voronoiCell.coord, voronoiCell);
        mapCells.Add(voronoiCell);
    }

    private void SetVoronoiNeighborCells()
    {
        Delaunator delaunator_tri = new Delaunator(cellPoints.Select(point => new Vector2(point.x, point.y)).ToPoints());
        Transform tempLine = new GameObject().transform;
        
        delaunator_tri.ForEachTriangleEdge(edge =>
        {
            if(debugMap)
            {
                Vector3 tempP = edge.P.ToVector3();
                tempP.z = -0.1f;
                Vector3 tempQ = edge.Q.ToVector3();
                tempQ.z = -0.1f;
                CreateLine(tempLine, $"TriangleEdge - {edge.Index}", new Vector3[] { tempP, tempQ }, new Color(1,1,1), 0.001f, 0);
            }

            Vector2 qPoint = new Vector2((float)edge.Q.X, (float)edge.Q.Y);
            Vector2 pPoint = new Vector2((float)edge.P.X, (float)edge.P.Y);
    
            MapCells qCell = (MapCells)cellQuadTree.getObjectAtPoint(qPoint);
            MapCells pCell = (MapCells)cellQuadTree.getObjectAtPoint(pPoint);

            float distance = (qCell.GetComponent<PolygonCollider2D>().Distance(pCell.GetComponent<PolygonCollider2D>())).distance;
            if((distance <= 0))
            {
                qCell.neightbors.Add(pCell);
                pCell.neightbors.Add(qCell);
            }
            
        });
    }

    private void CreateLine(Transform container, string name, Vector3[] points, Color color, float width, int order = 1)
    {
        var lineGameObject = new GameObject(name);
        lineGameObject.transform.parent = container;
        var lineRenderer = lineGameObject.AddComponent<LineRenderer>();

        lineRenderer.SetPositions(points);

        lineRenderer.material = new Material(Shader.Find("Standard"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.sortingOrder = order;
    }

    private void SetCoastalCells()
    {
        foreach(MapCells cell in mapCells)
        {
            if((cell.neightbors.Count < cell.maxNumberOfNeightbors) && (cell.height < 30.0f))
            {
                cell.isCoastCell = true;
            }
            else
            {
                cell.isCoastCell = false;
            }
        }
    }



    private void CreateNewContainers()
    {
        CreateCellsContainer();
        CreateRiversContainer();
    }

    private void CreateCellsContainer()
    {
        if (CellsContainer != null)
        {
            Destroy(CellsContainer.gameObject);
        }

        CellsContainer = new GameObject(nameof(CellsContainer)).transform;
    }

    private void CreateRiversContainer()
    {
        if (RiversContainer != null)
        {
            Destroy(RiversContainer.gameObject);
        }

        RiversContainer = new GameObject(nameof(RiversContainer)).transform;
    }

    public void GenerateRivers()
    {
        List<MapCells> sortedCellsByHeight = mapCells.OrderBy (e => e.height).ToList();
        depressionFill(sortedCellsByHeight);
        for(int i = sortedCellsByHeight.Count-1; i >=0 ; i--)
        {
            MapCells cell = sortedCellsByHeight[i];
            if (!cell.isCoastCell)
            {                
                if (cell.river==null)
                {
                    River cellRiver = Instantiate(riverPrefab);
                    cellRiver.riverID = sortedCellsByHeight.Count - i;
                    cellRiver.name = String.Format("River-{0}", cellRiver.riverID);
                    cellRiver.createRiverSegment(cell);
                    cellRiver.transform.parent = RiversContainer.transform;
                }
            }
        }
        foreach(MapCells cell in mapCells)
        {
            if(cell.waterTexture == null)
            {
                cell.generateWaterTexture();
            }
        }
    }

    private void depressionFill(List<MapCells> sortedCells)
    {
        float depressStartTime = Time.realtimeSinceStartup;

        // keep depressing cells until loop goes through without increasing height
        bool keepDepressing = true;
        while(keepDepressing)
        {
            keepDepressing = false;
            foreach(MapCells cell in sortedCells)
            {
                if (!cell.isCoastCell)
                {
                    bool keepDepressingCell = true;
                    while(keepDepressingCell)
                    {
                        if(cell.isCellLowest())
                        {
                            cell.height += depressIncrease;
                            keepDepressing = true;
                        }
                        else
                        {
                            keepDepressingCell = false;
                        }
                    }
                }
            }
        }
        if(debugMap) 
        {
            Debug.Log(String.Format("Time to apply depression alogrithm {0}", Time.realtimeSinceStartup - depressStartTime));
        }
    }
}
//}

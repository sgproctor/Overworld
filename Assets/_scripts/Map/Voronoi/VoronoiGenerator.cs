using UnityEngine;
using System;
using System.Linq;
using DelaunatorSharp;
using System.Collections.Generic;
using DelaunatorSharp.Unity.Extensions;

public class VoronoiGenerator : MonoBehaviour
{
    public static VoronoiGenerator Instance;
    public Material lineMaterial;
    public Biome[] biomes;
    public Dictionary<Vector3, MapCells> cellMap;
    private List<IPoint> points;
    List<IheightIncrease> heightIncreaseList;
    // private IColorable meshColorer;
    private QuadTree<MapCells> cellQuadTree;
    [SerializeField] MapCells inspectedCell;

    public Dictionary<Vector3, float> vertexHeightMap;
    private Color[] mapColors;
    // public MeshRenderer meshRenderer;
    // public MeshFilter meshFilter;
    // public Material meshMaterial;
    // public Material lineMaterial;
    List<VoronoiEdge> voronoiEdges;
    
    public int width;
    public int height;
    public float waterLevel = 0.2f;

    public int generationSize;

    private Vector2 topLeft;
    private Vector2 lowerRight;

    private Delaunator delaunay;
    // public enum DisplayEnum { height, biome, temperature, precipitation}
    // public DisplayEnum displayEnum;
    // public Gradient heightGradient;
    // public Gradient tempGradient;
    // public Gradient precipitationGradient;
    public int numBlobs;
    public int blobRadius;
    public int CircleRadius;
    public float CircleScale;
    public Vector2 Origin;
    public float perturbScale;
    public int seed;
    public bool debugMap;
    public bool randomSeed;
    public float maxFlux = 0;
    public float noiseScale;
    public int octaves;
    public float persistance;
    public float lacunarity;
    List<Vector3> cellPoints;
    // public int windIterations;
    // public int initialWaterVal;
    // public float slopeDelfectionMod;
    WeatherManager weatherManager;
    MapDisplayManager mapDisplayManager;
    Cities cities;
    Coast coasts;
    River rivers;
    public float equator;
    public float tropic;
    public float artic;


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
    public void GenerateMap(int cellSize = 0)
    {
        generationSize = cellSize == 0 ? generationSize : cellSize;
        float debugTime = Time.realtimeSinceStartup;
        if(randomSeed){
            seed = UnityEngine.Random.Range(0, int.MaxValue);
            Origin = new Vector2(Mathf.Sqrt(Math.Abs(seed.GetHashCode())), 
							     Mathf.Sqrt(Math.Abs(seed.GetHashCode())));
        }
        UnityEngine.Random.InitState(seed);

        cellQuadTree = new QuadTree<MapCells>(10, new Rect(0,0,width,height));
        cellMap = new Dictionary<Vector3, MapCells>();
        voronoiEdges = new List<VoronoiEdge>();
        cellPoints = new List<Vector3>();
        vertexHeightMap = new Dictionary<Vector3, float>();
        heightIncreaseList = new List<IheightIncrease>();
        points = new List<IPoint>();

        mapDisplayManager = GameObject.Find("MapDisplayManager").GetComponent<MapDisplayManager>();
        weatherManager = GameObject.Find("WeatherManager").GetComponent<WeatherManager>();
        coasts = GameObject.Find("Coasts").GetComponent<Coast>();
        rivers = GameObject.FindGameObjectWithTag("River").GetComponent<River>();
        cities = GameObject.FindGameObjectWithTag("City").GetComponent<Cities>();

        coasts.DeleteChildren();
        rivers.DeleteChildren();
        cities.DeleteChildren();

        GenerateHeightMappers();
        mapDisplayManager.GenerateMeshcolorers();

        SetupDelauny();
        if(debugMap) 
        {
            Debug.Log(String.Format("Time to generate Delauny Graph {0}", Time.realtimeSinceStartup - debugTime));
        }

        CreateVoronoi();
        if(debugMap) 
        {
            Debug.Log(String.Format("Time to generate Voronoi Cells {0}", Time.realtimeSinceStartup - debugTime));
        }

        Erosion.fillSinks(cellMap, vertexHeightMap);

        RunWeatherSim();
        SetCellPrecipitation();
        if(debugMap) 
        {
            Debug.Log(String.Format("Time to run weather sim {0}", Time.realtimeSinceStartup - debugTime));
        }

        maxFlux = Erosion.setFlux(cellMap, waterLevel);
        Erosion.setSlope(cellMap);
        
        Erosion.setErosionRate(cellMap);
        Erosion.doErosion(cellMap, vertexHeightMap, 0.15f, waterLevel, 5);
        //coasts.cleanCoastline(cellMap, vertexHeightMap, waterLevel);
        coasts.cleanCoastline(cellMap, vertexHeightMap, waterLevel);
        Erosion.fillSinks(cellMap, vertexHeightMap);
        if(debugMap) 
        {
            Debug.Log(String.Format("Time to fill sinks {0}", Time.realtimeSinceStartup - debugTime));
        }


        CreateVoronoiEdges();
        coasts.DrawSeaLevelContour(generationSize/10f, lineMaterial, voronoiEdges, vertexHeightMap, waterLevel);
        if(debugMap) 
        {
            Debug.Log(String.Format("Time to generate Coast {0}", Time.realtimeSinceStartup - debugTime));
        }

        maxFlux = Erosion.setFlux(cellMap, waterLevel);
        rivers.DrawRivers(cellMap, maxFlux, waterLevel, generationSize, lineMaterial);
        if(debugMap) 
        {
            Debug.Log(String.Format("Time to generate Rivers {0}", Time.realtimeSinceStartup - debugTime));
        }
        
        FilterPrecipitationMinima();
        if(debugMap) 
        {
            Debug.Log(String.Format("Time to set precipitation {0}", Time.realtimeSinceStartup - debugTime));
        }
        
        SetCellBiome();
        if(debugMap) 
        {
            Debug.Log(String.Format("Time to set biomes {0}", Time.realtimeSinceStartup - debugTime));
        }
        //DrawLatLines();
        mapDisplayManager.maxCityScore = cities.SetCityScores(cellMap, waterLevel);
        mapDisplayManager.UpdateDisplay();

    }

    public void PlaceCities(int numCities)
    {
        for (int i = 0; i < numCities; i++)
        {
            cities.AddCity(cellMap);
        }
    }

    void Start()
    {
        //GenerateMap();
    }

    private void DrawLatLines()
    {
        LineDrawer.CreateLine(gameObject.transform, "equator", new Vector3[] {
            new Vector2(0,equator*height), new Vector2(width, equator*height)}, 
            Color.black, generationSize, lineMaterial, 5, 5);
        LineDrawer.CreateLine(gameObject.transform, "tropic", new Vector3[] {
            new Vector2(0,tropic*height), new Vector2(width, tropic*height)}, 
            Color.black, generationSize, lineMaterial, 5, 5);
        LineDrawer.CreateLine(gameObject.transform, "artic", new Vector3[] {
            new Vector2(0,artic*height), new Vector2(width, artic*height)}, 
            Color.black, generationSize, lineMaterial, 5, 5);
    }

    void Update() {
		// if (Input.GetMouseButtonDown(0)){
		// 	Vector2 clickedPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //     float startTime = Time.realtimeSinceStartup;
        //     Debug.Log(String.Format("Time to find cell using tree{0}", Time.realtimeSinceStartup - startTime));
        //     inspectedCell = GetCellAtPoint(clickedPoint);
        //     foreach(Vector3 neighbor in inspectedCell.neighbors)
        //     {
        //         Debug.DrawLine(inspectedCell.coord, neighbor, Color.black, 1);
        //     }
        //     //Debug.DrawLine(inspectedCell.coord, downHillNeighbor(inspectedCell).coord, Color.red, 5);

        //     Debug.Log(String.Format("Slope to DH Neighbor {0}", inspectedCell.slope));
        //     Debug.Log(String.Format("Flux {0}", inspectedCell.flux));
		// }   
    }

    private void RunWeatherSim()
    {

        weatherManager.SetLatitudes(artic: height * artic, tropic: height * tropic, equator: height * equator);
        int cellsize = generationSize;

        float[,] heightmap = new float[width/cellsize,height/cellsize];
        for(int x = 0; x < width/cellsize; x ++)
        {
            for(int y = 0; y < height/cellsize; y++)
            {
                heightmap[x,y] = GetCellAtPoint(new Vector2(x*cellsize + cellsize/2f,y*cellsize + cellsize/2f)).height;
            }
        }
        weatherManager.RunSim(height, width, generationSize, waterLevel, heightmap);
    }

    private void SetCellPrecipitation()
    {
        List<MapCells> cells = cellMap.Values.ToList();
        cells = cells.Where( cell => cell.height >= waterLevel).ToList();

        foreach(MapCells cell in cells)
        {
            cell.precipitation = weatherManager.precipitationGrid.getItem(cell.coord);
            cell.flux = cell.precipitation;
        }
    }

    private void SetCellBiome()
    {
        List<MapCells> cells = cellMap.Values.ToList();
        cells = cells.Where( cell => cell.height >= waterLevel).ToList();

        foreach(MapCells cell in cells)
        {
            cell.setBiome(biomes);
        }
    }

    private void FilterPrecipitationMinima()
    {
        List<MapCells> cells = cellMap.Values.ToList();
        cells = cells.Where( cell => cell.height >= waterLevel).ToList();
        bool keepFiltering = true;
        while(keepFiltering)
        {
            keepFiltering = false;
            foreach(MapCells cell in cells)
            {
                if(cell.precipitation == 0)
                {
                    int validNeighbors = 0;
                    float precipSum = 0;

                    foreach(float nPrecip in weatherManager.precipitationGrid.GetCellNeighbors(cell.coord))
                    {
                        if(nPrecip > 0)
                        {
                            validNeighbors ++;
                            precipSum += nPrecip;
                        }
                    }
                    if(validNeighbors == 0)
                    {
                        keepFiltering = true;
                        continue;
                    }
                    cell.precipitation = precipSum / (float) validNeighbors;
                }
            }
        }
    }

    public MapCells GetCellAtPoint(Vector2 point)
    {
        //List<Vector3> cellPoints = cellMap.Keys.ToList();
        Rect quadRect = new Rect(point.x - generationSize, point.y - generationSize,
                                2 * generationSize, 2* generationSize);

        //if(debugMap) DebugUtils.DrawRect(quadRect, Color.black, 1);
        List<MapCells> cells = cellQuadTree.RetrieveObjectsInArea(quadRect);
        //Debug.Log(cells.Count);
        float minDistance = 10000;
        MapCells returncell = new MapCells();
        //returncell = cells.First();
        foreach(MapCells cell in cells)
        {
            //if(cell.PointInCell())
            float distance = Vector2.Distance(cell.coord, point); 
            if(distance < minDistance)
            {
                minDistance = distance;
                returncell = cell;
            }
        }
        return returncell;
    }

    public void GenerateHeightMappers()
    {
        for(int i = 0; i < numBlobs; i++)
        {
            PerturbedCircle circle = new PerturbedCircle(CircleRadius, width, height, CircleScale, Origin, perturbScale);
            heightIncreaseList.Add(circle);            
        }
        HeightNoise hNoise = new HeightNoise(Origin, noiseScale, octaves, persistance, lacunarity);
        heightIncreaseList.Add(hNoise);
    }

    public void SetupDelauny()
    {
        topLeft = new Vector2(0,0);
        lowerRight = new Vector2(width, height);
        var sampler =  DelaunatorSharp.Unity.UniformPoissonDiskSampler.SampleRectangle(topLeft, lowerRight, generationSize);
        points = sampler.Select(point => new Vector2(point.x, point.y)).ToPoints().ToList();

        Debug.Log($"Generated Points Count {points.Count}");
        delaunay = new Delaunator(points.ToArray());
    }

    private void addVertex(Vector3 vertex, Vector3 cellMidPoint)
    {
        if(!vertexHeightMap.ContainsKey(vertex))
        {
            vertexHeightMap.Add(vertex, getHeightAtPoint(vertex));
        }
    }

    private void CreateVoronoiEdges()
    {
        delaunay.ForEachTriangleEdge(edge =>
        {
            var triangle = Delaunator.TriangleOfEdge(edge.Index);
            Vector3 trianglePoint = delaunay.GetCentroid(Delaunator.TriangleOfEdge(edge.Index)).ToVector3();

            var adjectentTriangle = delaunay.Halfedges[edge.Index];
            if(adjectentTriangle == -1) return;
            Vector3 adjacentTrianglePoint = delaunay.GetCentroid(Delaunator.TriangleOfEdge(adjectentTriangle)).ToVector3();
            Vector3 right, left;
            if(trianglePoint.x >= adjacentTrianglePoint.x)
            {
                right = trianglePoint;
                left = adjacentTrianglePoint;
            }
            else
            {
                left = trianglePoint;
                right = adjacentTrianglePoint;
            }

            VoronoiEdge vEdge = new VoronoiEdge(edge.P.ToVector3(), edge.Q.ToVector3(), right, left);
            voronoiEdges.Add(vEdge);
        });
    }

    public void CreateVoronoi()
    {       
        int triCount = 0; 
        foreach(IVoronoiCell cell in GetVoronoiCells())
        {
            if(cell.Points.Count() <= 2) continue;
            List<Vector3> cellNeighbors = new List<Vector3>();
            List<Vector3> cellVertices = new List<Vector3>();
            List<int> cellTriangles = new List<int>();

            foreach (var edge in delaunay.EdgesAroundPoint(cell.Index))
            {
                Vector3[] points = delaunay.GetTrianglePoints(Delaunator.TriangleOfEdge(edge)).ToVectors3();
                cellNeighbors.AddRange(points);
            }

            Vector3 midpoint = cellNeighbors.GroupBy(s => s)
                         .OrderByDescending(s => s.Count())
                         .First().Key;

            cellNeighbors = cellNeighbors.Distinct().ToList();
            cellNeighbors.Remove(midpoint);

            for(int i = 0; i < cell.Points.Count() ; i++)
            {   
                addVertex(cell.Points[i].ToVector3(), midpoint);
                cellTriangles.Add(triCount + i);
                cellTriangles.Add(triCount + cell.Points.Count()-1);

                if(i + 1 >= cell.Points.Count()-1){
                    cellTriangles.Add(triCount);
                }

                else{
                    cellTriangles.Add(triCount+i+1);
                } 
            }
            addVertex(midpoint, midpoint);
            triCount = triCount + cell.Points.Count() + 1;

            float height = getHeightAtPoint(midpoint);
            MapCells vCell = new MapCells(midpoint, cell.Points.ToVectors3().ToList(),
                                                    cellTriangles, height);
            vCell.neighbors = cellNeighbors;
            cellQuadTree.Insert(vCell);
            //cellQuadTree.insert(midpoint, vCell);
            if(!cellMap.ContainsKey(midpoint))
            {
                cellMap.Add(midpoint, vCell);
            }
            vCell.setTemp(this.height * equator, waterLevel, this.height);
        }
        cellPoints = cellMap.Keys.ToList();
        mapDisplayManager.CreateMapMesh();
    }

    public float getHeightAtPoint(Vector2 point)
    {
        float heightSum = 0;
        for (int i = 0; i < heightIncreaseList.Count - 1; i++)
        {
            heightSum = Mathf.Max(heightIncreaseList[i].heightAt(point), heightSum);
        }
        heightSum *= heightIncreaseList.Last().heightAt(point);

        if(heightSum >= waterLevel)
        {
            heightSum = Mathf.Lerp(0f,1f, heightSum);
            heightSum = Mathf.Pow(heightSum, 3);
            heightSum = Mathf.Lerp(0.2f,1f, heightSum);
        }
        return heightSum;
    }

    public IEnumerable<IVoronoiCell> GetVoronoiCells(Func<int, IPoint> triangleVerticeSelector = null)
    {
        if (triangleVerticeSelector == null) triangleVerticeSelector = x => delaunay.GetCentroid(x);

        var seen = new HashSet<int>();
        var vertices = new List<IPoint>(10);    // Keep it outside the loop, reuse capacity, less resizes.

        for (var triangleId = 0; triangleId < delaunay.Triangles.Length; triangleId++)
        {
            var id = delaunay.Triangles[Delaunator.NextHalfedge(triangleId)];
            
            // True if element was added, If resize the set? O(n) : O(1)
            if (seen.Add(id))
            {
                foreach (var edge in delaunay.EdgesAroundPoint(triangleId))
                {
                    // triangleVerticeSelector cant be null, no need to check before invoke (?.).
                    vertices.Add(triangleVerticeSelector.Invoke(Delaunator.TriangleOfEdge(edge)));
                }
                yield return new VoronoiCell(triangleId, vertices.ToArray());
                vertices.Clear();   // Clear elements, keep capacity
            }
        }
    }
}


// using UnityEngine;
// using System.Linq;
// using DelaunatorSharp;
// using Unity.Mathematics;
// using System.Collections.Generic;
// using DelaunatorSharp.Unity.Extensions;


// public class MapGenerator : MonoBehaviour
// {
    
//     // --------------public variables--------------

//     // --------------serialized variables--------------
//     [SerializeField] MapCells mapCellsPrefab;
//     public List<MapCells> mapCells = new List<MapCells>();

//     // --------------private variables--------------
//     // delauntator vars
//     private List<IPoint> points = new List<IPoint>();
//     private List<Vector2> cellPoints = new List<Vector2>();
//     private QuadTree<MapCells> cellQuadTree;

//     // texture vars
//     private Color[] col;
//     private Texture2D tex;
//     private Vector2 originPoint = new Vector2();

//     public float xSize;
//     public float ySize;
//     public float xOffset;
//     public float yOffset;

//     private Vector2 topLeft;
//     private Vector2 lowerRight;

//     private Delaunator delaunay;


//     public void SetupDelauny()
//     {
//         //cellQuadTree = new QuadTree<MapCells>(new Rect());
//         topLeft = new Vector2(xOffset, yOffset);
//         lowerRight = new Vector2(xOffset + xSize, yOffset + ySize);
//         var sampler =  DelaunatorSharp.Unity.UniformPoissonDiskSampler.SampleRectangle(topLeft, lowerRight, MapManager.Instance.generationMinDistance);
//         points = sampler.Select(point => new Vector2(point.x, point.y)).ToPoints().ToList();
//         Debug.Log($"Generated Points Count {points.Count}");

//         originPoint = new Vector2(Mathf.Sqrt(MapManager.Instance.Seed), Mathf.Sqrt(MapManager.Instance.Seed));
//         delaunay = new Delaunator(points.ToArray());
        
//     }
    
//     public void CreateVoronoi()
//     {

//         //loop through each cell from the delauny diagram and generate voronoi polygons            
//         delaunay.ForEachVoronoiCell(cell =>
//         {
//             // generate cell points
//             List<int> triangles = new List<int>();
//             List<Vector3> verticies = new List<Vector3>();
//             List<Vector2> uvs = new List<Vector2>();
//             float totalX = 0f;
//             float totalY = 0f;
//             foreach(IPoint point in cell.Points)
//             {
//                 verticies.Add(point.ToVector3()); 
//                 uvs.Add(new Vector2(0,1));
//                 totalX += (float)point.X;
//                 totalY += (float)point.Y;
//             }

//             Vector3 midpoint = new Vector3(totalX/cell.Points.Count(),totalY/cell.Points.Count(),0);
//             float height = MapCells.Noisefunction(midpoint.x * 0.5f, midpoint.y * 0.5f, xOffset, yOffset, xSize,  ySize, originPoint);
            
//             // if the cell is a land cell
//             if(height >= MapManager.Instance.oceanHeight)
//             {
//                 CreateVoronoiCell(verticies, triangles, midpoint, uvs, height);
//             }
//         });
//     }

//     public void CreateVoronoiCell(List<Vector3> vertices, List<int> triangles, Vector3 midpoint, List<Vector2> uvs, float height)
//     {
//         cellPoints.Add(new Vector2(midpoint.x,midpoint.y));
//         vertices.Add(midpoint);
//         uvs.Add(new Vector2(0,1));
//         MapCells voronoiCell = Instantiate(mapCellsPrefab,Vector3.zero,new quaternion(0,0,0,0));
//         for(int i = 0; i < vertices.Count() - 1 ; i++)
//         {   
//             triangles.Add(i);
//             triangles.Add(vertices.Count()-1);
//             if(i + 1 >= vertices.Count() - 1){
//                 triangles.Add(0);
//                 MapEdge tempEdge = new MapEdge();
//                 tempEdge.pointA = vertices[i];
//                 tempEdge.pointB = vertices[0];
//                 voronoiCell.edges.Add(tempEdge);
//             }
//             else{
//                 triangles.Add(i+1);
//                 MapEdge tempEdge = new MapEdge();
//                 tempEdge.pointA = vertices[i];
//                 tempEdge.pointB = vertices[i+1];
//                 voronoiCell.edges.Add(tempEdge);
//             } 
//         }
        
//         voronoiCell.maxNumberOfNeightbors = vertices.Count -1;
//         voronoiCell.transform.parent = MapManager.Instance.CellsContainer;
//         voronoiCell.name = string.Format("cell at ({0},{1})", midpoint.x, midpoint.y);
//         voronoiCell.coord = new Vector2(midpoint.x,midpoint.y);
//         voronoiCell.height = height;
//         voronoiCell.generateOriginalMesh(vertices.ToArray(), triangles.ToArray(), uvs.ToArray(), midpoint);

//         vertices.RemoveAt(vertices.Count-1);
//         voronoiCell.vertices = vertices;

//         //cellQuadTree.insert(voronoiCell.coord, voronoiCell);
//         mapCells.Add(voronoiCell);
//     }

//     public void SetVoronoiNeighborCells()
//     {
//         Delaunator delaunator_tri = new Delaunator(cellPoints.Select(point => new Vector2(point.x, point.y)).ToPoints());
//         Transform tempLine = new GameObject().transform;
        
//         delaunator_tri.ForEachTriangleEdge(edge =>
//         {
//             if(MapManager.Instance.debugMap)
//             {
//                 Vector3 tempP = edge.P.ToVector3();
//                 tempP.z = -0.1f;
//                 Vector3 tempQ = edge.Q.ToVector3();
//                 tempQ.z = -0.1f;
//                 CreateLine(tempLine, $"TriangleEdge - {edge.Index}", new Vector3[] { tempP, tempQ }, new Color(1,1,1), 0.001f, 0);
//             }

//             Vector2 qPoint = new Vector2((float)edge.Q.X, (float)edge.Q.Y);
//             Vector2 pPoint = new Vector2((float)edge.P.X, (float)edge.P.Y);
    
//             //MapCells qCell = (MapCells)cellQuadTree.getObjectAtPoint(qPoint);
//             //MapCells pCell = (MapCells)cellQuadTree.getObjectAtPoint(pPoint);

//             float distance = (qCell.GetComponent<PolygonCollider2D>().Distance(pCell.GetComponent<PolygonCollider2D>())).distance;
//             if((distance <= 0))
//             {
//                 qCell.neightbors.Add(pCell);
//                 pCell.neightbors.Add(qCell);
//             }
            
//         });
//     }

//     public void CreateLine(Transform container, string name, Vector3[] points, Color color, float width, int order = 1)
//     {
//         var lineGameObject = new GameObject(name);
//         lineGameObject.transform.parent = container;
//         var lineRenderer = lineGameObject.AddComponent<LineRenderer>();

//         lineRenderer.SetPositions(points);

//         lineRenderer.material = new Material(Shader.Find("Standard"));
//         lineRenderer.startColor = color;
//         lineRenderer.endColor = color;
//         lineRenderer.startWidth = width;
//         lineRenderer.endWidth = width;
//         lineRenderer.sortingOrder = order;
//     }

//     public void SetCoastalCells()
//     {
//         foreach(MapCells cell in mapCells)
//         {
//             if((cell.height < 0.3f) && (cell.neightbors.Count < cell.maxNumberOfNeightbors))
//             {
//                 cell.isCoastCell = true;
//             }
//             else
//             {
//                 cell.isCoastCell = false;
//             }
//         }
//     }
// }


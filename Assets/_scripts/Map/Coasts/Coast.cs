using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Coast : MonoBehaviour
{
    public void DeleteChildren()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void DrawSeaLevelContour(float segmentSize, Material lineMaterial, List<VoronoiEdge> edges, 
                                    Dictionary<Vector3, float> vertexHeightmap, float waterLevel)
    {
        List<Vector3> contourEdges = AddSeaEdges(edges, vertexHeightmap, waterLevel);
        
        foreach(List<Vector3> segment in LineMerger.mergeLineSegments(contourEdges))
        {
            // LineDrawer.CreateLine(gameObject.transform, "edge", segment.ToArray(), Color.black, 
            //                         generationSize, lineMaterial, 3, 3 );
            List<float> widths = new List<float>();
            for (int i = 0; i < segment.Count; i++)
            {
                widths.Add(3);
            }

            LineDrawer.CreateDynamicLine(gameObject.transform, "coast", segment.ToArray(), Color.black,
                                            widths.ToArray(), 1, segmentSize, lineMaterial);
        }
    }
    private List<Vector3> AddSeaEdges(List<VoronoiEdge> edges, Dictionary<Vector3, float> vertexHeightmap, 
                                      float waterLevel)
    {
        List<Vector3> contourPoints = new List<Vector3>();
        foreach(VoronoiEdge edge in edges)
        {
            if(!vertexHeightmap.ContainsKey(edge.pointA) || !vertexHeightmap.ContainsKey(edge.pointB)) continue;
            float heightA = vertexHeightmap[edge.pointA];
            float heightB = vertexHeightmap[edge.pointB];

            if((heightA > waterLevel && heightB <= waterLevel) ||
               (heightB > waterLevel && heightA <= waterLevel)) {
                    contourPoints.Add(edge.leftPoint);
                    contourPoints.Add(edge.rightPoint);
            }
        }
        return contourPoints;
    }

    public void cleanCoastline(Dictionary<Vector3, MapCells> cellMap, Dictionary<Vector3, float> vertexHeightMap, 
                                float waterLevel)
    {
        List<MapCells> cells = cellMap.Values.ToList();
        foreach(MapCells cell in cells)
        {
            // if the cell is above the waterlevel, check if majority of neighbors are above
            // if not, cell's new height is the highest cell below water
            if(cell.height > waterLevel && cell.height <= waterLevel * 1.2f)
            {
                int aboveCount = 0;
                float highestShore = 0;
                foreach(Vector3 neighborVertex in cell.neighbors)
                {
                    if(cellMap[neighborVertex].height > waterLevel) 
                    {
                        aboveCount += 1;
                    }
                    else
                    {
                        if(cellMap[neighborVertex].height > highestShore)
                        {
                            highestShore = cellMap[neighborVertex].height;
                        }
                    }
                }
                if(aboveCount < cell.neighbors.Count / 2)
                {
                    float difference = cell.height - highestShore;
                    cell.height = highestShore;
                    vertexHeightMap[cell.coord] = highestShore;
                    foreach(Vector3 vertex in cell.vertices)
                    {
                        vertexHeightMap[vertex] -= difference;
                    }
                }
            }
            else if(cell.height <= waterLevel && cell.height >= waterLevel * 0.8f)
            {
                int belowCount = 0;
                float lowestbeach = 1;
                foreach(Vector3 neighborVertex in cell.neighbors)
                {
                    if(cellMap[neighborVertex].height <= waterLevel) 
                    {
                        belowCount += 1;
                    }
                    else
                    {
                        if(cellMap[neighborVertex].height < lowestbeach)
                        {
                            lowestbeach = cellMap[neighborVertex].height;
                        }
                    }
                }
                if(belowCount < cell.neighbors.Count / 2)
                {
                    float difference = lowestbeach - cell.height;
                    cell.height = lowestbeach;
                    vertexHeightMap[cell.coord] = lowestbeach;
                    foreach(Vector3 vertex in cell.vertices)
                    {
                        vertexHeightMap[vertex] -= difference;
                    }
                }
            }
        }
    }
}
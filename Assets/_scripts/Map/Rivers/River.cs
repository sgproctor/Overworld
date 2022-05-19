using UnityEngine;
using System.Linq;
using System.Collections.Generic;


public class River : MonoBehaviour
{
    public void DeleteChildren()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void DrawRivers(Dictionary<Vector3, MapCells> cellMap, float maxFlux, float waterLevel, float generationSize,
                            Material lineMaterial)
    {
        GameObject rivers = GameObject.Find("Rivers");
        Debug.Log(rivers);
        float widthMod = 10f;
        float minWidth = 1f/widthMod;
        foreach(List<Vector3> segment in AddRiverSegments(cellMap, maxFlux, waterLevel))
        {
            List<float> widths = new List<float>();
            widths.Add(minWidth);
            for (int i = 1; i < segment.Count - 1; i++)
            {
                widths.Add(minWidth+(cellMap[segment[i]].flux / maxFlux));
            }
            widths.Add(minWidth+(cellMap[segment[segment.Count - 2]].flux / maxFlux));

            LineDrawer.CreateDynamicLine(rivers.transform, "river", segment.ToArray(), Color.blue, 
                                            widths.ToArray(), widthMod, generationSize/4, lineMaterial);
        }
    }

    private List<List<Vector3>> AddRiverSegments(Dictionary<Vector3, MapCells> cellMap, float maxFlux, float waterLevel)
    {
        List<Vector3> addedCells = new List<Vector3>();
        float fluxLimit = maxFlux * 0.0025f;
        List<MapCells> cells = cellMap.Values.ToList();
        List<List<Vector3>> rivers = new List<List<Vector3>>();
        cells = cells.Where( cell => cell.flux > fluxLimit && cell.height > waterLevel).ToList();
        foreach(MapCells cell in cells)
        {
            if(addedCells.Contains(cell.coord)) continue;
            MapCells current = cell;
            List<Vector3> path = new List<Vector3>();
            path.Add(cell.coord);
            MapCells dhNeighbor = Erosion.downHillNeighbor(cell, cellMap);
            while(current.height > waterLevel)
            {
                if(dhNeighbor.height <= waterLevel)
                {
                    path.Add(sharedVertex(current,dhNeighbor));
                }
                else path.Add(dhNeighbor.coord);
                current = dhNeighbor;
                dhNeighbor = Erosion.downHillNeighbor(dhNeighbor, cellMap);
                if(addedCells.Contains(dhNeighbor.coord)) 
                {
                    path.Add(dhNeighbor.coord);
                    break;
                }
            }
            if(path.Count > 5)
            {
                rivers.Add(path);
                addedCells.AddRange(path);
            }
        }
        return rivers;
    }

    private Vector3 sharedVertex(MapCells cellA, MapCells cellB)
    {
        IEnumerable<Vector3> sharedPoint =  cellA.vertices.Intersect(cellB.vertices);
        return (sharedPoint.First() + sharedPoint.Last()) / 2f ;
    }
}
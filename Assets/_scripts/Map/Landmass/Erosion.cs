using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Erosion
{

    public static List<Vector3> DownhillPath(MapCells cell, Dictionary<Vector3, MapCells> cellMap, float waterLevel)
    {
        List<Vector3> path = new List<Vector3>();
        MapCells dhNeighbor = downHillNeighbor(cell, cellMap);
        path.Add(cell.coord);
        while(dhNeighbor.height > waterLevel)
        {
            path.Add(dhNeighbor.coord);
            dhNeighbor = downHillNeighbor(dhNeighbor, cellMap);
            if(path.Contains(dhNeighbor.coord)) break;
        }
        path.Add(dhNeighbor.coord);
        return path;
    }

    public static MapCells downHillNeighbor(MapCells cell, Dictionary<Vector3, MapCells> cellMap)
    {
        float lowestHeight  = 100f;
        MapCells lowestNeighbor = new MapCells();
        foreach(Vector3 neighborPoint in cell.neighbors)
        {
            MapCells neighbor = cellMap[neighborPoint];
            if(neighbor.height < lowestHeight)
            {
                lowestNeighbor = neighbor;
                lowestHeight = neighbor.height;
            }
        }
        return lowestNeighbor;
    }

    public static void setSlope(Dictionary<Vector3, MapCells> cellMap)
    {
        List<MapCells> cells = cellMap.Values.ToList();
        cells = cells.Where( cell => cell.height > 0).ToList();
        foreach(MapCells cell in cells)
        {
            MapCells dhNeighbor = downHillNeighbor(cell, cellMap);
            float distance = Vector3.Distance(cell.coord, dhNeighbor.coord);
            float heightDifference = cell.height - dhNeighbor.height;
            cell.slope = heightDifference / distance;
        }
    }

    public static float setFlux(Dictionary<Vector3, MapCells> cellMap, float waterLevel)
    {
        float maxFlux = 0f;
        List<MapCells> cells = cellMap.Values.ToList();
        cells = cells.Where( cell => cell.height > waterLevel).ToList();
        cells = cells.OrderByDescending(cell => cell.height).ToList();
        foreach(MapCells cell in cells)
        {
            MapCells dhNeighbor = downHillNeighbor(cell, cellMap);
            dhNeighbor.flux += cell.flux;
            if(dhNeighbor.flux > maxFlux) maxFlux = dhNeighbor.flux;
        }
        return maxFlux;
    }

    public static void fillSinks(Dictionary<Vector3, MapCells> cellMap, Dictionary<Vector3, float> vertexHeightMap)
    {
        List<MapCells> cells = cellMap.Values.ToList();
        cells = cells.Where( cell => cell.height > 0).ToList();
        cells = cells.OrderByDescending(cell => cell.height).ToList();
        bool keepFilling = true;
        while(keepFilling)
        {
            keepFilling = false;
            foreach(MapCells cell in cells)
            {
                MapCells dhNeighbor = downHillNeighbor(cell, cellMap);
                if(cell.height <= dhNeighbor.height)
                {
                    //cell.height = dhNeighbor.height + 0.01f;
                    keepFilling = true;
                    float hDiff = (dhNeighbor.height - cell.height) + 0.01f;
                    cell.height += hDiff;
                    vertexHeightMap[cell.coord] += hDiff;
                    foreach(Vector3 vertex in cell.vertices)
                    {
                        vertexHeightMap[vertex] += hDiff;
                    }
                }
            }
        }
    }

    public static void setErosionRate(Dictionary<Vector3, MapCells> cellMap)
    {
        List<MapCells> cells = cellMap.Values.ToList();
        cells = cells.Where( cell => cell.height > 0).ToList();
        foreach(MapCells cell in cells)
        {
            var river = Mathf.Sqrt(cell.flux) * cell.slope;
            var creep = cell.slope * cell.slope;
            var total = 1000 * river + creep;
            total = Mathf.Clamp(total, 0, 200);
            cell.erosionRate = total;
        }
    }

    public static void erode(Dictionary<Vector3, MapCells> cellMap, Dictionary<Vector3, float> vertexHeightMap, 
                             float amount, float waterLevel)
    {
        List<MapCells> cells = cellMap.Values.ToList();
        cells = cells.Where( cell => cell.height > 0).ToList();
        float maxRate = cells.Max(x => x.erosionRate);
        foreach(MapCells cell in cells)
        {
            float erodeAmount = (cell.height - amount * (cell.erosionRate / maxRate)) <= waterLevel * 0.8 ? 
                                 0 : amount * (cell.erosionRate / maxRate);

            cell.height = cell.height - erodeAmount;
            vertexHeightMap[cell.coord] -= erodeAmount;
            foreach(Vector3 vertex in cell.vertices)
            {
                vertexHeightMap[vertex] -= erodeAmount;
            }
        }
    }

    public static void doErosion(Dictionary<Vector3, MapCells> cellMap, Dictionary<Vector3, float> vertexHeightMap,
                                    float amount, float waterLevel, int n) {
        for (int i = 0; i < n; i++)
        {
            erode(cellMap, vertexHeightMap, amount, waterLevel);
            fillSinks(cellMap, vertexHeightMap);
        }
    }
}
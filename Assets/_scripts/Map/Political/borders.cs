using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Region
{
    int id;
    Settlement capitolCity;
    Color regionColor;

    public Region(int id, Settlement capitol, Vector2 point)
    {
        this.id = id;
        this.capitolCity = capitol;
        this.regionColor = GetColor(point);
    }

    private void GetColor(Vector2 capitolLoc)
    {
        MapCells capitol = VoronoiGenerator.Instance.GetCellAtPoint(capitolLoc);
        Color color = new Color(capitol.temperature, capitol.height, capitol.precipitation. 1f);
        return color;
    }
}

public class BorderGenerator 
{
    private List<Region> regions;
    public int regioncount;

    public void SetCapitols(Dictionary<Vector3, MapCells> cellMap)
    {
        regions = new List<Region>();

        List<MapCells> cells = cellMap.Values.ToList();
        cells = cells.Where( cell =>  cell.cityScore > 0).OrderByDescending(cell => cell.cityScore).ToList();
        for (int i = 0; i < regioncount; i++)
        {
            Settlement capitol = cells[i].city;
            Region region = new Region(i, capitol, cells[i].coord);
        }

        
    }
}


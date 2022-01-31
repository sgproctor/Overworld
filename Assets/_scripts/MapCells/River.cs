using UnityEngine;
using System.Collections.Generic;

public class River : MonoBehaviour{
    // --------------public variables--------------
    public int riverID;

    // --------------serialized variables-------------- 
    [SerializeField] List<RiverSegment> river;
    [SerializeField] List<Vector3> pointsArray;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Gradient waterColor;
    [SerializeField] Material lineMaterial;
    [SerializeField] float totalFlux;
    [SerializeField] float widthMultiplier = 0f;
    [SerializeField] float maxRivertoMaxWaterLevel = 5f;
    [SerializeField] float riverZheight = -0.1f;
    [SerializeField] float maxRiverWidth;
    [SerializeField] float fluxToPrecipitationMultiplier;

    // --------------private variables-------------- 
    private float curvedLineSegmentSize = 0.01f;

    private void Awake() {
        river = new List<RiverSegment>();
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        maxRiverWidth =  MapGenerator.Instance.generationMinDistance / 3f;
        widthMultiplier = (maxRiverWidth / MapGenerator.Instance.weatherManager.maxWaterLevel) / maxRivertoMaxWaterLevel;
        // the more cells the larger the rivers can get (lower generation size equates to more cells)
        fluxToPrecipitationMultiplier = MapGenerator.Instance.generationMinDistance;
        
    }

    public void drawRiver()
    {
        lineRenderer.startWidth = 0f;
        lineRenderer.endWidth = Mathf.Min(widthMultiplier * totalFlux, maxRiverWidth);
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0, 0);
        float flux = 0f;

        for(int i = 0; i < river.Count; i++)
        {
            flux += river[i].waterFlux;
            pointsArray.Add(river[i].pointA);
            curve.AddKey(((float)i+1f)/river.Count,  Mathf.Min(widthMultiplier * flux, maxRiverWidth));
        }

        lineRenderer.widthCurve = curve;
        Vector3[] smoothedLine = LineSmoother.SmoothLine(pointsArray.ToArray(),curvedLineSegmentSize);
        lineRenderer.positionCount =smoothedLine.Length;
        
        lineRenderer.SetPositions(smoothedLine);

        lineRenderer.colorGradient = waterColor;
        lineRenderer.material = lineMaterial;
    }

    public void createRiverSegment(MapCells cell)
    {
        cell.river = gameObject.GetComponent<River>();
        if(cell.neightbors.Count > 0)
        {
            float minHeight = cell.height;
            MapCells lowestNeighbor = cell.neightbors[0];
            

            foreach(MapCells neighbor in cell.neightbors)
            {
                
                if(neighbor.height < minHeight)
                {
                    lowestNeighbor = neighbor;
                    minHeight = neighbor.height;
                }
            }

            RiverSegment segment = new RiverSegment();
            segment.pointA =  new Vector3(cell.coord.x, cell.coord.y, riverZheight);
            segment.waterFlux = cell.precipitation;
            totalFlux += segment.waterFlux;
            cell.precipitation += totalFlux * fluxToPrecipitationMultiplier;
            
            //if next to ocean dump river
            if(cell.isCoastCell && river.Count > 0)
            {
                Vector2 toOceanVect = cell.getVectorToOcean();
                segment.pointB = new Vector3(cell.coord.x,cell.coord.y, riverZheight);
                river.Add(segment);

                RiverSegment lastSegment = new RiverSegment();
                Vector2 vectToSea = new Vector2(cell.coord.x+toOceanVect.x,cell.coord.y+toOceanVect.y);
                Vector2 beachPoint = new Vector2();
                foreach(MapEdge edge in cell.edges)
                {
                    if(LineSegmentsIntersection.Math2d.LineSegmentsIntersection(cell.coord, vectToSea, edge.pointA, edge.pointB, out beachPoint))
                    {
                        break;
                    }
                }
                lastSegment.pointA = new Vector3(beachPoint.x,beachPoint.y, riverZheight);
                lastSegment.pointB = Vector3.zero;
                
                river.Add(lastSegment);
                drawRiver();     
            }

            //if connecting cell
            else if((minHeight > MapGenerator.Instance.oceanHeight) && (minHeight < cell.height) && (!lowestNeighbor.river))
            {         
                segment.pointB = new Vector3(lowestNeighbor.coord.x, lowestNeighbor.coord.y, riverZheight);
                river.Add(segment);
                createRiverSegment(lowestNeighbor);
            }
            //if feeding into another river
            else{
                segment.pointB = new Vector3(lowestNeighbor.coord.x,lowestNeighbor.coord.y, riverZheight);
                river.Add(segment);
                
                RiverSegment lastSegment = new RiverSegment();
                lastSegment.pointA = new Vector3(lowestNeighbor.coord.x,lowestNeighbor.coord.y, riverZheight);
                lastSegment.pointB = Vector3.zero;
                river.Add(lastSegment);

                //lowestNeighbor.river.totalFlux += totalFlux;
                drawRiver();

            }
        }
    }
}

public class RiverSegment{
    public Vector3 pointA;
    public Vector3 pointB;
    public float waterFlux;
}
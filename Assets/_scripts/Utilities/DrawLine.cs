using UnityEngine;
using System.Linq;

public class LineDrawer : MonoBehaviour 
{
    public static void CreateDynamicLine(Transform container, string name, Vector3[] points, Color color, float[] widths, 
                                    float multiplier, float segmentSize, Material lineMaterial)
    {
        var lineGameObject = new GameObject(name);
        lineGameObject.transform.parent = container;
        var lineRenderer = lineGameObject.AddComponent<LineRenderer>();
        AnimationCurve curve = new AnimationCurve();

        Vector3[] smoothedLine = LineSmoother.SmoothLine(points.ToArray(),segmentSize);
        lineRenderer.positionCount = smoothedLine.Length;
        lineRenderer.SetPositions(smoothedLine);
        

        for (int i = 0; i < widths.Count(); i++)
        {
            curve.AddKey((float)i / (widths.Count()), widths[i]);
        }

        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.widthCurve = curve;
        lineRenderer.widthMultiplier = multiplier;
    }

    public static void CreateLine(Transform container, string name, Vector3[] points, Color color, 
                            float generationSize, Material lineMaterial, float startWidth = 0f, 
                            float endWidth = 0f, bool smooth = false, int order = 1)
    {
        var lineGameObject = new GameObject(name);
        lineGameObject.transform.parent = container;
        var lineRenderer = lineGameObject.AddComponent<LineRenderer>();

        if(smooth)
        {
            Vector3[] smoothedLine = LineSmoother.SmoothLine(points.ToArray(),generationSize*0.1f);
            lineRenderer.positionCount =smoothedLine.Length;
            lineRenderer.SetPositions(smoothedLine);
        }
        else
        {
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }

        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.sortingOrder = order;
    }
}
using UnityEngine;
public class VoronoiEdge
{
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 rightPoint;
    public Vector3 leftPoint;

    public VoronoiEdge(Vector3 pointA, Vector3 pointB, Vector3 right, Vector3 left)
    {
        this.pointA = pointA;
        this.pointB = pointB;
        this.rightPoint = right;
        this.leftPoint = left;
    }
}
// using UnityEngine;
// using System;
// using System.Linq;
// using System.Collections.Generic;



// public static class Vector3Extension
// {
//     public static Vector2[] toVector2Array (this Vector3[] v3)
//     {
//         return System.Array.ConvertAll<Vector3, Vector2> (v3, getV3fromV2);
//     }
        
//     public static Vector2 getV3fromV2 (Vector3 v3)
//     {
//         return new Vector2 (v3.x, v3.y);
//     }
// }

// public static class FloodFillExtension
// {

//     private static bool validPoint(Vector2 point, float width, float height)
//     {
//         if(point.x < 0 || point.x > width) return false;
//         if(point.y < 0 || point.y > height) return false;
//         return true;
//     }

//     public static void fillToBounds(Vector2 startingPoint, MapEdge[] boundingEdges, float mapWidth, float mapHeight, out List<Vector2> filledVectors)
//     {
// 		Queue<Vector2> pointQueue = new Queue<Vector2>();
// 		filledVectors = new List<Vector2>();

// 		pointQueue.Enqueue(startingPoint);
//         filledVectors.Add(startingPoint);
//         Vector2 currentPoint = Vector2.zero;
        

// 		while(pointQueue.Count > 0){
//             currentPoint = pointQueue.Dequeue();
			
//             Vector2 neighborVector = Vector2.zero;
            
//             //left cell
//             neighborVector = new Vector2(currentPoint.x - 1, currentPoint.y);
//             if(validPoint(neighborVector, mapWidth, mapHeight) 
//                 && !filledVectors.Contains(neighborVector) 
//                 && PolygonExtension.pointInPolygon(neighborVector, boundingEdges))
//             {
//                 pointQueue.Enqueue(neighborVector);
//                 filledVectors.Add(neighborVector);
//             }

//             //right cell
//             neighborVector = new Vector2(currentPoint.x + 1, currentPoint.y);
//             if(validPoint(neighborVector, mapWidth, mapHeight) 
//                 && !filledVectors.Contains(neighborVector) 
//                 && PolygonExtension.pointInPolygon(neighborVector, boundingEdges))
//             {
//                 pointQueue.Enqueue(neighborVector);
//                 filledVectors.Add(neighborVector);
//             }

//             //cell above
//             neighborVector = new Vector2(currentPoint.x, currentPoint.y + 1);
//             if(validPoint(neighborVector, mapWidth, mapHeight) 
//                 && !filledVectors.Contains(neighborVector) 
//                 && PolygonExtension.pointInPolygon(neighborVector, boundingEdges))
//             {
//                 pointQueue.Enqueue(neighborVector);
//                 filledVectors.Add(neighborVector);
//             }

//             //cell below
//             neighborVector = new Vector2(currentPoint.x, currentPoint.y - 1);
//             if(validPoint(neighborVector, mapWidth, mapHeight) 
//                 && !filledVectors.Contains(neighborVector) 
//                 && PolygonExtension.pointInPolygon(neighborVector, boundingEdges))
//             {
//                 pointQueue.Enqueue(neighborVector);
//                 filledVectors.Add(neighborVector);
//             }
            
//         }
//     }
// }

// public static class PolygonExtension
// {
//     public static bool pointInPolygon(Vector2 point, MapEdge[] boundingEdges)
//     {
//         if(boundingEdges.Length < 3) return false;
//         int intersectCount = 0;
//         foreach(MapEdge edge in boundingEdges)
//         {
//             // only check intersection is point is below one edge and above the other, and on the left side
//             if(point.x >= edge.pointA.x && point.x >= edge.pointB.x ) continue;
//             if(point.y >= edge.pointA.y && point.y >= edge.pointB.y ) continue;
//             if(point.y <= edge.pointA.y && point.y <= edge.pointB.y ) continue;

//             bool doesIntersect = false;
//             float rightMostEdgePXVal = Mathf.Max(edge.pointA.x, edge.pointB.x);
//             Vector2 rightPoint = new Vector2(rightMostEdgePXVal + 10f, point.y);
//             Vector2 intersectPoint = GetIntersectionPointCoordinates(point, rightPoint, edge.pointA, edge.pointB, out doesIntersect);

//             if(!doesIntersect) continue;
//             if(intersectPoint.x > point.x) intersectCount++;
//         }
//         if(intersectCount==1) return true;
//         return false;
//     }

//     public static Vector2 GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out bool found)
//     {
//         float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);
    
//         if (tmp == 0)
//         {
//             // No solution!
//             found = false;
//             return Vector2.zero;
//         }
    
//         float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;
    
//         found = true;
    
//         return new Vector2(
//             B1.x + (B2.x - B1.x) * mu,
//             B1.y + (B2.y - B1.y) * mu
//         );
//     }
// }
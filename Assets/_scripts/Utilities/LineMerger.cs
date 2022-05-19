using UnityEngine;
using System;
using System.Linq;
using DelaunatorSharp;
using System.Collections.Generic;
using DelaunatorSharp.Unity.Extensions;

public class LineMerger
{
    public static List<List<Vector3>> mergeLineSegments(List<Vector3> lineSegments)    
    {
        List<List<Vector3>> mergedLineSegments = new List<List<Vector3>>();
        for (int i = 0; i < lineSegments.Count-1; i+=2)
        {
            Vector3 leftPoint = lineSegments[i];
            Vector3 rightPoint = lineSegments[i+1];

            if(mergedLineSegments.Count() == 0)
            {
                mergedLineSegments.Add(new List<Vector3>{leftPoint, rightPoint});
                continue;
            }

            bool inList = false;
            int LeftListIndex = -1;
            int RightListIndex = -1;
            
            for (int j = 0; j < mergedLineSegments.Count; j++)
            {
                if(mergedLineSegments[j].Contains(leftPoint)) 
                {
                    LeftListIndex = j;
                    inList = true;
                }
                if(mergedLineSegments[j].Contains(rightPoint))
                {
                    RightListIndex = j;
                    inList = true;
                }
            }

            if(!inList)
            {
                mergedLineSegments.Add(new List<Vector3>{leftPoint, rightPoint});
                continue;
            }

            // only one point is in a list
            if(LeftListIndex != -1 && RightListIndex == -1)
            {
                int index = mergedLineSegments[LeftListIndex].IndexOf(leftPoint);
                if(index == 0) mergedLineSegments[LeftListIndex].Insert(0, rightPoint);
                else if((index == mergedLineSegments[LeftListIndex].Count - 1)) 
                {
                    mergedLineSegments[LeftListIndex].Add(rightPoint);
                }
                else Debug.Log("Point in middle of segment????");
            }
            else if(LeftListIndex == -1 && RightListIndex != -1)
            {
                int index = mergedLineSegments[RightListIndex].IndexOf(rightPoint);
                if(index == 0) mergedLineSegments[RightListIndex].Insert(0, leftPoint);
                else if((index == mergedLineSegments[RightListIndex].Count - 1)) 
                {
                    mergedLineSegments[RightListIndex].Add(leftPoint);
                }
                else Debug.Log("Point in middle of segment????");
            }

            // merging lists due to both points being in two different lists
            else if(LeftListIndex != -1 && RightListIndex != -1)
            {
                // if one list has a point at the back and the other list contains the point at the front
                int leftIndex = mergedLineSegments[LeftListIndex].IndexOf(leftPoint);
                int rightIndex = mergedLineSegments[RightListIndex].IndexOf(rightPoint);
                // if points are on the same segment (i.e. enclosing)
                if(LeftListIndex == RightListIndex)
                {
                    // if the rightpoint is the last point, then append the left point
                    if(rightIndex == mergedLineSegments[RightListIndex].Count - 1)
                    {
                        mergedLineSegments[RightListIndex].Add(leftPoint);
                    }
                    else
                    {
                        mergedLineSegments[RightListIndex].Add(rightPoint);
                    }
                }

                // left point is at front and right is back
                else if(leftIndex == 0 && rightIndex == mergedLineSegments[RightListIndex].Count - 1)
                {
                    // merge left list into right list
                    mergedLineSegments[RightListIndex].AddRange(mergedLineSegments[LeftListIndex]);
                    mergedLineSegments.RemoveAt(LeftListIndex);
                }
                // right point is at front and left is back
                else if(leftIndex == mergedLineSegments[LeftListIndex].Count - 1 && rightIndex == 0)
                {
                    mergedLineSegments[LeftListIndex].AddRange(mergedLineSegments[RightListIndex]);
                    mergedLineSegments.RemoveAt(RightListIndex);
                }
                // both points are at the front
                else if(leftIndex == 0 && rightIndex == 0)
                {
                    mergedLineSegments[LeftListIndex].Reverse();
                    mergedLineSegments[LeftListIndex].AddRange(mergedLineSegments[RightListIndex]);
                    mergedLineSegments.RemoveAt(RightListIndex);
                }
                // both points are at the back
                else if(leftIndex == mergedLineSegments[LeftListIndex].Count - 1 &&
                        rightIndex == mergedLineSegments[RightListIndex].Count - 1)
                {
                    mergedLineSegments[RightListIndex].Reverse();
                    mergedLineSegments[LeftListIndex].AddRange(mergedLineSegments[RightListIndex]);
                    mergedLineSegments.RemoveAt(RightListIndex);
                }
                else Debug.Log("Point in middle of segment????");
            }
        }
        return mergedLineSegments;
    }
}
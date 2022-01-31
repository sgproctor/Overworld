using UnityEngine;

public class QuadTree {
    private QuadTree northeastNode;
    private QuadTree southeastNode;
    private QuadTree northwestNode;
    private QuadTree southwestNode;
    private Vector2 point = new Vector2();
    private Object leafObject = null;
    private bool returnNull;

    public QuadTree()
    {
        //Initialize defualts
    }

    private QuadTree(Vector2 insertedPoint, Object insertedObject)
    {
        point = insertedPoint;
        leafObject = insertedObject;
    }

    public object getObjectAtPoint(Vector2 objectPoint)
    {

        if (objectPoint == point)
        {
            return leafObject;
        }
        else
        {
            Vector2 vectorDiff = objectPoint - point;
            float resultAngle = Vector2.SignedAngle(vectorDiff, Vector2.up);

            // northeast
            if (resultAngle > 0.0f && resultAngle <= 90.0f)
            {
                return northeastNode.getObjectAtPoint(objectPoint);
            }

            // southeast
            else if (resultAngle > 90.0f && resultAngle <= 180.0f)
            {
                return southeastNode.getObjectAtPoint(objectPoint);
            }

            // southwest
            else if (resultAngle > -180.0f && resultAngle <= -90.0f)
            {
                return southwestNode.getObjectAtPoint(objectPoint);
            }

            // northwest
            else
            {
                return northwestNode.getObjectAtPoint(objectPoint);
            } 
        }
    }

    public (object, bool) getClosestObjectAtPoint(Vector2 objectPoint)
    {
        //Debug.Log("--------------------------------------------------------------");
        MapCells tempCell = (MapCells)leafObject;
        
        // Debug.Log(string.Format("Object Point {0}, {1}", objectPoint.x, objectPoint.y));
        // Debug.Log(tempCell);
        // Debug.Log(string.Format("{0} -- does it contain", tempCell.mapCollider.bounds.Contains(objectPoint)));
        // Debug.Log(tempCell.mapCollider.bounds.extents.magnitude);
        // Debug.Log(string.Format("Bound Point {0}, {1}",tempCell.mapCollider.bounds.center.x, tempCell.mapCollider.bounds.center.y));
        if(tempCell.mapCollider.OverlapPoint(objectPoint))
        {
            return (leafObject, false);
        }
        else
        {
            Vector2 vectorDiff = objectPoint - point;
            float resultAngle = Vector2.SignedAngle(vectorDiff, Vector2.up);

            // northeast
            if (resultAngle > 0.0f && resultAngle <= 90.0f)
            {
                if(northeastNode != null)
                {
                    return northeastNode.getClosestObjectAtPoint(objectPoint);
                }
                else
                {
                    return (new object(), true);
                }
            }

            // southeast
            else if (resultAngle > 90.0f && resultAngle <= 180.0f)
            {
                if(southeastNode != null)
                {
                    return southeastNode.getClosestObjectAtPoint(objectPoint);
                }
                else
                {
                    return (new object(), true);
                }
            }

            // southwest
            else if (resultAngle > -180.0f && resultAngle <= -90.0f)
            {
                if(southwestNode != null)
                {
                    return southwestNode.getClosestObjectAtPoint(objectPoint);
                }
                else
                {
                    return (new object(),true);
                }
            }

            // northwest
            else
            {
                if(northwestNode != null)
                {
                    return northwestNode.getClosestObjectAtPoint(objectPoint);
                }
                else
                {
                    return (new object(),true);
                }
            } 
        }
    }

    public void insert(Vector2 insertedPoint, Object insertedObject)
    {
        if (leafObject)
        {
            Vector2 vectorDiff = insertedPoint - point;
            float resultAngle = Vector2.SignedAngle(vectorDiff, Vector2.up);

            // northeast
            if (resultAngle > 0.0f && resultAngle <= 90.0f)
            {
                if (northeastNode == null)
                {
                    northeastNode = new QuadTree(insertedPoint, insertedObject);
                }
                else
                {
                    northeastNode.insert(insertedPoint, insertedObject);
                }
            }

            // southeast
            else if (resultAngle > 90.0f && resultAngle <= 180.0f)
            {
                if (southeastNode == null)
                {
                    southeastNode = new QuadTree(insertedPoint, insertedObject);
                }
                else
                {
                    southeastNode.insert(insertedPoint, insertedObject);
                }
            }

            // southwest
            else if (resultAngle > -180.0f && resultAngle <= -90.0f)
            {
                if (southwestNode == null)
                {
                    southwestNode = new QuadTree(insertedPoint, insertedObject);
                }
                else
                {
                    southwestNode.insert(insertedPoint, insertedObject);
                }
            }

            // northwest
            else
            {
                if (northwestNode == null)
                {
                    northwestNode = new QuadTree(insertedPoint, insertedObject);
                }
                else
                {
                    northwestNode.insert(insertedPoint, insertedObject);
                }
            }     
        }
        else
        {
            point = insertedPoint;
            leafObject = insertedObject;
        }
    }
}
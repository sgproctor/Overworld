using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovement : MonoBehaviour
{
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private float zoomMod;
    private Vector3 dragOrigin;

    

    private float width;
    private float height;

    void Start()
    {
        width = VoronoiGenerator.Instance.width;
        height = VoronoiGenerator.Instance.height;
    }

    void Update()
    {
        PanCamera();
    }

    private void PanCamera()
    {
        if(Input.GetMouseButtonDown(1) || (Input.GetKeyDown(KeyCode.Mouse2)))
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        
        if(Input.GetMouseButton(1) || (Input.GetKey(KeyCode.Mouse2)))
        {
            float camSize = cam.orthographicSize;
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            float xDiff = Mathf.Clamp(cam.transform.position.x + difference.x, camSize/2f, width-camSize/2f );
            float yDiff = Mathf.Clamp(cam.transform.position.y + difference.y, camSize/2f, height-camSize/2f );
            cam.transform.position = new Vector3(xDiff, yDiff, cam.transform.position.z);
        }
        if(Input.mouseScrollDelta.y >= 1 || Input.mouseScrollDelta.y <= -1)
        {
            cam.orthographicSize -= Input.mouseScrollDelta.y * zoomMod;
        }
    }
}

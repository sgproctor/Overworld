using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(MapDisplayManager))]
public class DisplayEditor : Editor {

    MapDisplayManager mapDisplayManager;
    Editor displayEditor;

	public override void OnInspectorGUI()
	{
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                mapDisplayManager = GameObject.Find("MapDisplayManager").GetComponent<MapDisplayManager>();
                mapDisplayManager.UpdateDisplay();
            }
        }
	}
    // void Start()
    // {
    //     mapDisplayManager = GameObject.Find("MapDisplayManager").GetComponent<MapDisplayManager>();
    // }

    // public override void OnInspectorGUI () {
    //     DrawDefaultInspector ();
    //     if (GUILayout.Button ("Update Map Colors")) {
    //         mapDisplayManager = GameObject.Find("MapDisplayManager").GetComponent<MapDisplayManager>();
    //         mapDisplayManager.UpdateDisplay();
    //         //VoronoiGenerator.Instance.UpdateDisplay();
    //     }
    // }
}
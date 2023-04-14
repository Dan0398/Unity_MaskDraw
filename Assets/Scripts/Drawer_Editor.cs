#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
 
[CustomEditor(typeof(Drawer))]
[ExecuteAlways]
[System.Serializable]
public class DrawerEditor : Editor
{
    Drawer Target;
    [SerializeField] public bool RequireToDrawInEditMode;
    [SerializeField] public bool IsLeftMousePressed;
    Tool OldSelectedTool;
    int OldHotControl;
    
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var Required = EditorGUILayout.Toggle("Draw in edit mode", RequireToDrawInEditMode);
        if (Required != RequireToDrawInEditMode)
        {
            RequireToDrawInEditMode = Required;
            ActiveEditorTracker.sharedTracker.isLocked = Required;
            if (Required)
            {
                OldSelectedTool = Tools.current;
                Tools.current = Tool.None;
                OldHotControl = EditorGUIUtility.hotControl;
            }
            else 
            {
                Tools.current = OldSelectedTool;
                EditorGUIUtility.hotControl = OldHotControl;
            }
        }
    }
    
    void OnSceneGUI()
    {
        if (Target == null) Target = (Drawer)target;
        if (!RequireToDrawInEditMode) return;
        if (!Target.AllowToDraw()) return;
        var Ev = Event.current;
        if (Ev != null)
        {
            if (Ev.isMouse)
            {
                if (Ev.type == EventType.MouseUp)
                {
                    IsLeftMousePressed = false;
                    return;
                }
                else if (Ev.type == EventType.MouseDown && Ev.button == 0)
                {
                    IsLeftMousePressed = true;
                }
            }
        }
        if (IsLeftMousePressed)
        {
            var ScreenRay = HandleUtility.GUIPointToWorldRay(Ev.mousePosition);
            if (Physics.Raycast(ScreenRay,out RaycastHit hit, 100))
            {
                var Mask = hit.collider.GetComponent<Renderer>();
                if (Mask != null && Mask == Target.TargetMesh)
                {
                    EditorGUIUtility.hotControl = -1;
                    Target.ProcessDraw(hit, ScreenRay.direction, SceneView.currentDrawingSceneView.camera.transform.up);
                    Repaint();
                }
            }
        }
    }
}
#endif
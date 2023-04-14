#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
 
[CustomEditor(typeof(Drawer))]
[ExecuteAlways]
[System.Serializable]
public partial class DrawerEditor : Editor
{
    Drawer DrawerComponent;
    [SerializeField] public bool RequireToDrawInEditMode;
    [SerializeField] public bool IsLeftMousePressed;
    Transform SceneViewTransform;
    Tool OldSelectedTool;
    int OldHotControl;
    
    public override void OnInspectorGUI()
    {
        DrawInspector();
        if (!ReadyToDraw) return;
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
        if (DrawerComponent == null) DrawerComponent = (Drawer)target;
        if (!RequireToDrawInEditMode) return;
        if (!DrawerComponent.AllowToDraw()) return;
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
                if (Mask != null && Mask == DrawerComponent.TargetMesh)
                {
                    EditorGUIUtility.hotControl = -1;
                    if (SceneViewTransform == null)
                    {
                        SceneViewTransform = SceneView.currentDrawingSceneView.camera.transform;
                    }
                    DrawerComponent.ProcessDraw(hit, ScreenRay.direction, SceneViewTransform.up);
                    Repaint();
                }
            }
        }
    }
}
#endif
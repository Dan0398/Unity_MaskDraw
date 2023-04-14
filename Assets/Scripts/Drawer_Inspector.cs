#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
 
public partial class DrawerEditor : Editor
{
    bool UnwrappedSetupWindow = true;
    bool ReadyToDraw, ShadersReady, MeshReady, MaskReady;
    
    void DrawInspector()
    {
        if (DrawerComponent == null) DrawerComponent = (Drawer)target;
        ProcessStatus();
        DrawPreparingPart();
        DrawProjectionPart();
        DrawPalettePart();
        serializedObject.ApplyModifiedProperties();
    }
    
    void ProcessStatus()
    {
        ShadersReady = DrawerComponent.UVShader != null && DrawerComponent.RenderTextureShader != null;
        MeshReady = DrawerComponent.TargetMesh != null;
        MaskReady = DrawerComponent.Mask != null;
        ReadyToDraw = ShadersReady && MeshReady && MaskReady;
    }
    
    void DrawPreparingPart()
    {
        if (!ReadyToDraw)
        {
            EditorGUILayout.LabelField("Component not ready");
            if (!ShadersReady) EditorGUILayout.LabelField("Setup shaders");
            else if (!MeshReady) EditorGUILayout.LabelField("Setup mesh");
            else if (!MeshReady) EditorGUILayout.LabelField("Setup mesh");
            else if (!MaskReady) EditorGUILayout.LabelField("Setup mask");
        }
        if (ReadyToDraw)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            UnwrappedSetupWindow = EditorGUILayout.BeginFoldoutHeaderGroup(UnwrappedSetupWindow, "Require To work");
        }
        if (!ReadyToDraw || UnwrappedSetupWindow)
        {
            DrawShaderFields();
            if (!ShadersReady) return;
            DrawMeshField();
            if (!MeshReady) return;
            DrawMaskField();
            if (!MaskReady) return;
            if (UnwrappedSetupWindow) DrawExportButton(); 
        }
        if (ReadyToDraw)
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        
        void DrawShaderFields()
        {
            DrawerComponent.UVShader = (Shader)EditorGUILayout.ObjectField("UV Projection Shader",DrawerComponent.UVShader, typeof(Shader), true);
            DrawerComponent.RenderTextureShader = (Shader)EditorGUILayout.ObjectField("RenderTexture Shader", DrawerComponent.RenderTextureShader, typeof(Shader), true);
        }
        
        void DrawMeshField()
        {
            DrawerComponent.TargetMesh = (Renderer)EditorGUILayout.ObjectField("Mesh for draw",DrawerComponent.TargetMesh, typeof(Renderer), true);
        }
        
        void DrawMaskField()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Mask"));
        }
        
        void DrawExportButton()
        {
            if (!ReadyToDraw) return;
            if (GUILayout.Button("Export as .png"))
            {
                var Path = EditorUtility.SaveFilePanelInProject("Save mask as ready image", "Mask", "png", "Save");
                var Size = DrawerComponent.Mask.width;
                Texture2D tex = new Texture2D(Size, Size, TextureFormat.RGB24, false);
                // ReadPixels looks at the active RenderTexture.
                RenderTexture.active = DrawerComponent.Mask;
                tex.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
                tex.Apply(false);
                System.IO.File.WriteAllBytes(Path,tex.EncodeToPNG());
                Debug.Log("Export Done!");
                Object.DestroyImmediate(tex, true);
            }
        }
    }
    
    void DrawProjectionPart()
    {
        if (!ReadyToDraw) return;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Projection");
        DrawOutStandingDirection();
        DrawProjectionType();
        DrawProjectionParams();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxLength"));
        EditorGUILayout.EndVertical();
        
        void DrawOutStandingDirection()
        {
            string Info = DrawerComponent.ProjectionUseLookDirection? "Look direction" : "Raycast normal";
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Outstanding direction");
            if (GUILayout.Button(Info))
            {
                DrawerComponent.ProjectionUseLookDirection = !DrawerComponent.ProjectionUseLookDirection;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        void DrawProjectionType()
        {
            string Info = DrawerComponent.ProjectionIsOrtho? "Orthographic" : "Perspective";
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Projection Type");
            if (GUILayout.Button(Info))
            {
                DrawerComponent.ProjectionIsOrtho = !DrawerComponent.ProjectionIsOrtho;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        void DrawProjectionParams()
        {
            if (DrawerComponent.ProjectionIsOrtho)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Size"));
            }
            else 
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FOV"));
            }
        }
    }
    
    void DrawPalettePart()
    {
        if (!ReadyToDraw) return;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("DrawMask"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("DrawColor"));
        DrawFastSwitchColorButtons();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("DrawSensitivity"));
        
        void DrawFastSwitchColorButtons()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Red"))    DrawerComponent.DrawColor = Color.red; 
            if (GUILayout.Button("Green"))  DrawerComponent.DrawColor = Color.green; 
            if (GUILayout.Button("Blue"))   DrawerComponent.DrawColor = Color.blue; 
            if (GUILayout.Button("Black"))  DrawerComponent.DrawColor = Color.black; 
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
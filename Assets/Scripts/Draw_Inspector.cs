#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
 
public partial class DrawEditor : Editor
{
    bool UnwrappedSetupWindow = true;
    bool ReadyToDraw, ShadersReady, MeshReady, MaskReady;
    
    void DrawInspector()
    {
        if (DrawComponent == null) DrawComponent = (Draw)target;
        ProcessStatus();
        DrawPreparingPart();
        DrawProjectionPart();
        DrawPalettePart();
        serializedObject.ApplyModifiedProperties();
    }
    
    void ProcessStatus()
    {
        ShadersReady = DrawComponent.UVShader != null && DrawComponent.RenderTextureShader != null;
        MeshReady = DrawComponent.TargetMesh != null;
        MaskReady = DrawComponent.Mask != null;
        ReadyToDraw = ShadersReady && MeshReady && MaskReady;
    }
    
    void DrawPreparingPart()
    {
        if (!ReadyToDraw)
        {
            EditorGUILayout.LabelField("Component not ready");
            if (!ShadersReady) EditorGUILayout.LabelField("Setup shaders");
            else if (!MeshReady) EditorGUILayout.LabelField("Setup mesh");
            else if (!MaskReady) EditorGUILayout.LabelField("Setup mask");
        }
        if (ReadyToDraw)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            UnwrappedSetupWindow = EditorGUILayout.BeginFoldoutHeaderGroup(UnwrappedSetupWindow, "Required To work");
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
            DrawComponent.UVShader = (Shader)EditorGUILayout.ObjectField("UV Projection Shader",DrawComponent.UVShader, typeof(Shader), true);
            DrawComponent.RenderTextureShader = (Shader)EditorGUILayout.ObjectField("RenderTexture Shader", DrawComponent.RenderTextureShader, typeof(Shader), true);
        }
        
        void DrawMeshField()
        {
            DrawComponent.TargetMesh = (Renderer)EditorGUILayout.ObjectField("Mesh for draw",DrawComponent.TargetMesh, typeof(Renderer), true);
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
                var Size = DrawComponent.Mask.width;
                Texture2D tex = new Texture2D(Size, Size, TextureFormat.RGB24, false);
                RenderTexture.active = DrawComponent.Mask;
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
            string Info = DrawComponent.ProjectionUseLookDirection? "Look direction" : "Raycast normal";
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Outstanding direction");
            if (GUILayout.Button(Info))
            {
                DrawComponent.ProjectionUseLookDirection = !DrawComponent.ProjectionUseLookDirection;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        void DrawProjectionType()
        {
            string Info = DrawComponent.ProjectionIsOrtho? "Orthographic" : "Perspective";
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Projection Type");
            if (GUILayout.Button(Info))
            {
                DrawComponent.ProjectionIsOrtho = !DrawComponent.ProjectionIsOrtho;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        void DrawProjectionParams()
        {
            if (DrawComponent.ProjectionIsOrtho)
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
            if (GUILayout.Button("Red"))    DrawComponent.DrawColor = Color.red; 
            if (GUILayout.Button("Green"))  DrawComponent.DrawColor = Color.green; 
            if (GUILayout.Button("Blue"))   DrawComponent.DrawColor = Color.blue; 
            if (GUILayout.Button("Black"))  DrawComponent.DrawColor = Color.black; 
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
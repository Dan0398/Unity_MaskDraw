using UnityEngine;
using UnityEngine.Rendering;

public class Drawer : MonoBehaviour
{
    [SerializeField] Shader UVShader, RenderTextureShader;
    [SerializeField] Texture2D[] DrawMasks;
    [SerializeField, Min(0)] int SelectedMaskNumber;
    
    [Header("Projection Params")]
    [SerializeField, Range(0.01f, 10)] float Size = 1;
    [SerializeField, Range(10, 90)] int FOV = 30;
    [SerializeField, Range(0.01f, 10)] float Length = 1;
    
    [Header("Draw Parameters")]
    [SerializeField] Color32 DrawColor = Color.red;
    [SerializeField, Range(0.0f, 1.0f)] float DrawSensitivity = 1;
    
    [Space(), SerializeField] Renderer TargetMesh;
    [SerializeField] CustomRenderTexture TextureForDraw;
    Material UVProjectorMat, MaterialForDraw;
    RenderTexture UVMaskProjector;
    Matrix4x4 ProjectionMatrix, ViewMatrix;
    CommandBuffer command;
    
    void Start()
    {
        UVProjectorMat = new Material(UVShader);
        UVMaskProjector = new RenderTexture(512,512,0, UnityEngine.Experimental.Rendering.DefaultFormat.LDR)
        {
            filterMode = FilterMode.Point
        };
        UVProjectorMat.SetFloat("Size", 512);
        MaterialForDraw = new Material(RenderTextureShader);
        TextureForDraw.material = MaterialForDraw;
        TextureForDraw.Initialize();
        command = new CommandBuffer();
        OnValidate();
    }
    
    void OnValidate()
    {
        ProjectionMatrix = Matrix4x4.Perspective(FOV, 1, 0, Length);
    }
    
    void Update()
    {
        TryRaycast();
    }
    
    bool AllowToDraw()
    {
        if (UVShader == null || RenderTextureShader == null)
        {
            Debug.LogError($"Selected Mask number invalid! Now is {SelectedMaskNumber}, required from 0 to {DrawMasks.Length-1}");
            return false;
        }
        if (SelectedMaskNumber < 0 ||SelectedMaskNumber >= DrawMasks.Length)
        {
            Debug.LogError($"Selected Mask number invalid! Now is {SelectedMaskNumber}, required from 0 to {DrawMasks.Length-1}");
            return false;
        }
        return true;
    }
    
    void TryRaycast()
    {
        if (Input.GetMouseButton(0))
        {
            if (!AllowToDraw()) return;
            var ScreenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ScreenRay,out RaycastHit hit, 100))
            {
                var Mask = hit.collider.GetComponent<Renderer>();
                if (Mask != null && Mask == TargetMesh)
                {
                    ProcessDraw(hit, ScreenRay.direction);
                }
            }
        }
    }
    
    public void ProcessDraw(RaycastHit Hit, Vector3 LookDirection)
    {
        var Position = Hit.point - LookDirection;
        var Rotation = Quaternion.LookRotation(LookDirection, Camera.main.transform.up);
        ViewMatrix = Matrix4x4.TRS(Position, Rotation, Vector3.one);
        DrawTexture();
    }
    
    void DrawTexture()
    {
        ProcessUVCoords();
        ProcessDraw();
        
        void ProcessUVCoords()
        {
            command.SetRenderTarget(UVMaskProjector);
            UVProjectorMat.SetVector("DrawDirection", ViewMatrix.MultiplyVector(Vector3.forward));
            UVProjectorMat.SetVector("DrawWorldPos", ViewMatrix.MultiplyPoint(Vector3.zero));
            UVProjectorMat.SetFloat("MaxDistance", Length);
            command.SetViewMatrix(ViewMatrix.inverse);
            command.SetProjectionMatrix(ProjectionMatrix);
            command.DrawRenderer(TargetMesh, UVProjectorMat, 0);
            
            Graphics.ExecuteCommandBuffer(command);
            command.Clear();
        }
        
        void ProcessDraw()
        {    
            if (TextureForDraw != null)
            {
                MaterialForDraw.SetTexture("MaskImage", GiveMeTextureForDraw());
                MaterialForDraw.SetTexture("UVMap", UVMaskProjector);
                MaterialForDraw.SetColor("DrawColor", DrawColor);
                MaterialForDraw.SetFloat("DrawSensitivity", DrawSensitivity);
                TextureForDraw.Update();
            }
            
            Texture2D GiveMeTextureForDraw()
            {
                return DrawMasks[SelectedMaskNumber];
            }
        }
    }
}
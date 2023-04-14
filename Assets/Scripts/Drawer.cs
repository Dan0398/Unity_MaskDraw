using UnityEngine;
using UnityEngine.Rendering;

public class Drawer : MonoBehaviour
{
    const float ProjectionOutstandingHeight = 0.2f;
    [System.Serializable] enum ProjectionType { Orthographic, Perspective}
    
    [SerializeField] Shader UVShader, RenderTextureShader;
    [Space(), SerializeField] public Renderer TargetMesh;
    [SerializeField] Texture2D DrawMask;
    
    [Header("Projection Params")]
    public bool ProjectionUseLookDirection;
    [SerializeField] ProjectionType Type;
    [SerializeField, Range(0.01f, 10)] float Size = 1;
    [SerializeField, Range(10, 90)] int FOV = 30;
    [SerializeField, Range(0.01f, 10)] float Length = 1;
    
    [Header("Draw Parameters")]
    [SerializeField] Color32 DrawColor = Color.red;
    [SerializeField, Range(0.0f, 1.0f)] float DrawSensitivity = 1;
    
    [SerializeField] CustomRenderTexture TextureForDraw;
    Camera PlaymodeCamera;
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
        if(Type == ProjectionType.Perspective)
        {
            ProjectionMatrix = Matrix4x4.Perspective(FOV, 1, 0, 100);
        }
        else 
        {
            var HalfSize = Size / 2f;
            ProjectionMatrix = Matrix4x4.Ortho(HalfSize,HalfSize,HalfSize,HalfSize,0,100);
        }
    }
    
    void Update()
    {
        TryRaycastToDraw();
    }
    
    void TryRaycastToDraw()
    {
        if (Input.GetMouseButton(0))
        {
            if (!AllowToDraw()) return;
            if (PlaymodeCamera == null)
            {
                PlaymodeCamera = Camera.main;
            }
            if (PlaymodeCamera == null)
            {
                Debug.LogError("Unable to find camera for projecting texture! Assign tag \"MainCamera\" to your camera and try again.");
                return;
            }
            var ScreenRay = PlaymodeCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ScreenRay,out RaycastHit hit, 100))
            {
                var Mask = hit.collider.GetComponent<Renderer>();
                if (Mask != null && Mask == TargetMesh)
                {
                    ProcessDraw(hit, ScreenRay.direction, PlaymodeCamera.transform.up);
                }
            }
        }
    }
    
    public bool AllowToDraw()
    {
        if (UVShader == null || RenderTextureShader == null)
        {
            Debug.LogError("Shaders are not assigned.");
            return false;
        }
        return true;
    }
    
    public void ProcessDraw(RaycastHit Hit, Vector3 LookDirection, Vector3 CameraUpVector)
    {
        var OutstandingVector = ProjectionUseLookDirection? LookDirection : (-Hit.normal);
        var Position = Hit.point - OutstandingVector * ProjectionOutstandingHeight;
        var Rotation = Quaternion.LookRotation(OutstandingVector, CameraUpVector);
        ViewMatrix = Matrix4x4.TRS(Position, Rotation, Vector3.one);
        DrawTexture();
    }
    
    void DrawTexture()
    {
        if (command == null) Start();
        ProcessUVCoords();
        ProcessDrawRoutine();
        
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
        
        void ProcessDrawRoutine()
        {    
            MaterialForDraw.SetTexture("MaskImage", DrawMask);
            MaterialForDraw.SetTexture("UVMap", UVMaskProjector);
            MaterialForDraw.SetColor("DrawColor", DrawColor);
            MaterialForDraw.SetFloat("DrawSensitivity", DrawSensitivity);
            TextureForDraw.Update();
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red - Color.black * 0.6f;
        Gizmos.DrawSphere(ViewMatrix.MultiplyPoint(Vector3.zero), 0.02f);
        if(Type == ProjectionType.Perspective)
        {
            var StartPoint = ViewMatrix.MultiplyPoint(Vector3.zero);
            var Angle = new Vector2(Mathf.Sin(FOV * Mathf.Deg2Rad), Mathf.Cos(FOV * Mathf.Deg2Rad))/2f;
            var LowLeft = ViewMatrix.MultiplyPoint(new Vector3(-Angle.x,  Angle.x, ProjectionOutstandingHeight));
            var LowRight = ViewMatrix.MultiplyPoint(new Vector3(Angle.x,  Angle.x, ProjectionOutstandingHeight));
            var UpLeft = ViewMatrix.MultiplyPoint(new Vector3( -Angle.x, -Angle.x, ProjectionOutstandingHeight));
            var UpRight = ViewMatrix.MultiplyPoint(new Vector3( Angle.x, -Angle.x, ProjectionOutstandingHeight));
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(LowRight, UpRight);
            Gizmos.DrawLine(LowLeft, LowRight);
            Gizmos.DrawLine(LowLeft, UpLeft);
            Gizmos.DrawLine(UpLeft, UpRight);
        }
    }
}
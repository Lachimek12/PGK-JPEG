using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraSetup : MonoBehaviour
{
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.3f, 0.47f, 1f);
    
    private Camera cam;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
    }
    
    private void Start()
    {
        if (setupOnStart)
        {
            SetupCamera();
        }
    }
    
    [ContextMenu("Setup Camera for 1920x1080")]
    public void SetupCamera()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("No camera found!");
                return;
            }
        }
        
        cam.orthographic = true;
        cam.backgroundColor = backgroundColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        
        cam.transform.position = new Vector3(0, 0, -10);
        cam.transform.rotation = Quaternion.identity;
        
        float targetAspect = referenceResolution.x / referenceResolution.y;
        float currentAspect = (float)Screen.width / Screen.height;
        
        if (currentAspect > targetAspect)
        {
            cam.orthographicSize = referenceResolution.y / 200f;
        }
        else
        {
            cam.orthographicSize = (referenceResolution.y / 200f) * (targetAspect / currentAspect);
        }
        
        Debug.Log($"Camera set up for {referenceResolution.x}x{referenceResolution.y} resolution. Orthographic Size: {cam.orthographicSize}, Position: {cam.transform.position}");
    }
}


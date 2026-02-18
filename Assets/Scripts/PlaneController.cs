using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{

    [Header("Rotation Settings")]
    public float rotationSpeed = 2f;
    private float moveInput;

    public Transform player;
    public float halfThickness = 1.5f;

    static readonly int ID_Plane = Shader.PropertyToID("_WorldSlicePlane");
    static readonly int ID_Thickness = Shader.PropertyToID("_SliceThickness");


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float rotation = rotationSpeed * moveInput;
        Vector3 euler = transform.eulerAngles;
        euler.y = transform.eulerAngles.y + rotation;
        transform.eulerAngles = euler;
    }

    void LateUpdate()
    {
        Vector3 normal = player.forward;
        float d = -Vector3.Dot(normal, player.position);

        Shader.SetGlobalVector(ID_Plane, new Vector4(normal.x, normal.y, normal.z, d));
        Shader.SetGlobalFloat(ID_Thickness, halfThickness);
    }

    public void OnRotateZ(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<float>();
    }
}

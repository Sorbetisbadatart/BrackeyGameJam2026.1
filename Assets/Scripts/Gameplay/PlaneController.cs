using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{

    [Header("Rotation Settings")]
    public float rotationSpeed = 2f;
    private float moveInput;

    public Transform player;
    public float sliceThickness = 0.2f;

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
        Vector3 normal = player.forward.normalized;
        float d = -Vector3.Dot(normal, player.position);

        Shader.SetGlobalVector(
            "_WorldSlicePlane",
            new Vector4(normal.x, normal.y, normal.z, d)
        );

        Shader.SetGlobalFloat("_SliceThickness", sliceThickness);
    }

    public void OnRotateZ(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<float>();
    }
}

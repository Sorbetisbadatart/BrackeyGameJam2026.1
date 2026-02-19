using UnityEngine;

// WorldSliceMeshBuilder
// Attach to a cube of any position and scale. Every frame, rebuilds the mesh
// as the intersection of the frustum and the cube in world space, with the
// third axis solved from the slice plane equation using whichever component
// of the plane normal is largest, to stay numerically stable at any rotation.
//
// Setup:
//   - Camera is a child of the Player, offset by -10 on local Z.
//   - sliceDepth = 10 so the slice sits right at the player.

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WorldSliceMeshBuilder : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The camera that follows the player. Auto-found if null.")]
    public Camera targetCamera;

    [Header("Slice Settings")]
    [Tooltip("Distance in front of the camera to sample the frustum cross-section.")]
    public float sliceDepth = 10f;

    MeshFilter _meshFilter;
    Mesh _mesh;

    float _minX, _maxX;
    float _minY, _maxY;
    float _minZ, _maxZ;

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();

        _mesh = new Mesh { name = "WorldSlice" };
        _mesh.MarkDynamic();
        _meshFilter.mesh = _mesh;

        Vector3 c = transform.position;
        float hw = transform.lossyScale.x * 0.5f;
        float hh = transform.lossyScale.y * 0.5f;
        float hd = transform.lossyScale.z * 0.5f;

        _minX = c.x - hw; _maxX = c.x + hw;
        _minY = c.y - hh; _maxY = c.y + hh;
        _minZ = c.z - hd; _maxZ = c.z + hd;

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;
        RebuildMesh();
    }

    void RebuildMesh()
    {
        Transform cam = targetCamera.transform;

        Vector3 planeOrigin = cam.position + cam.forward * sliceDepth;
        Vector3 planeNormal = cam.forward;

        // Pick the dominant axis of the plane normal to solve for.
        // This keeps the divisor as large as possible and avoids divide-by-zero
        // at any camera rotation (e.g. 90 deg Y where nz approaches 0).
        float absX = Mathf.Abs(planeNormal.x);
        float absY = Mathf.Abs(planeNormal.y);
        float absZ = Mathf.Abs(planeNormal.z);

        int dominantAxis; // 0 = X, 1 = Y, 2 = Z
        if (absX >= absY && absX >= absZ)
            dominantAxis = 0;
        else if (absY >= absX && absY >= absZ)
            dominantAxis = 1;
        else
            dominantAxis = 2;

        // Get the two free axes (the ones we intersect in) and the solve axis.
        // We always intersect the frustum and cube in the two free world axes,
        // then solve the third from the plane equation.
        int axis0, axis1; // free axes
        GetFreeAxes(dominantAxis, out axis0, out axis1);

        float freeMin0, freeMax0, freeMin1, freeMax1;
        GetCubeBounds(axis0, out freeMin0, out freeMax0);
        GetCubeBounds(axis1, out freeMin1, out freeMax1);

        // Frustum extents in the two free axes.
        Vector3 fBL = targetCamera.ViewportToWorldPoint(new Vector3(0, 0, sliceDepth));
        Vector3 fBR = targetCamera.ViewportToWorldPoint(new Vector3(1, 0, sliceDepth));
        Vector3 fTR = targetCamera.ViewportToWorldPoint(new Vector3(1, 1, sliceDepth));
        Vector3 fTL = targetCamera.ViewportToWorldPoint(new Vector3(0, 1, sliceDepth));

        float frustMin0 = Min4(GetAxis(fBL, axis0), GetAxis(fBR, axis0),
                               GetAxis(fTR, axis0), GetAxis(fTL, axis0));
        float frustMax0 = Max4(GetAxis(fBL, axis0), GetAxis(fBR, axis0),
                               GetAxis(fTR, axis0), GetAxis(fTL, axis0));
        float frustMin1 = Min4(GetAxis(fBL, axis1), GetAxis(fBR, axis1),
                               GetAxis(fTR, axis1), GetAxis(fTL, axis1));
        float frustMax1 = Max4(GetAxis(fBL, axis1), GetAxis(fBR, axis1),
                               GetAxis(fTR, axis1), GetAxis(fTL, axis1));

        // Intersect.
        float iMin0 = Mathf.Max(frustMin0, freeMin0);
        float iMax0 = Mathf.Min(frustMax0, freeMax0);
        float iMin1 = Mathf.Max(frustMin1, freeMin1);
        float iMax1 = Mathf.Min(frustMax1, freeMax1);

        if (iMin0 >= iMax0 || iMin1 >= iMax1)
        {
            _mesh.Clear();
            return;
        }

        // Build the four corners, solving the dominant axis from the plane equation.
        Vector3 wBL = SolveVertex(iMin0, iMin1, axis0, axis1, dominantAxis, planeOrigin, planeNormal);
        Vector3 wBR = SolveVertex(iMax0, iMin1, axis0, axis1, dominantAxis, planeOrigin, planeNormal);
        Vector3 wTR = SolveVertex(iMax0, iMax1, axis0, axis1, dominantAxis, planeOrigin, planeNormal);
        Vector3 wTL = SolveVertex(iMin0, iMax1, axis0, axis1, dominantAxis, planeOrigin, planeNormal);

        Matrix4x4 w2l = transform.worldToLocalMatrix;
        Vector3 vBL = w2l.MultiplyPoint3x4(wBL);
        Vector3 vBR = w2l.MultiplyPoint3x4(wBR);
        Vector3 vTR = w2l.MultiplyPoint3x4(wTR);
        Vector3 vTL = w2l.MultiplyPoint3x4(wTL);

        Vector3 localNormal = transform.InverseTransformDirection(-cam.forward).normalized;

        _mesh.Clear();

        _mesh.vertices = new Vector3[]
        {
            vBL, vBR, vTR, vTL,
            vBL, vBR, vTR, vTL,
        };

        _mesh.normals = new Vector3[]
        {
             localNormal,  localNormal,  localNormal,  localNormal,
            -localNormal, -localNormal, -localNormal, -localNormal,
        };

        _mesh.triangles = new int[]
        {
            0, 3, 2,
            0, 2, 1,
            4, 5, 6,
            4, 6, 7,
        };

        _mesh.uv = new Vector2[]
        {
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
        };

        _mesh.RecalculateBounds();
    }

    // Solves a world-space vertex given two free axis values and the plane equation.
    Vector3 SolveVertex(float v0, float v1,
                        int axis0, int axis1, int solveAxis,
                        Vector3 planeOrigin, Vector3 planeNormal)
    {
        // Plane equation: dot(p - planeOrigin, planeNormal) = 0
        // Expanded: n0*(a0 - o0) + n1*(a1 - o1) + ns*(as - os) = 0
        // Solve for as: as = os - (n0*(a0-o0) + n1*(a1-o1)) / ns

        float n0 = GetAxis(planeNormal, axis0);
        float n1 = GetAxis(planeNormal, axis1);
        float ns = GetAxis(planeNormal, solveAxis);
        float o0 = GetAxis(planeOrigin, axis0);
        float o1 = GetAxis(planeOrigin, axis1);
        float os = GetAxis(planeOrigin, solveAxis);

        float solvedValue = os - (n0 * (v0 - o0) + n1 * (v1 - o1)) / ns;

        // Clamp the solved axis to the cube's bounds on that axis.
        float cubeMin, cubeMax;
        GetCubeBounds(solveAxis, out cubeMin, out cubeMax);
        solvedValue = Mathf.Clamp(solvedValue, cubeMin, cubeMax);

        Vector3 result = Vector3.zero;
        SetAxis(ref result, axis0, v0);
        SetAxis(ref result, axis1, v1);
        SetAxis(ref result, solveAxis, solvedValue);
        return result;
    }

    void GetCubeBounds(int axis, out float min, out float max)
    {
        if (axis == 0) { min = _minX; max = _maxX; }
        else if (axis == 1) { min = _minY; max = _maxY; }
        else { min = _minZ; max = _maxZ; }
    }

    static void GetFreeAxes(int dominant, out int axis0, out int axis1)
    {
        if (dominant == 0) { axis0 = 1; axis1 = 2; }
        else if (dominant == 1) { axis0 = 0; axis1 = 2; }
        else { axis0 = 0; axis1 = 1; }
    }

    static float GetAxis(Vector3 v, int axis)
    {
        if (axis == 0) return v.x;
        if (axis == 1) return v.y;
        return v.z;
    }

    static void SetAxis(ref Vector3 v, int axis, float value)
    {
        if (axis == 0) v.x = value;
        else if (axis == 1) v.y = value;
        else v.z = value;
    }

    static float Min4(float a, float b, float c, float d)
    {
        return Mathf.Min(Mathf.Min(a, b), Mathf.Min(c, d));
    }

    static float Max4(float a, float b, float c, float d)
    {
        return Mathf.Max(Mathf.Max(a, b), Mathf.Max(c, d));
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || targetCamera == null) return;

        Transform cam = targetCamera.transform;
        Vector3 planeOrigin = cam.position + cam.forward * sliceDepth;
        Vector3 planeNormal = cam.forward;

        float absX = Mathf.Abs(planeNormal.x);
        float absY = Mathf.Abs(planeNormal.y);
        float absZ = Mathf.Abs(planeNormal.z);

        int dominant;
        if (absX >= absY && absX >= absZ) dominant = 0;
        else if (absY >= absX && absY >= absZ) dominant = 1;
        else dominant = 2;

        int axis0, axis1;
        GetFreeAxes(dominant, out axis0, out axis1);

        Gizmos.color = Color.yellow;
        Vector3 gBL = SolveVertex(_minX, _minY, axis0, axis1, dominant, planeOrigin, planeNormal);
        Vector3 gBR = SolveVertex(_maxX, _minY, axis0, axis1, dominant, planeOrigin, planeNormal);
        Vector3 gTR = SolveVertex(_maxX, _maxY, axis0, axis1, dominant, planeOrigin, planeNormal);
        Vector3 gTL = SolveVertex(_minX, _maxY, axis0, axis1, dominant, planeOrigin, planeNormal);
        Gizmos.DrawLine(gBL, gBR);
        Gizmos.DrawLine(gBR, gTR);
        Gizmos.DrawLine(gTR, gTL);
        Gizmos.DrawLine(gTL, gBL);

        Gizmos.color = Color.cyan;
        Vector3 bl = targetCamera.ViewportToWorldPoint(new Vector3(0, 0, sliceDepth));
        Vector3 br = targetCamera.ViewportToWorldPoint(new Vector3(1, 0, sliceDepth));
        Vector3 tr = targetCamera.ViewportToWorldPoint(new Vector3(1, 1, sliceDepth));
        Vector3 tl = targetCamera.ViewportToWorldPoint(new Vector3(0, 1, sliceDepth));
        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }
#endif
}
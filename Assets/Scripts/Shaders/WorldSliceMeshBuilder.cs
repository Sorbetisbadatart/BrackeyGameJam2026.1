using System.Collections.Generic;
using UnityEngine;

// Minimal world-slice mesh builder.
// Setup:
//   1. Create an empty GameObject. Add this component.
//   2. Add a Material to its MeshRenderer (anything, even Default-Material).
//   3. Drag the MeshFilter of the object you want to slice into sourceMeshFilter.
//   4. Assign targetCamera (or leave null for Camera.main).
//   5. sliceDepth = distance in front of camera. Must place the plane INSIDE the mesh.

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WorldSliceMeshBuilderGeneric : MonoBehaviour
{
    public MeshFilter sourceMeshFilter;
    public Camera targetCamera;
    public float sliceDepth = 10f;

    Mesh _mesh;

    readonly List<Vector3> _segA = new List<Vector3>();
    readonly List<Vector3> _segB = new List<Vector3>();
    readonly List<Vector3> _loop = new List<Vector3>();

    // The source mesh is stored here so we can read it after the MeshFilter
    // is repurposed to hold the output mesh.
    Mesh _sourceMesh;

    void Awake()
    {
        MeshFilter mf = GetComponent<MeshFilter>();

        // Grab the original mesh BEFORE we overwrite the MeshFilter.
        if (sourceMeshFilter == null)
            sourceMeshFilter = mf;
        _sourceMesh = sourceMeshFilter.sharedMesh;

        _mesh = new Mesh { name = "WorldSlice" };
        _mesh.MarkDynamic();
        mf.mesh = _mesh; // replaces sharedMesh on this object

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (_sourceMesh == null)
            Debug.LogError("[Slice] could not find a source mesh to slice.");
        else
            Debug.Log("[Slice] source mesh: " + _sourceMesh.name
                + "  verts=" + _sourceMesh.vertexCount
                + "  tris=" + (_sourceMesh.triangles.Length / 3));
    }

    void LateUpdate()
    {
        _mesh.Clear();

        if (targetCamera == null) { Debug.LogError("[Slice] no camera"); return; }
        if (sourceMeshFilter == null) { Debug.LogError("[Slice] no sourceMeshFilter -- assign it in the Inspector"); return; }

        Mesh src = _sourceMesh;
        if (src == null) { Debug.LogError("[Slice] sharedMesh is null"); return; }

        // -----------------------------------------------------------------
        // 1. Slice plane
        // -----------------------------------------------------------------
        Vector3 planeOrigin = targetCamera.transform.position
                            + targetCamera.transform.forward * sliceDepth;
        Vector3 planeNormal = targetCamera.transform.forward;

        // -----------------------------------------------------------------
        // 2. Triangle-plane intersections -> segments
        // -----------------------------------------------------------------
        Vector3[] sv = src.vertices;
        int[] st = src.triangles;
        Matrix4x4 l2w = sourceMeshFilter.transform.localToWorldMatrix;

        _segA.Clear();
        _segB.Clear();

        for (int i = 0; i < st.Length; i += 3)
        {
            Vector3 wa = l2w.MultiplyPoint3x4(sv[st[i]]);
            Vector3 wb = l2w.MultiplyPoint3x4(sv[st[i + 1]]);
            Vector3 wc = l2w.MultiplyPoint3x4(sv[st[i + 2]]);

            float da = Vector3.Dot(wa - planeOrigin, planeNormal);
            float db = Vector3.Dot(wb - planeOrigin, planeNormal);
            float dc = Vector3.Dot(wc - planeOrigin, planeNormal);

            Vector3 p0 = Vector3.zero, p1 = Vector3.zero;
            int found = 0;

            if (da * db < 0f) { Lerp(wa, wb, da, db, ref p0, ref p1, ref found); }
            if (db * dc < 0f) { Lerp(wb, wc, db, dc, ref p0, ref p1, ref found); }
            if (dc * da < 0f) { Lerp(wc, wa, dc, da, ref p0, ref p1, ref found); }

            if (found == 2)
            {
                _segA.Add(p0);
                _segB.Add(p1);
            }
        }

        // Diagnose misses: print plane pos vs mesh world bounds
        Bounds meshWorldBounds = new Bounds(
            l2w.MultiplyPoint3x4(src.bounds.center),
            Vector3.zero);
        Vector3 ext = src.bounds.extents;
        // expand bounds by all 8 corners
        for (int si = -1; si <= 1; si += 2)
            for (int sj = -1; sj <= 1; sj += 2)
                for (int sk = -1; sk <= 1; sk += 2)
                    meshWorldBounds.Encapsulate(l2w.MultiplyPoint3x4(src.bounds.center
                        + new Vector3(ext.x * si, ext.y * sj, ext.z * sk)));

        Debug.Log("[Slice] segments=" + _segA.Count
            + "  planeOrigin=" + planeOrigin.ToString("F2")
            + "  meshBounds=" + meshWorldBounds.ToString()
            + "  meshCenter=" + meshWorldBounds.center.ToString("F2")
            + "  camPos=" + targetCamera.transform.position.ToString("F2")
            + "  sliceDepth=" + sliceDepth);

        if (_segA.Count < 2)
        {
            Debug.LogWarning("[Slice] plane missed mesh. "
                + "Try setting sliceDepth to the distance from your camera to "
                + meshWorldBounds.center.ToString("F2") + " which is ~"
                + Vector3.Distance(targetCamera.transform.position, meshWorldBounds.center).ToString("F1"));
            return;
        }

        // -----------------------------------------------------------------
        // 3. Chain segments -> ordered loop
        // -----------------------------------------------------------------
        Chain(_segA, _segB, _loop);
        Debug.Log("[Slice] loop pts=" + _loop.Count);
        if (_loop.Count < 3) { Debug.LogWarning("[Slice] loop too small"); return; }

        // -----------------------------------------------------------------
        // 4. Fan-triangulate directly in WORLD space, upload as local coords
        // -----------------------------------------------------------------
        // Centroid
        Vector3 centre = Vector3.zero;
        for (int i = 0; i < _loop.Count; i++) centre += _loop[i];
        centre /= _loop.Count;

        int n = _loop.Count;
        Matrix4x4 w2l = transform.worldToLocalMatrix;

        // 2*n+2 verts: front-centre, front-ring, back-centre, back-ring
        Vector3[] verts = new Vector3[2 * n + 2];
        Vector3[] normals = new Vector3[2 * n + 2];
        int[] tris = new int[n * 6];

        Vector3 nF = transform.InverseTransformDirection(-planeNormal).normalized;
        Vector3 nB = -nF;

        verts[0] = w2l.MultiplyPoint3x4(centre); normals[0] = nF; // front centre
        verts[n + 1] = w2l.MultiplyPoint3x4(centre); normals[n + 1] = nB; // back centre

        for (int i = 0; i < n; i++)
        {
            verts[1 + i] = w2l.MultiplyPoint3x4(_loop[i]); normals[1 + i] = nF;
            verts[n + 2 + i] = w2l.MultiplyPoint3x4(_loop[i]); normals[n + 2 + i] = nB;
        }

        int t = 0;
        for (int i = 0; i < n; i++)
        {
            int a = 1 + i, b = 1 + (i + 1) % n;
            // front
            tris[t++] = 0; tris[t++] = a; tris[t++] = b;
            // back
            tris[t++] = n + 1; tris[t++] = n + 2 + (i + 1) % n; tris[t++] = n + 2 + i;
        }

        Debug.Log("[Slice] uploading verts=" + verts.Length + " tris=" + (tris.Length / 3));

        _mesh.vertices = verts;
        _mesh.normals = normals;
        _mesh.triangles = tris;
        _mesh.RecalculateBounds();
    }

    static void Lerp(Vector3 a, Vector3 b, float da, float db,
                     ref Vector3 p0, ref Vector3 p1, ref int found)
    {
        Vector3 p = Vector3.Lerp(a, b, da / (da - db));
        if (found == 0) p0 = p; else p1 = p;
        found++;
    }

    static void Chain(List<Vector3> A, List<Vector3> B, List<Vector3> loop)
    {
        int n = A.Count;
        bool[] used = new bool[n];
        loop.Clear();

        loop.Add(A[0]);
        loop.Add(B[0]);
        used[0] = true;
        Vector3 tail = B[0];

        for (int iter = 1; iter < n; iter++)
        {
            int best = -1; bool flip = false; float bd = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                if (used[i]) continue;
                float dA = (A[i] - tail).sqrMagnitude;
                float dB = (B[i] - tail).sqrMagnitude;
                if (dA < bd) { bd = dA; best = i; flip = false; }
                if (dB < bd) { bd = dB; best = i; flip = true; }
            }
            if (best < 0) break;
            used[best] = true;
            Vector3 far = flip ? A[best] : B[best];
            loop.Add(far);
            tail = far;
        }
    }
}
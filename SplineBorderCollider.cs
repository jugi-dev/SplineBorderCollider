using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(SplineContainer))]
public class SplineBorderCollider : MonoBehaviour
{
    [Min(0.01f)]
    public float wallHeight = 10f;

    public bool useKnotYPosition = false;

    public bool drawMesh = true;
    public bool drawWireframe = true;

    SplineContainer splineContainer;
    Mesh mesh;
    MeshCollider meshCollider;

    void OnEnable()
    {
        splineContainer = GetComponent<SplineContainer>();
        meshCollider = GetComponent<MeshCollider>();
        Spline.Changed += OnSplineChanged;
    }

    private void OnDisable()
    {
        Spline.Changed -= OnSplineChanged;
    }

    void OnValidate()
    {
        GenerateBorder();
    }

    private void OnSplineChanged(Spline spline, int arg2, SplineModification modification)
    {
        GenerateBorder();
    }

    void GenerateBorder()
    {
        if (splineContainer == null)
            return;

        Spline spline = splineContainer.Spline;

        int knotCount = spline.Count;
        if (knotCount < 2)
            return;

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "PlayableAreaMesh";
        }
        else
        {
            mesh.Clear();
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < knotCount; i++)
        {
            BezierKnot knot = spline[i];
            Vector3 pos = knot.Position;

            if (useKnotYPosition)
            {
                pos.y = knot.Position.y;
            }
            else
            {
                pos.y = 0f;
            }

            vertices.Add(pos + Vector3.up * -wallHeight); // bottom
            vertices.Add(pos + Vector3.up * wallHeight); // top
        }

        if (spline.Closed)
        {
            DrawClosedLoop(knotCount, triangles);
        }
        else
        {
            DrawOpenLoop(knotCount, triangles);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false;
    }

    void DrawClosedLoop(int knotCount, List<int> triangles)
    {
        for (int i = 0; i < knotCount; i++)
        {
            int curr = i * 2;
            int next = (i + 1) % knotCount * 2;

            triangles.Add(curr);
            triangles.Add(next);
            triangles.Add(curr + 1);

            triangles.Add(curr + 1);
            triangles.Add(next);
            triangles.Add(next + 1);
        }
    }

    void DrawOpenLoop(int knotCount, List<int> triangles)
    {
        for (int i = 0; i < knotCount - 1; i++)
        {
            int curr = i * 2;
            int next = (i + 1) % knotCount * 2;

            triangles.Add(curr);
            triangles.Add(next);
            triangles.Add(curr + 1);

            triangles.Add(curr + 1);
            triangles.Add(next);
            triangles.Add(next + 1);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1f, 0, 0.25f);
        if (drawMesh)
        {
            Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
        }
        if (drawWireframe)
        {
            Gizmos.DrawWireMesh(mesh, transform.position, transform.rotation);
        }
    }
}
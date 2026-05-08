using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PolygonCollider2D))]
public class ProBuilderTo2DCollider : MonoBehaviour
{
    [ContextMenu("Bake Mesh To PolygonCollider2D")]
    public void BakeCollider()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        PolygonCollider2D col = GetComponent<PolygonCollider2D>();

        Vector3[] verts = mesh.vertices;
        Vector2[] points2D = System.Array.ConvertAll(verts, v => new Vector2(v.x, v.y));

        // Get the outline only (no internal triangles)
        col.pathCount = 1;
        col.SetPath(0, points2D);
    }
}
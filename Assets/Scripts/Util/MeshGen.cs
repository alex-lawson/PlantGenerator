using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen {
    public int VertexCount {
        get { return vertices.Count; }
    }

    public int TriangleCount {
        get { return triangles[subMesh].Count; }
    }

    private List<Vector3> vertices;
    private List<int>[] triangles;
    private Material[] materials;
    private int subMesh;
    private int subMeshCount;
    private Mesh mesh;
    private MeshFilter targetMeshFilter;
    private MeshRenderer targetMeshRenderer;
    private MeshCollider targetMeshCollider;

    public MeshGen() {
        vertices = new List<Vector3>();
        triangles = new List<int>[1];
        mesh = new Mesh();
    }

    public MeshGen(Material[] subMeshMaterials) {
        vertices = new List<Vector3>();
        mesh = new Mesh();
        SetMaterials(subMeshMaterials);
    }

    public void SetMaterials(Material[] newMaterials) {
        materials = newMaterials;
        subMeshCount = materials.Length;
        subMesh = 0;
        triangles = new List<int>[subMeshCount];
        for (int i = 0; i < subMeshCount; i++)
            triangles[i] = new List<int>();
    }

    public void SetMaterial(int materialId) {
        if (materialId < subMeshCount)
            subMesh = materialId;
    }

    public void SetTarget(GameObject targetObject) {
        targetMeshFilter = targetObject.GetComponent<MeshFilter>();
        targetMeshRenderer = targetObject.GetComponent<MeshRenderer>();
        targetMeshCollider = targetObject.GetComponent<MeshCollider>();
    }

    public void Clear() {
        mesh.Clear();
        vertices.Clear();
        for (int i = 0; i < subMeshCount; i++)
            triangles[i].Clear();
    }

    public void BuildAndAssign() {
        BuildMesh();
        AssignMesh();
    }

    public void BuildMesh() {
        mesh.SetVertices(vertices);

        mesh.subMeshCount = subMeshCount;
        for (int i = 0; i < subMeshCount; i++)
            mesh.SetTriangles(triangles[i], i);

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

    public void AssignMesh() {
        targetMeshFilter.sharedMesh = mesh;
        targetMeshRenderer.materials = materials;
        if (targetMeshCollider != null)
            targetMeshCollider.sharedMesh = mesh;
    }

    public int AddVertex(Vector3 newVertex) {
        int newIndex = vertices.Count;
        vertices.Add(newVertex);
        return newIndex;
    }

    public void AddTriangle(int a, int b, int c) {
        triangles[subMesh].Add(a);
        triangles[subMesh].Add(b);
        triangles[subMesh].Add(c);
    }

    public void AddFace(int a, int b, int c) {
        AddTriangle(a, b, c);
    }

    public void AddFace(int a, int b, int c, int d) {
        AddTriangle(a, b, c);
        AddTriangle(a, c, d);
    }

    public void AddFace(int a, int b, int c, int d, int e) {
        AddTriangle(a, b, c);
        AddTriangle(a, c, d);
        AddTriangle(a, d, e);
    }

    public void AddFace(int a, int b, int c, int d, int e, int f) {
        AddTriangle(a, b, c);
        AddTriangle(a, c, d);
        AddTriangle(a, d, e);
        AddTriangle(a, e, f);
    }

    public void AddFace(int a, int b, int c, int d, int e, int f, int g) {
        AddTriangle(a, b, c);
        AddTriangle(a, c, d);
        AddTriangle(a, d, e);
        AddTriangle(a, e, f);
        AddTriangle(a, f, g);
    }

    public void AddFace(int a, int b, int c, int d, int e, int f, int g, int h) {
        AddTriangle(a, b, c);
        AddTriangle(a, c, d);
        AddTriangle(a, d, e);
        AddTriangle(a, e, f);
        AddTriangle(a, f, g);
        AddTriangle(a, g, h);
    }
    
    public void TranslateVertices(Vector3 offset) {
        TranslateVertices(0, VertexCount, offset);
    }

    public void TranslateVertices(int fromId, int toId, Vector3 offset) {
        for (int i = fromId; i < toId; i++) {
            vertices[i] += offset;
        }
    }

    public void RotateVertices(float angle, Vector3 axis, Vector3 center) {
        RotateVertices(0, VertexCount, angle, axis, center);
    }

    public void RotateVertices(int fromId, int toId, float angle, Vector3 axis, Vector3 center) {
        Quaternion rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);
        RotateVertices(fromId, toId, rotation);
    }

    public void RotateVertices(Quaternion rotation) {
        RotateVertices(0, VertexCount, rotation);
    }

    public void RotateVertices(int fromId, int toId, Quaternion rotation) {
        for (int i = fromId; i < toId; i++) {
            vertices[i] = rotation * vertices[i];
        }
    }

    public void ScaleVertices(float scale, Vector3 center) {
        ScaleVertices(0, VertexCount, scale, center);
    }

    public void ScaleVertices(int fromId, int toId, float scale, Vector3 center) {
        for (int i = fromId; i < toId; i++) {
            Vector3 v = vertices[i];
            vertices[i] = (v - center) * scale + center;
        }
    }

    public void ScaleVertices(float scale) {
        ScaleVertices(0, VertexCount, scale);
    }

    public void ScaleVertices(int fromId, int toId, float scale) {
        for (int i = fromId; i < toId; i++) {
            vertices[i] *= scale;
        }
    }
}

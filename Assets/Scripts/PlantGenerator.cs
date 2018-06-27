using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlantSpecies {
    public int RotationalSymmetry;
    public float SegmentLength;
    public float SegmentRadius;
}

[ExecuteInEditMode]
public class PlantGenerator : MonoBehaviour {

    public PlantSpecies Species;
    public float Growth;

	void Start () {
        Generate();
	}

	void Update () {

	}

    private void OnValidate() {
        Generate();
    }

    public void Generate() {
        List<Vector3> stemVertices = new List<Vector3>();
        List<int> stemTriangles = new List<int>();
        Quaternion baseRotation = Quaternion.identity;

        GenerateSegment(Vector3.zero, baseRotation, Growth, ref stemVertices, ref stemTriangles);

        Mesh mesh = MeshGen.BuildMesh(stemVertices, stemTriangles);

        MeshGen.AssignMesh(gameObject, mesh);
    }

    private void GenerateSegment(Vector3 startPosition, Quaternion orientation, float segmentGrowth, ref List<Vector3> vertices, ref List<int> triangles) {
        List<Vector3> segmentVertices = new List<Vector3>();
        List<int> segmentTriangles = new List<int>();

        float startScale = Mathf.Pow(segmentGrowth, 0.33f);
        float endScale = Mathf.Pow(segmentGrowth - 1, 0.33f);
        float segmentLength = Species.SegmentLength * startScale;

        float sliceAngle = 2 * Mathf.PI / Species.RotationalSymmetry;

        List<Vector3> startRing = new List<Vector3>();
        for (int i = 0; i < Species.RotationalSymmetry; i++) {
            startRing.Add(MeshGen.RadialPoint(i * -sliceAngle, Species.SegmentRadius * startScale, 0));
        }

        if (segmentGrowth >= 1) {
            List<Vector3> endRing = new List<Vector3>();
            for (int i = 0; i < Species.RotationalSymmetry; i++) {
                endRing.Add(MeshGen.RadialPoint(i * -sliceAngle, Species.SegmentRadius * endScale, segmentLength));
            }

            MeshGen.AddTubeFaces(startRing, endRing, ref segmentVertices, ref segmentTriangles);
        } else {
            MeshGen.AddPeak(startRing, segmentLength, ref segmentVertices, ref segmentTriangles);
        }

        segmentVertices = MeshGen.RotateVertices(segmentVertices, orientation, Vector3.zero);
        segmentVertices = MeshGen.TranslateVertices(segmentVertices, startPosition);

        MeshGen.CombineLists(segmentVertices, segmentTriangles, ref vertices, ref triangles);

        Vector3 endOffset = Vector3.up * segmentLength;
        Vector3 endPosition = startPosition + orientation * endOffset;
        if (segmentGrowth > 1)
            GenerateSegment(endPosition, orientation, segmentGrowth - 1, ref vertices, ref triangles);
    }
}

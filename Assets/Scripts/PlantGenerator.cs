using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlantSpecies {
    public int StemSides;
    public float SegmentLength;
    public float SegmentRadius;
    public float ScaleExponent;
    public float LeafThreshold;
    public int LeavesPerSegment;
    public float LeafVerticalAngle;
    public int WhorlNom;
    public int WhorlDenom;
    public float PetioleLength;
    public float PetioleWidth;
    public float PetioleDepth;
    public float BladePosition;
    public float BladeLength;
    public float BladeWidth;
    public float BladeFoldAngle;
}

[ExecuteInEditMode]
public class PlantGenerator : MonoBehaviour {

    public PlantSpecies Species;
    public float Growth;
    public Material[] Materials;

	void Start () {
        Generate();
	}

	void Update () {

	}

    private void OnValidate() {
        Generate();
    }

    public void Generate() {
        List<Vector3> vertices = new List<Vector3>();
        List<int>[] triangles = new List<int>[2];
        for (int i = 0; i < triangles.Length; i++)
            triangles[i] = new List<int>();

        Quaternion baseRotation = Quaternion.identity;

        float startTime = Time.realtimeSinceStartup;

        GenerateSegment(Vector3.zero, baseRotation, Growth, ref vertices, ref triangles);

        float elapsedMs = (Time.realtimeSinceStartup - startTime) * 1000;
        Debug.Log($"plant generated in {elapsedMs:F2}ms");

        MeshGen.DeduplicateVertices(ref vertices, ref triangles);

        Mesh mesh = MeshGen.BuildMesh(vertices, triangles);

        MeshGen.AssignMesh(gameObject, mesh, Materials);
    }

    private void GenerateSegment(Vector3 startPosition, Quaternion orientation, float segmentGrowth, ref List<Vector3> vertices, ref List<int>[] triangles) {
        List<Vector3> segmentVertices = new List<Vector3>();
        List<int> segmentTriangles = new List<int>();

        int segmentNumber = Mathf.FloorToInt(Growth - segmentGrowth);

        float startScale = Mathf.Pow(segmentGrowth, Species.ScaleExponent);
        float endScale = Mathf.Pow(segmentGrowth - 1, Species.ScaleExponent);
        float segmentLength = Species.SegmentLength * startScale;

        // build stem segment geometry

        float sliceAngle = 2 * Mathf.PI / Species.StemSides;

        List<Vector3> startRing = new List<Vector3>();
        for (int i = 0; i < Species.StemSides; i++) {
            startRing.Add(MeshGen.RadialPoint(i * -sliceAngle, Species.SegmentRadius * startScale, 0));
        }

        if (segmentGrowth >= 1) {
            List<Vector3> endRing = new List<Vector3>();
            for (int i = 0; i < Species.StemSides; i++) {
                endRing.Add(MeshGen.RadialPoint(i * -sliceAngle, Species.SegmentRadius * endScale, segmentLength));
            }

            MeshGen.AddTubeFaces(startRing, endRing, ref segmentVertices, ref segmentTriangles);
        } else {
            MeshGen.AddPeak(startRing, segmentLength, ref segmentVertices, ref segmentTriangles);
        }

        // rotate, translate, and add stem segment to final mesh

        segmentVertices = MeshGen.RotateVertices(segmentVertices, orientation, Vector3.zero);
        segmentVertices = MeshGen.TranslateVertices(segmentVertices, startPosition);

        MeshGen.CombineLists(segmentVertices, segmentTriangles, ref vertices, ref triangles[0]);

        if (segmentNumber > 0 && segmentGrowth > Species.LeafThreshold && Species.LeavesPerSegment > 0) {
            float leafBaseRotation = segmentNumber * 360 * (Species.WhorlNom / (float)Species.WhorlDenom - 1);
            // build leaves
            float leafAngleSeparation = 360f / Species.LeavesPerSegment;
            for (int i = 0; i < Species.LeavesPerSegment; i++) {
                Quaternion rotate = Quaternion.AngleAxis(leafBaseRotation + i * leafAngleSeparation, Vector3.up);
                GenerateLeaf(startPosition, rotate * orientation, segmentGrowth - Species.LeafThreshold, ref vertices, ref triangles);
            }
        }

        Vector3 endOffset = Vector3.up * segmentLength;
        Vector3 endPosition = startPosition + orientation * endOffset;
        if (segmentGrowth > 1)
            GenerateSegment(endPosition, orientation, segmentGrowth - 1, ref vertices, ref triangles);
    }

    public void GenerateLeaf(Vector3 position, Quaternion orientation, float leafGrowth, ref List<Vector3> vertices, ref List<int>[] triangles) {

        Quaternion leafOrientation = orientation * Quaternion.Euler(Species.LeafVerticalAngle, 0, 0);

        float leafScale = Mathf.Pow(leafGrowth, Species.ScaleExponent);

        // generate petiole

        List<Vector3> petioleVerts = new List<Vector3>();
        List<int> petioleTris = new List<int>();
        
        List<Vector3> petioleRing = new List<Vector3> {
            Vector3.right * Species.PetioleWidth * 0.5f,
            Vector3.left * Species.PetioleWidth * 0.5f,
            Vector3.down * Species.PetioleDepth
        };
        petioleRing = MeshGen.ScaleVertices(petioleRing, leafScale, Vector3.zero);

        float petioleLength = Species.PetioleLength * leafScale;
        Vector3 petioleTip = new Vector3(0, 0, petioleLength);

        MeshGen.AddFanFaces(petioleRing, petioleTip, ref petioleVerts, ref petioleTris);

        petioleVerts = MeshGen.RotateVertices(petioleVerts, leafOrientation, Vector3.zero);
        petioleVerts = MeshGen.TranslateVertices(petioleVerts, position);

        MeshGen.CombineLists(petioleVerts, petioleTris, ref vertices, ref triangles[0]);

        // generate blade

        List<Vector3> bladeVerts = new List<Vector3>();
        List<int> bladeTris = new List<int>();

        Vector3 bladeBase = Vector3.zero;
        Vector3 bladeTip = new Vector3(0, 0, Species.BladeLength);
        float bladeX = Mathf.Cos(Species.BladeFoldAngle) * Species.BladeWidth * 0.5f;
        float bladeY = Mathf.Sin(Species.BladeFoldAngle) * Species.BladeWidth * 0.5f;
        Vector3 bladeLeft = new Vector3(-bladeX, bladeY, Species.BladeLength * 0.5f);
        Vector3 bladeRight = new Vector3(bladeX, bladeY, Species.BladeLength * 0.5f);
        MeshGen.AddFace(new List<Vector3> { bladeBase, bladeTip, bladeRight }, ref bladeVerts, ref bladeTris);
        MeshGen.AddFace(new List<Vector3> { bladeBase, bladeLeft, bladeTip }, ref bladeVerts, ref bladeTris);

        bladeVerts = MeshGen.ScaleVertices(bladeVerts, leafScale, Vector3.zero);
        bladeVerts = MeshGen.RotateVertices(bladeVerts, leafOrientation, Vector3.zero);

        Vector3 bladePosition = position + leafOrientation * Vector3.forward * petioleLength * Species.BladePosition;
        bladeVerts = MeshGen.TranslateVertices(bladeVerts, position);

        MeshGen.CombineLists(bladeVerts, bladeTris, ref vertices, ref triangles[1]);
    }
}

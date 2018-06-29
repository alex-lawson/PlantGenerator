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
    public int WhorlNumber;
    public float PetioleLength;
    public float PetioleWidth;
    public float PetioleDepth;
    public float BladePosition;
    public float BladeLength;
    public float BladeWidth;
    public float BladeFoldAngle;

    public void ClampValues() {
        StemSides = Mathf.Max(StemSides, 3);
        SegmentLength = Mathf.Max(SegmentLength, 0);
        SegmentRadius = Mathf.Max(SegmentRadius, 0);
        ScaleExponent = Mathf.Max(ScaleExponent, 0);
        LeavesPerSegment = Mathf.Max(LeavesPerSegment, 0);
        WhorlNumber = Mathf.Clamp(WhorlNumber, 0, 26);
        PetioleLength = Mathf.Max(PetioleLength, 0);
        PetioleWidth = Mathf.Max(PetioleWidth, 0);
        PetioleDepth = Mathf.Max(PetioleDepth, 0);
        BladeLength = Mathf.Max(BladeLength, 0);
        BladeWidth = Mathf.Max(BladeWidth, 0);
    }
}

[ExecuteInEditMode]
public class PlantGenerator : MonoBehaviour {

    public static readonly int[] Fibonacci = { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181, 6765, 10946, 17711, 28657, 46368, 75025, 121393, 196418, 317811 };

    public bool GenerateOnUpdate;
    public PlantSpecies Species;
    public float Growth;
    public Material[] Materials;

    private MeshGen mg;
    private PoolRing<List<int>> tempIndices;

    private void Start () {
        mg = new MeshGen(Materials);
        mg.SetTarget(gameObject);

        tempIndices = new PoolRing<List<int>>(4);

        Generate();
	}

	private void Update () {
        if (GenerateOnUpdate)
            Generate();
	}

    public void Generate() {
        Species.ClampValues();

        mg.Clear();

        Quaternion baseRotation = Quaternion.identity;

        //float startTime = Time.realtimeSinceStartup;

        GenerateSegment(Vector3.zero, baseRotation, Growth, null);

        mg.BuildAndAssign();

        //float elapsedMs = (Time.realtimeSinceStartup - startTime) * 1000;
        //Debug.Log($"plant generated in {elapsedMs:F2}ms");
    }

    private void GenerateSegment(Vector3 startPosition, Quaternion orientation, float segmentGrowth, List<int> startRing) {
        int segmentNumber = Mathf.FloorToInt(Growth - segmentGrowth);

        float startScale = Mathf.Pow(segmentGrowth, Species.ScaleExponent);
        float endScale = Mathf.Pow(segmentGrowth - 1, Species.ScaleExponent);
        float segmentLength = Species.SegmentLength * startScale;

        // build stem segment geometry

        int startVC = mg.VertexCount;

        mg.SetMaterial(0);

        float sliceAngle = 2 * Mathf.PI / Species.StemSides;

        if (startRing == null) {
            startRing = tempIndices.GetNext();
            startRing.Clear();

            for (int i = 0; i < Species.StemSides; i++) {
                startRing.Add(mg.AddVertex(RadialPoint(i * sliceAngle, Species.SegmentRadius * startScale, 0)));
            }
        }

        if (segmentGrowth >= 1) {
            // normal (tube) segment

            List<int> endRing = tempIndices.GetNext();
            endRing.Clear();

            for (int i = 0; i < Species.StemSides; i++) {
                endRing.Add(mg.AddVertex(RadialPoint(i * sliceAngle, Species.SegmentRadius * endScale, segmentLength)));
            }

            for (int i = 0; i < Species.StemSides; i++) {
                int j = (i + 1) % Species.StemSides;
                mg.AddFace(endRing[i], endRing[j], startRing[j], startRing[i]);
            }

            startRing = endRing;
        } else {
            // end (cone) segment

            int endPoint = mg.AddVertex(new Vector3(0, segmentLength, 0));

            for (int i = 0; i < Species.StemSides; i++) {
                int j = (i + 1) % Species.StemSides;
                mg.AddFace(endPoint, startRing[j], startRing[i]);
            }
        }

        // orient and position stem segment

        int endVC = mg.VertexCount;
        mg.RotateVertices(startVC, endVC, orientation);
        mg.TranslateVertices(startVC, endVC, startPosition);

        // grow leaves

        if (segmentNumber > 0 && segmentGrowth > Species.LeafThreshold && Species.LeavesPerSegment > 0) {
            float leafBaseRotation = segmentNumber * 180 * (Fibonacci[Species.WhorlNumber] / (float)Fibonacci[Species.WhorlNumber + 1]);
            float leafAngleSeparation = 360f / Species.LeavesPerSegment;
            for (int i = 0; i < Species.LeavesPerSegment; i++) {
                Quaternion rotate = Quaternion.AngleAxis(leafBaseRotation + i * leafAngleSeparation, Vector3.up);
                GenerateLeaf(startPosition, rotate * orientation, segmentGrowth - Species.LeafThreshold);
            }
        }

        // grow a new segment if necessary

        if (segmentGrowth > 1) {
            Vector3 endOffset = Vector3.up * segmentLength;
            Vector3 endPosition = startPosition + orientation * endOffset;
            GenerateSegment(endPosition, orientation, segmentGrowth - 1, startRing);
        }
    }

    private void GenerateLeaf(Vector3 position, Quaternion orientation, float leafGrowth) {

        Quaternion leafOrientation = orientation * Quaternion.Euler(-Species.LeafVerticalAngle, 0, 0);

        float leafScale = Mathf.Pow(leafGrowth, Species.ScaleExponent);

        // generate petiole

        int startVC = mg.VertexCount;

        mg.SetMaterial(0);

        List<int> petioleRing = tempIndices.GetNext();
        petioleRing.Clear();

        petioleRing.Add(mg.AddVertex(Vector3.left * Species.PetioleWidth * 0.5f));
        petioleRing.Add(mg.AddVertex(Vector3.right * Species.PetioleWidth * 0.5f));
        petioleRing.Add(mg.AddVertex(Vector3.down * Species.PetioleDepth));

        int petioleEndPoint = mg.AddVertex(new Vector3(0, 0, Species.PetioleLength));

        for (int i = 0; i < 3; i++) {
            int j = (i + 1) % 3;
            mg.AddFace(petioleEndPoint, petioleRing[j], petioleRing[i]);
        }

        // generate blade

        int bladeStartVC = mg.VertexCount;

        mg.SetMaterial(1);

        // blade front faces

        float bladeX = Mathf.Cos(Species.BladeFoldAngle * Mathf.Deg2Rad) * Species.BladeWidth * 0.5f;
        float bladeY = Mathf.Sin(Species.BladeFoldAngle * Mathf.Deg2Rad) * Species.BladeWidth * 0.5f;
        Vector3 bladeLeftPos = new Vector3(-bladeX, bladeY, Species.BladeLength * 0.5f);
        Vector3 bladeRightPos = new Vector3(bladeX, bladeY, Species.BladeLength * 0.5f);
        Vector3 bladeTipPos = new Vector3(0, 0, Species.BladeLength);

        int bladeBase = mg.AddVertex(Vector3.zero);
        int bladeTip = mg.AddVertex(bladeTipPos);
        int bladeLeft = mg.AddVertex(bladeLeftPos);
        int bladeRight = mg.AddVertex(bladeRightPos);

        mg.AddFace(bladeBase, bladeTip, bladeRight);
        mg.AddFace(bladeBase, bladeLeft, bladeTip);

        // blade back faces

        bladeBase = mg.AddVertex(Vector3.zero);
        bladeTip = mg.AddVertex(bladeTipPos);
        bladeLeft = mg.AddVertex(bladeLeftPos);
        bladeRight = mg.AddVertex(bladeRightPos);

        mg.AddFace(bladeBase, bladeRight, bladeTip);
        mg.AddFace(bladeBase, bladeTip, bladeLeft);

        // orient and position leaf

        int endVC = mg.VertexCount;

        mg.TranslateVertices(bladeStartVC, endVC, new Vector3(0, 0, Species.PetioleLength * Species.BladePosition));

        mg.ScaleVertices(startVC, endVC, leafScale);
        mg.RotateVertices(startVC, endVC, leafOrientation);
        mg.TranslateVertices(startVC, endVC, position);
    }

    private static Vector3 RadialPoint(float angle, float radius, float y) {
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        return new Vector3(x, y, z);
    }
}

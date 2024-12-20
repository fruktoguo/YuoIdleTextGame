using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

public class Potential : MonoBehaviour
{
    List<PointCharge> chargesInScene = new List<PointCharge>();
    public ScalarFieldPoint[] scalarField;
    public NativeParallelHashMap<int, ScalarFieldPoint> scalarFieldMap;

    public UpdatePotentialJob potentialModificationJob;
    public JobHandle potentialModificationJobHandle;

    public float fieldExponent = 1.0f;

    public void RegisterCharge(PointCharge input)
    {
        chargesInScene.Add(input);
    }

    public void RemoveCharge(PointCharge input)
    {
        chargesInScene.Remove(input);
    }

    public void BuildScalarField(int nX, int nY, int nZ, float gridSize)
    {
        scalarField = new ScalarFieldPoint[nX * nY * nZ];
        scalarFieldMap = new NativeParallelHashMap<int, ScalarFieldPoint>(nX * nY * nZ, Allocator.TempJob);

        NativeArray<PointCharge> chargesInScenNative =
            new NativeArray<PointCharge>(chargesInScene.Count, Allocator.TempJob);

        for (int i = 0; i < chargesInScene.Count; i++)
            chargesInScenNative[i] = chargesInScene[i];

        potentialModificationJob = new UpdatePotentialJob()
        {
            nX = nX,
            nY = nY,
            nZ = nZ,
            gridSize = gridSize,
            chargesInScene = chargesInScenNative,
            ScalarFieldWriter = scalarFieldMap,
            fieldExponent = fieldExponent
        };

        potentialModificationJobHandle = potentialModificationJob.Schedule(nX * nY * nZ, 64);
        potentialModificationJobHandle.Complete();

        // Use TryGetValue instead of direct indexing
        for (int i = 0; i < (nX * nY * nZ); i++)
        {
            ScalarFieldPoint point;
            if (scalarFieldMap.TryGetValue(i, out point))
            {
                scalarField[i] = point;
            }
        }

        chargesInScenNative.Dispose();
        scalarFieldMap.Dispose();
    }

    [BurstCompile]
    public struct UpdatePotentialJob : IJobParallelFor
    {
        [ReadOnly] public int nX;
        [ReadOnly] public int nY;
        [ReadOnly] public int nZ;
        [ReadOnly] public float gridSize;
        [ReadOnly] public NativeArray<PointCharge> chargesInScene;
        [ReadOnly] public float fieldExponent;

        [NativeDisableParallelForRestriction] public NativeParallelHashMap<int, ScalarFieldPoint> ScalarFieldWriter;

        public void Execute(int i)
        {
            BuildScalarField(i);
        }

        float GetPotential(Vector3 Position)
        {
            float potential = 0;
            for (int i = 0; i < chargesInScene.Length; i++)
            {
                PointCharge x = chargesInScene[i];
                potential += x.charge / math.pow((Position - x.position).magnitude, fieldExponent);
            }
            return potential;
        }

        void BuildScalarField(int i)
        {
            Position position = GetCoordsFromLinear(i);
            ScalarFieldPoint scalarFieldPoint;
            scalarFieldPoint.position =
                new Vector3(position.x * gridSize, position.y * gridSize, position.z * gridSize);
            scalarFieldPoint.potential = GetPotential(scalarFieldPoint.position);
            ScalarFieldWriter.TryAdd(i, scalarFieldPoint);
        }

        struct Position
        {
            public int x;
            public int y;
            public int z;
        }

        Position GetCoordsFromLinear(int index)
        {
            Position output;
            output.x = index / (nY * nZ);
            output.y = (index % (nY * nZ)) / nZ;
            output.z = index % nZ;
            return output;
        }
    }
}


public struct ScalarFieldPoint
{
    public Vector3 position;
    public float potential;
}

public struct PointCharge
{
    public Vector3 position;
    public float charge;
}
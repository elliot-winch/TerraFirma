using System.Collections.Generic;
using UnityEngine;

public abstract class DensityGenerator : MonoBehaviour 
{
    [SerializeField]
    protected ComputeShader m_DensityShader;
    [SerializeField]
    private MeshGenerator m_MeshGenerator;

    protected List<ComputeBuffer> buffersToRelease;

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            m_MeshGenerator.SettingsUpdated.Value = true;
        }
    }

    public virtual ComputeBuffer Generate 
        (ComputeBuffer pointsBuffer, 
        Vector3Int numPoints, 
        Vector3 boundsSize, 
        Vector3 centre, 
        Vector3 offset, 
        Vector3 spacing) 
    {
        // Points buffer is populated inside shader with pos (xyz) + density (w).
        // Set paramaters
        m_DensityShader.SetBuffer (0, "points", pointsBuffer);
        m_DensityShader.SetInts ("numPoints", numPoints.x, numPoints.y, numPoints.z);
        m_DensityShader.SetVector ("boundsSize", new Vector4(boundsSize.x, boundsSize.y, boundsSize.z));
        m_DensityShader.SetVector ("centre", new Vector4 (centre.x, centre.y, centre.z));
        m_DensityShader.SetVector ("offset", new Vector4 (offset.x, offset.y, offset.z));
        m_DensityShader.SetVector ("spacing", new Vector4(spacing.x, spacing.y, spacing.z));

        // Dispatch shader
        int xGroups = Mathf.CeilToInt(numPoints.x / (float)MeshGenerator.THREAD_GROUP_SIZE);
        int yGroups = Mathf.CeilToInt(numPoints.y / (float)MeshGenerator.THREAD_GROUP_SIZE);
        int zGroups = Mathf.CeilToInt(numPoints.z / (float)MeshGenerator.THREAD_GROUP_SIZE);
        m_DensityShader.Dispatch (0, xGroups, yGroups, zGroups);

        if (buffersToRelease != null) 
        {
            foreach (var b in buffersToRelease) 
            {
                b.Release();
            }
        }

        // Return voxel data buffer so it can be used to generate mesh
        return pointsBuffer;
    }
}
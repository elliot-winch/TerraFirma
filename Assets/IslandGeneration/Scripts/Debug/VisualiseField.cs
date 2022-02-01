using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VisualiseField : MonoBehaviour
{
    [SerializeField]
    private MeshGenerator m_MeshGenerator;
    [SerializeField]
    private DensityGenerator m_DensityGenerator;
    [SerializeField]
    private int m_Level;

    // Buffers
    ComputeBuffer pointsBuffer;

    [Header("Visualise")]
    public Color minInsideColor;
    public Color maxInsideColor;
    public Color minOutsideColor;
    public Color maxOutsideColor;
    public bool m_ThresholdColor;

    private bool m_SettingsUpdated;
    private Vector4[] m_Points;
    private float m_Min;
    private float m_Max;

    [SerializeField]
    private MeshRenderer m_TextureObject;
    private Texture2D m_Texture;

    void OnValidate()
    {
        m_SettingsUpdated = true;
    }

    private void Update()
    {
        if (m_SettingsUpdated)
        {
            Run();
            m_SettingsUpdated = false;
        }
    }

    public void Run()
    {
        CreateBuffers();
        CreateTexture();

        foreach (Chunk chunk in m_MeshGenerator.chunks)
        {
            Visualise(chunk);
        }
    }

    public void Visualise(Chunk chunk)
    {
        m_DensityGenerator.Generate(
            pointsBuffer, 
            m_MeshGenerator.numPointsPerAxis, 
            m_MeshGenerator.boundsSize, 
            m_MeshGenerator.WorldBounds,
            m_MeshGenerator.CentreFromCoord(chunk.coord), 
            m_MeshGenerator.offset, 
            m_MeshGenerator.PointSpacing
        );
        
        m_Points = new Vector4[m_MeshGenerator.NumPoints];

        pointsBuffer.GetData(m_Points);

        m_Min = float.MaxValue;
        m_Max = float.MinValue;

        foreach (Vector4 p in m_Points)
        {
            if (p.w < m_Min)
            {
                m_Min = p.w;
            }

            if (p.w > m_Max)
            {
                m_Max = p.w;
            }
        }

        DrawTexture();
    }

    private void DrawTexture()
    {
        if(m_Points == null)
        {
            return;
        }

        if (m_Level < 0 || m_Level >= m_MeshGenerator.numPointsPerAxis)
        {
            return;
        }

        Color[] textureColors = new Color[m_Texture.width * m_Texture.height];

        for(int i = 0; i < m_Texture.width; i++)
        {
            for (int j = 0; j < m_Texture.height; j++)
            {
                int pixelIndex = j + i * m_MeshGenerator.numPointsPerAxis;
                int fieldIndex = pixelIndex + m_Level * m_MeshGenerator.numPointsPerAxis * m_MeshGenerator.numPointsPerAxis;

                float normalisedValue = Mathf.InverseLerp(m_Min, m_Max, m_Points[fieldIndex].w);

                Color c;

                if (m_Points[fieldIndex].w > m_MeshGenerator.isoLevel)
                {
                    c = Color.Lerp(minInsideColor, maxInsideColor, normalisedValue);
                }
                else
                {
                    c = Color.Lerp(minOutsideColor, maxOutsideColor, normalisedValue);
                }

                textureColors[pixelIndex] = c;
            }
        }

        m_Texture.SetPixels(textureColors);
        m_Texture.Apply();
    }

    private void CreateTexture()
    {
        if (m_Texture == null || m_Texture.width != m_MeshGenerator.numPointsPerAxis || m_Texture.height != m_MeshGenerator.numPointsPerAxis)
        {
            m_Texture = new Texture2D(m_MeshGenerator.numPointsPerAxis, m_MeshGenerator.numPointsPerAxis)
            {
                filterMode = FilterMode.Point
            };
            m_TextureObject.sharedMaterial.SetTexture("_MainTex", m_Texture);
        }
    }

    private void CreateBuffers()
    {
        if (pointsBuffer == null || m_MeshGenerator.NumPoints != pointsBuffer.count)
        {
            ReleaseBuffers();
            pointsBuffer = new ComputeBuffer(m_MeshGenerator.NumPoints, sizeof(float) * 4);
        }
    }

    private void ReleaseBuffers()
    {
        if (pointsBuffer != null)
        {
            pointsBuffer.Release();
        }
    }
}

using System;
using System.Linq;
using UnityEngine;

public class VisualiseField : MonoBehaviour
{
    private enum Dimension
    {
        X,
        Y,
        Z
    };

    [SerializeField]
    private Vector3Int m_ChunkCoord;
    [SerializeField]
    private MeshGenerator m_MeshGenerator;
    [SerializeField]
    private DensityGenerator m_DensityGenerator;
    [SerializeField]
    private int m_Level;
    [SerializeField]
    private Dimension m_Dimension;
    [SerializeField]
    private Color m_MinInsideColor;
    [SerializeField]
    private Color m_MaxInsideColor;
    [SerializeField]
    private Color m_MinOutsideColor;
    [SerializeField]
    private Color m_MaxOutsideColor;
    [SerializeField]
    private MeshRenderer m_TextureObject;

    private ComputeBuffer m_PointsBuffer;

    private Texture2D m_Texture;

    public Action OnValidated { get; set; }

    private int NumPointsDimensionCurrent
    {
        get
        {
            switch (m_Dimension)
            {
                case Dimension.X:
                    return m_MeshGenerator.NumPointsPerAxis.x;
                case Dimension.Y:
                    return m_MeshGenerator.NumPointsPerAxis.y;
                default:
                    return m_MeshGenerator.NumPointsPerAxis.z;
            }
        }
    }

    private int NumPointsDimensionHeight
    {
        get
        {
            switch (m_Dimension)
            {
                case Dimension.X:
                    return m_MeshGenerator.NumPointsPerAxis.y;
                case Dimension.Y:
                    return m_MeshGenerator.NumPointsPerAxis.z;
                default:
                    return m_MeshGenerator.NumPointsPerAxis.x;
            }
        }
    }

    private int NumPointsDimensionWidth
    {
        get
        {
            switch (m_Dimension)
            {
                case Dimension.X:
                    return m_MeshGenerator.NumPointsPerAxis.z;
                case Dimension.Y:
                    return m_MeshGenerator.NumPointsPerAxis.x;
                default:
                    return m_MeshGenerator.NumPointsPerAxis.y;
            }
        }
    }

    private void Awake()
    {
        CreateBuffers();
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    private void OnValidate()
    {
        OnValidated?.Invoke();
    }

    public void Visualise()
    {
        CreateTexture();

        if(m_MeshGenerator.Chunks == null)
        {
            return;
        }

        Chunk chunk = m_MeshGenerator.Chunks.FirstOrDefault(c => c.coord == m_ChunkCoord);

        if (chunk)
        {
            Visualise(chunk);
        }
    }

    public void Visualise(Chunk chunk)
    {
        if (m_Level < 0 || m_Level >= NumPointsDimensionCurrent)
        {
            return;
        }

        Vector3Int numPoints;
        switch (m_Dimension)
        {
            case Dimension.X:
                numPoints = new Vector3Int(1, NumPointsDimensionHeight, NumPointsDimensionWidth);
                break;
            case Dimension.Y:
                numPoints = new Vector3Int(NumPointsDimensionWidth, 1, NumPointsDimensionHeight);
                break;
            default:
                numPoints = new Vector3Int(NumPointsDimensionWidth, NumPointsDimensionHeight, 1);
                break;
        }

        Vector3 offset;
        switch (m_Dimension)
        {
            case Dimension.X:
                offset = new Vector3(m_Level * m_MeshGenerator.PointSpacing.x, 0f, 0f);
                break;
            case Dimension.Y:
                offset = new Vector3(0f, m_Level * m_MeshGenerator.PointSpacing.y, 0f);
                break;
            default:
                offset = new Vector3(0f, 0f, m_Level * m_MeshGenerator.PointSpacing.z);
                break;
        }

        m_DensityGenerator.Generate(
            m_PointsBuffer, 
            numPoints,
            m_MeshGenerator.ChunkSize, 
            m_MeshGenerator.CentreFromCoord(chunk.coord), 
            m_MeshGenerator.Offset + offset, 
            m_MeshGenerator.PointSpacing
        );
        
        Vector4[] points = new Vector4[NumPointsDimensionWidth * NumPointsDimensionHeight];

        m_PointsBuffer.GetData(points);

        float min = float.MaxValue;
        float max = float.MinValue;

        foreach (Vector4 p in points)
        {
            if (p.w < min)
            {
                min = p.w;
            }

            if (p.w > max)
            {
                max = p.w;
            }
        }

        DrawTexture(points, min, max);
    }

    private void DrawTexture(Vector4[] points, float min, float max)
    {
        Color[] textureColors = new Color[m_Texture.width * m_Texture.height];

        for(int i = 0; i < m_Texture.width; i++)
        {
            for (int j = 0; j < m_Texture.height; j++)
            {
                int pixelIndex = j + i * NumPointsDimensionWidth;

                float normalisedValue = Mathf.InverseLerp(min, max, points[pixelIndex].w);

                Color c;

                if (points[pixelIndex].w > m_MeshGenerator.FieldThreshold)
                {
                    c = Color.Lerp(m_MinInsideColor, m_MaxInsideColor, normalisedValue);
                }
                else
                {
                    c = Color.Lerp(m_MinOutsideColor, m_MaxOutsideColor, normalisedValue);
                }

                textureColors[pixelIndex] = c;
            }
        }

        m_Texture.SetPixels(textureColors);
        m_Texture.Apply();
    }

    private void CreateTexture()
    {
        if (m_Texture == null || m_Texture.width != NumPointsDimensionWidth || m_Texture.height != NumPointsDimensionHeight)
        {
            m_Texture = new Texture2D(NumPointsDimensionWidth, NumPointsDimensionHeight)
            {
                filterMode = FilterMode.Point
            };
            m_TextureObject.material.SetTexture("_MainTex", m_Texture);
        }
    }

    private void CreateBuffers()
    {
        ReleaseBuffers();

        m_PointsBuffer = new ComputeBuffer(m_MeshGenerator.NumPoints, sizeof(float) * 4);
    }

    private void ReleaseBuffers()
    {
        if (m_PointsBuffer != null)
        {
            m_PointsBuffer.Release();
        }
    }
}

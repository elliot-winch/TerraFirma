using UnityEngine;

public class VisualiseField : MonoBehaviour
{
    [SerializeField]
    private MeshGenerator m_MeshGenerator;
    [SerializeField]
    private DensityGenerator m_DensityGenerator;
    [SerializeField]
    private int m_Level;
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

    private void Awake()
    {
        CreateBuffers();
        CreateTexture();

        m_MeshGenerator.SettingsUpdated.Subscribe(OnSettingsUpdated);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            m_MeshGenerator.SettingsUpdated.Value = true;
        }
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    private void OnSettingsUpdated(bool updated)
    {
        if (updated)
        {
            Run();
        }
    }

    public void Run()
    {
        if(m_MeshGenerator.Chunks == null)
        {
            return;
        }

        foreach (Chunk chunk in m_MeshGenerator.Chunks)
        {
            Visualise(chunk);
        }
    }

    public void Visualise(Chunk chunk)
    {
        m_DensityGenerator.Generate(
            m_PointsBuffer, 
            new Vector3Int(m_MeshGenerator.NumPointsPerAxis.x, 1, m_MeshGenerator.NumPointsPerAxis.z), 
            m_MeshGenerator.ChunkSize, 
            m_MeshGenerator.CentreFromCoord(chunk.coord), 
            m_MeshGenerator.Offset, 
            m_MeshGenerator.PointSpacing
        );
        
        Vector4[] points = new Vector4[m_MeshGenerator.NumPointsPerAxis.x * m_MeshGenerator.NumPointsPerAxis.z];

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
        if (m_Level < 0 || m_Level >= m_MeshGenerator.NumPointsPerAxis.y)
        {
            return;
        }

        Color[] textureColors = new Color[m_Texture.width * m_Texture.height];

        for(int i = 0; i < m_Texture.width; i++)
        {
            for (int j = 0; j < m_Texture.height; j++)
            {
                int pixelIndex = j + i * m_MeshGenerator.NumPointsPerAxis.x;

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
        if (m_Texture == null || m_Texture.width != m_MeshGenerator.NumPointsPerAxis.x || m_Texture.height != m_MeshGenerator.NumPointsPerAxis.z)
        {
            m_Texture = new Texture2D(m_MeshGenerator.NumPointsPerAxis.x, m_MeshGenerator.NumPointsPerAxis.z)
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

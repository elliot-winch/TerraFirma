using UnityEngine;

public class IslandDensity : DensityGenerator
{
    [SerializeField]
    private IslandGenerationData m_Data;

    public IslandGenerationData Data
    {
        get
        {
            return m_Data;
        }
        set
        {
            if(m_Data != null)
            {
                m_Data.OnParametersChanged -= Validated;
            }

            m_Data = value;

            if(m_Data != null)
            {
                m_Data.OnParametersChanged += Validated;
            }
        }
    }

    private ComputeBuffer m_NoiseParameterBuffer;

    private void Awake()
    {
        CreateBuffers();
    }

    private void Start()
    {
        if (m_Data != null)
        {
            Data = m_Data;
        }
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    private void Validated()
    {
        m_MeshGenerator.SettingsUpdated.Value = true;
    }

    private void CreateBuffers()
    {
        ReleaseBuffers();

        m_NoiseParameterBuffer = new ComputeBuffer(2, IslandGenerationData.NoiseParameters.SizeOf);
    }

    private void ReleaseBuffers()
    {
        if (m_NoiseParameterBuffer != null)
        {
            m_NoiseParameterBuffer.Release();
        }
    }

    public override ComputeBuffer Generate
        (ComputeBuffer pointsBuffer,
        Vector3Int numPoints,
        Vector3 boundsSize,
        Vector3 centre,
        Vector3 offset,
        Vector3 spacing)
    {
        if (Data != null)
        {
            m_DensityShader.SetFloat("curvature", Data.Curvature);
            m_DensityShader.SetFloat("coneRadius", Data.ConeRadius);
            m_DensityShader.SetFloat("coneHeight", Data.ConeHeight);

            m_DensityShader.SetFloat("noiseInfluenceCurve", Data.NoiseInfluenceCurve);

            m_NoiseParameterBuffer.SetData(new IslandGenerationData.NoiseParameters[] { Data.VerticalNoiseParameters, Data.BaseNoiseParameters });
            m_DensityShader.SetBuffer(0, "noiseParameters", m_NoiseParameterBuffer);
        }
        else
        {
            throw new System.Exception("Attempting to generate island without data!");
        }

        return base.Generate(pointsBuffer, numPoints, boundsSize, centre, offset, spacing);
    }
}

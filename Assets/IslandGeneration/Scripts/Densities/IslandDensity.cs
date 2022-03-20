using UnityEngine;

public class IslandDensity : DensityGenerator
{
    public float Curvature;
    public float ConeRadius;
    public float ConeHeight;
    public float NoiseInfluenceCurve;

    public NoiseParameters VerticalNoiseParameters;
    [HideInInspector] //Set by Island
    public NoiseParameters BaseNoiseParameters;

    private ComputeBuffer m_NoiseParameterBuffer;

    private void Awake()
    {
        CreateBuffers();
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    private void CreateBuffers()
    {
        ReleaseBuffers();

        m_NoiseParameterBuffer = new ComputeBuffer(2, NoiseParameters.SizeOf);
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

        m_DensityShader.SetFloat("curvature", Curvature);
        m_DensityShader.SetFloat("coneRadius", ConeRadius);
        m_DensityShader.SetFloat("coneHeight", ConeHeight);

        m_DensityShader.SetFloat("noiseInfluenceCurve", NoiseInfluenceCurve);

        m_NoiseParameterBuffer.SetData(new NoiseParameters[] { BaseNoiseParameters, VerticalNoiseParameters });
        m_DensityShader.SetBuffer(0, "noiseParameters", m_NoiseParameterBuffer);

        return base.Generate(pointsBuffer, numPoints, boundsSize, centre, offset, spacing);
    }
}

using UnityEngine;

public class IslandDensity : DensityGenerator
{
    [SerializeField]
    private IslandGenerationData m_Data;

    public IslandGenerationData Data { get; set; }

    private void Awake()
    {
        if(m_Data != null)
        {
            Data = m_Data;
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

            m_DensityShader.SetInt("octaves", Data.Octaves);
            m_DensityShader.SetFloat("startingFrequency", Data.StartingFrequency);
            m_DensityShader.SetFloat("frequencyStep", Data.FrequencyStep);
            m_DensityShader.SetFloat("startingAmplitude", Data.StartingAmplitude);
            m_DensityShader.SetFloat("amplitudeStep", Data.AmplitudeStep);
            m_DensityShader.SetVector("noiseOffset", Data.NoiseOffset);
            m_DensityShader.SetVector("noiseScalar", Data.NoiseScalar);
        }
        else
        {
            throw new System.Exception("Attempting to generate island without data!");
        }

        return base.Generate(pointsBuffer, numPoints, boundsSize, centre, offset, spacing);
    }
}

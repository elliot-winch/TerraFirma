using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandBaseDensity : DensityGenerator
{
    [SerializeField]
    private float m_Curvature = 1f;
    [SerializeField]
    private float m_ConeRadius = 32f;
    [SerializeField]
    private float m_ConeHeight = 64f;
    [SerializeField]
    private float m_NoiseInfluenceCurve = 1f;

    [SerializeField]
    private int m_Octaves;
    [SerializeField]
    private float m_StartingFrequency;
    [SerializeField]
    private float m_FrequencyStep;
    [SerializeField]
    private float m_StartingAmplitude;
    [SerializeField]
    private float m_AmplitudeStep;
    [SerializeField]
    private Vector4 m_NoiseOffset;
    [SerializeField]
    private Vector4 m_NoiseScalar;

    public override ComputeBuffer Generate(ComputeBuffer pointsBuffer, int numPointsPerAxis, float boundsSize, Vector3 worldBounds, Vector3 centre, Vector3 offset, float spacing)
    {
        densityShader.SetFloat("curvature", m_Curvature);
        densityShader.SetFloat("coneRadius", m_ConeRadius);
        densityShader.SetFloat("coneHeight", m_ConeHeight);

        densityShader.SetFloat("noiseInfluenceCurve", m_NoiseInfluenceCurve);

        densityShader.SetInt("octaves", m_Octaves);
        densityShader.SetFloat("startingFrequency", m_StartingFrequency);
        densityShader.SetFloat("frequencyStep", m_FrequencyStep);
        densityShader.SetFloat("startingAmplitude", m_StartingAmplitude);
        densityShader.SetFloat("amplitudeStep", m_AmplitudeStep);
        densityShader.SetVector("noiseOffset", m_NoiseOffset);
        densityShader.SetVector("noiseScalar", m_NoiseScalar);

        return base.Generate(pointsBuffer, numPointsPerAxis, boundsSize, worldBounds, centre, offset, spacing);
    }
}

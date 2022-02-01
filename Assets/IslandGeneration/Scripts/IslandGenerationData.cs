using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IslandGenerationData", menuName = "Custom/IslandGenerationData")]
public class IslandGenerationData : ScriptableObject
{
    [Header("Cone")]
    [SerializeField]
    private float m_Curvature = 1f;
    public float Curvature => m_Curvature;
    [SerializeField]
    private float m_ConeRadius = 0.5f;
    public float ConeRadius => m_ConeRadius;

    [SerializeField]
    private float m_ConeHeight = 1f;
    public float ConeHeight => m_ConeHeight;
    [SerializeField]
    private float m_NoiseInfluenceCurve = 1f;
    public float NoiseInfluenceCurve => m_NoiseInfluenceCurve;

    [Header("Noise")]
    [SerializeField]
    private int m_Octaves;
    public int Octaves => m_Octaves;

    [SerializeField]
    private float m_StartingFrequency;
    public float StartingFrequency => m_StartingFrequency;

    [SerializeField]
    private float m_FrequencyStep;
    public float FrequencyStep => m_FrequencyStep;

    [SerializeField]
    private float m_StartingAmplitude;
    public float StartingAmplitude => m_StartingAmplitude;

    [SerializeField]
    private float m_AmplitudeStep;
    public float AmplitudeStep => m_AmplitudeStep;

    [SerializeField]
    private Vector4 m_NoiseOffset;
    public Vector4 NoiseOffset => m_NoiseOffset;

    [SerializeField]
    private Vector4 m_NoiseScalar;
    public Vector4 NoiseScalar => m_NoiseScalar;
}

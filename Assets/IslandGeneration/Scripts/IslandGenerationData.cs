using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IslandGenerationData", menuName = "Custom/IslandGenerationData")]
public class IslandGenerationData : ScriptableObject
{
    [Serializable]
    public struct NoiseParameters
    {
        public int Octaves;
        public float StartingFrequency;
        public float FrequencyStep;
        public float StartingAmplitude;
        public float AmplitudeStep;
        public Vector4 NoiseOffset;
        public Vector4 NoiseScalar;

        public static int SizeOf => sizeof(int) + sizeof(float) * 4 + sizeof(float) * 4 * 2;
    }

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
    private NoiseParameters m_VerticalNoiseParameters;
    public NoiseParameters VerticalNoiseParameters => m_VerticalNoiseParameters;

    [SerializeField]
    private NoiseParameters m_BaseNoiseParameters;
    public NoiseParameters BaseNoiseParameters => m_BaseNoiseParameters;

    public Action OnParametersChanged { get; set; }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            OnParametersChanged?.Invoke();
        }
    }
}

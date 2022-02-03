using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NoiseParameters", menuName = "Custom/Noise Parameters")]
public class NoiseParametersObject : ScriptableObject
{
    [SerializeField]
    private NoiseParameters m_NoiseParameters;

    public NoiseParameters NoiseParameters => m_NoiseParameters;
}

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

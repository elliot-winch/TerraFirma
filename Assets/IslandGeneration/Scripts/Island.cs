using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island : MonoBehaviour
{
    [SerializeField]
    private float m_ConeRadius;
    [SerializeField]
    private NoiseParameters m_BaseNoiseParameters;

    [SerializeField]
    private IslandDensity m_TopDensity;
    [SerializeField]
    private IslandDensity m_BottomDensity;

    private void Awake()
    {
        Sync();
    }

    private void Sync()
    {
        m_TopDensity.ConeRadius = m_ConeRadius;
        m_BottomDensity.ConeRadius = m_ConeRadius;
        m_TopDensity.BaseNoiseParameters = m_BaseNoiseParameters;
        m_BottomDensity.BaseNoiseParameters = m_BaseNoiseParameters;
    }
}

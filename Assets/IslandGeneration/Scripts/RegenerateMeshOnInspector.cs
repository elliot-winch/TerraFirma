using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegenerateMeshOnInspector : MonoBehaviour
{
    [SerializeField]
    private MeshGenerator m_MeshGenerator;
    [SerializeField]
    private DensityGenerator m_DensityGenerator;
    [SerializeField]
    private VisualiseField m_VisualiseField;

    private void Start()
    {
        m_MeshGenerator.OnValidated += Regenerate;
        m_DensityGenerator.OnValidated += Regenerate;

        if (m_VisualiseField != null)
        {
            m_VisualiseField.OnValidated += Regenerate;
        }
    }

    private void Regenerate()
    {
        m_MeshGenerator.UpdateChunks();

        if(m_VisualiseField != null)
        {
            m_VisualiseField.Visualise();
        }
    }
}

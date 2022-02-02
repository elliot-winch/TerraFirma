using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    private struct Triangle
    {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", a, b, c);
        }
    }

    //Cannot be changed without changing const in compute shader
    public const int THREAD_GROUP_SIZE = 8;

    [SerializeField]
    private DensityGenerator m_DensityGenerator;
    [SerializeField]
    private Transform m_ChunkHolder;

    [Header("Geometry")]
    [SerializeField]
    private Vector3Int m_NumChunks = Vector3Int.one;
    [SerializeField]
    private float m_FieldThreshold;
    public float FieldThreshold => m_FieldThreshold;

    [SerializeField]
    private Vector3 m_ChunkSize = Vector3.one;
    public Vector3 ChunkSize => m_ChunkSize;
    [Tooltip("Don't change at runtime, as the buffers do not change size")]
    [SerializeField]
    private Vector3Int m_NumPoints = Vector3Int.one * 2;
    public Vector3Int NumPointsPerAxis => m_NumPoints;
    [SerializeField]
    private Vector3 m_Offset;
    public Vector3 Offset => m_Offset;

    [Header("Rendering")]
    [SerializeField]
    private ComputeShader m_MarchingCubesShader;
    [SerializeField]
    private Material m_Material;
    [SerializeField]
    private bool m_GenerateColliders;

    [Header("Gizmos")]
    [SerializeField]
    private bool m_ShowBoundsGizmo = true;
    [SerializeField]
    private Color m_BoundsGizmoCol = Color.white;

    //Chunks
    private List<Chunk> m_Chunks;
    public List<Chunk> Chunks => m_Chunks;

    // Buffers
    private ComputeBuffer triangleBuffer;
    private ComputeBuffer pointsBuffer;
    private ComputeBuffer triCountBuffer;

    public SubscriptionValue<bool> SettingsUpdated { get; } = new SubscriptionValue<bool>(false);

    public Vector3 PointSpacing 
    {
        get
        {
            return new Vector3()
            {
                x = m_ChunkSize.x / (m_NumPoints.x - 1),
                y = m_ChunkSize.y / (m_NumPoints.y - 1),
                z = m_ChunkSize.z / (m_NumPoints.z - 1),
            };
        }
    }
    public Vector3 WorldBounds
    {
        get
        {
            return new Vector3
            {
                x = m_NumChunks.x * m_ChunkSize.x,
                y = m_NumChunks.y * m_ChunkSize.y,
                z = m_NumChunks.z * m_ChunkSize.z
            };
        }
    }
    public int NumPoints => m_NumPoints.x * m_NumPoints.y * m_NumPoints.z;
    public int NumVoxels => (m_NumPoints.x - 1) * (m_NumPoints.y - 1) * (m_NumPoints.z - 1);

    #region Unity Functions
    private void Awake()
    {
        CreateBuffers();
        InitChunks();
    }

    private void Start()
    {
        SettingsUpdated.Subscribe(OnSettingsUpdated);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            SettingsUpdated.Value = true;
        }
    }

    void OnDestroy()
    {
        ReleaseBuffers();
    }

    void OnDrawGizmos()
    {
        if (m_ShowBoundsGizmo && m_Chunks != null)
        {
            Gizmos.color = m_BoundsGizmoCol;

            foreach (var chunk in m_Chunks)
            {
                Gizmos.DrawWireCube(CentreFromCoord(chunk.coord), m_ChunkSize);
            }
        }
    }
    #endregion

    private void OnSettingsUpdated(bool updated)
    {
        if (updated)
        {
            SettingsUpdated.Value = false;
            UpdateChunks();
        }
    }

    #region Public Functions
    public void UpdateChunks()
    {
        foreach (Chunk chunk in m_Chunks)
        {
            UpdateChunkMesh(chunk);
        }
    }

    public Vector3 CentreFromCoord(Vector3Int coord)
    {
        Vector3 position = new()
        {
            x = coord.x * m_ChunkSize.x,
            y = coord.y * m_ChunkSize.y,
            z = coord.z * m_ChunkSize.z
        };

        return -WorldBounds / 2 + position + m_ChunkSize / 2;
    }
    #endregion

    #region Buffers
    private void CreateBuffers()
    {
        ReleaseBuffers();

        int maxTriangleCount = NumVoxels * 10;

        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        pointsBuffer = new ComputeBuffer(NumPoints, sizeof(float) * 4);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    }

    private void ReleaseBuffers()
    {
        if (triangleBuffer != null)
        {
            triangleBuffer.Release();
        }

        if(pointsBuffer != null)
        {
            pointsBuffer.Release();
        }

        if(triCountBuffer != null)
        {
            triCountBuffer.Release();
        }
    }
    #endregion

    #region Chunks
    private void UpdateChunkMesh(Chunk chunk)
    {
        Vector3 centre = CentreFromCoord(chunk.coord);

        //Dispatch density shader to calculate field points
        m_DensityGenerator.Generate(pointsBuffer, m_NumPoints, ChunkSize, centre, m_Offset, PointSpacing);
        
        //Dispatch marching cubes buffer to generate mesh data
        triangleBuffer.SetCounterValue(0);
        m_MarchingCubesShader.SetBuffer(0, "points", pointsBuffer);
        m_MarchingCubesShader.SetBuffer(0, "triangles", triangleBuffer);
        m_MarchingCubesShader.SetInts("numPoints", m_NumPoints.x, m_NumPoints.y, m_NumPoints.z);
        m_MarchingCubesShader.SetFloat("isoLevel", m_FieldThreshold);

        int xGroups = Mathf.CeilToInt((m_NumPoints.x - 1) / (float)THREAD_GROUP_SIZE);
        int yGroups = Mathf.CeilToInt((m_NumPoints.y - 1) / (float)THREAD_GROUP_SIZE);
        int zGroups = Mathf.CeilToInt((m_NumPoints.z - 1) / (float)THREAD_GROUP_SIZE);
        m_MarchingCubesShader.Dispatch(0, xGroups, yGroups, zGroups);

        //Use mesh data from GPU to build mesh on CPU
        CreateMesh(chunk);
    }

    private void CreateMesh(Chunk chunk)
    {
        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);

        Mesh mesh = chunk.mesh;
        mesh.Clear();

        var vertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateNormals();
    }

    /*
     * TODO: if smooth surfaces are required, use this algorithm on the GPU to
     * get average normals
    private void SmoothNormals(Mesh mesh)
    {
        Vector3[] meshNormals = mesh.normals;
        Vector3[] vertices = mesh.vertices;

        float dist = 0.00001f;
        Vector3[] smoothNormals = new Vector3[vertices.Length];
        int[] sameVerts = Enumerable.Repeat(-1, vertices.Length).ToArray();

        for (int i = 0; i < vertices.Length; i++)
        {
            if (sameVerts[i] >= 0)
            {
                smoothNormals[i] = smoothNormals[sameVerts[i]];
                continue;
            }

            sameVerts[i] = i;
            List<int> closeVerts = new();
            for (int j = 0; j < vertices.Length; j++)
            {
                if ((vertices[i] - vertices[j]).sqrMagnitude < dist)
                {
                    closeVerts.Add(j);
                    sameVerts[j] = i;
                }
            }

            Vector3 averageNormal = meshNormals[i];
            foreach (int vertIndex in closeVerts)
            {
                averageNormal += meshNormals[vertIndex];
            }

            smoothNormals[i] = (averageNormal / (closeVerts.Count + 1)).normalized;
        }

        mesh.normals = smoothNormals;
    }

    
    */

    private void InitChunks () 
    {
        m_Chunks = new List<Chunk>();

        // Go through all coords and create a chunk there if one doesn't already exist
        for (int x = 0; x < m_NumChunks.x; x++) 
        {
            for (int y = 0; y < m_NumChunks.y; y++) 
            {
                for (int z = 0; z < m_NumChunks.z; z++) 
                {
                    GameObject chunk = new($"Chunk ({x}, {y}, {z})");

                    chunk.transform.parent = m_ChunkHolder.transform;
                    chunk.transform.localRotation = Quaternion.identity;
                    chunk.transform.localPosition = Vector3.zero;

                    Chunk newChunk = chunk.AddComponent<Chunk>();
                    newChunk.coord = new Vector3Int(x, y, z);

                    newChunk.SetUp (m_Material, m_GenerateColliders);

                    m_Chunks.Add(newChunk);
                }
            }
        }
    }
    #endregion
}
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainWetOverlay : MonoBehaviour
{
    [SerializeField] Terrain terrain;
    [SerializeField, Range(8, 160)] int resolution = 80;
    [SerializeField] float heightOffset = 0.04f;
    [SerializeField] Color waterColor = new Color(0.08f, 0.32f, 0.55f, 0.38f);
    [SerializeField, Range(0f, 1f)] float smoothness = 0.92f;
    [SerializeField, Range(0f, 1f)] float metallic = 0f;
    [SerializeField] Material waterMaterial;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        SetupMaterial();
    }

    void OnEnable()
    {
        if (terrain == null)
            terrain = FindFirstObjectByType<Terrain>();

        GenerateOverlay();
    }

    [ContextMenu("Generate Overlay")]
    public void GenerateOverlay()
    {
        if (terrain == null || terrain.terrainData == null)
            return;

        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        var terrainTransform = terrain.transform;
        var terrainData = terrain.terrainData;
        var terrainSize = terrainData.size;
        var terrainPosition = terrainTransform.position;

        transform.position = terrainPosition;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        var vertexCount = (resolution + 1) * (resolution + 1);
        var vertices = new Vector3[vertexCount];
        var uvs = new Vector2[vertexCount];
        var triangles = new int[resolution * resolution * 6];

        for (var z = 0; z <= resolution; z++)
        {
            for (var x = 0; x <= resolution; x++)
            {
                var index = z * (resolution + 1) + x;
                var normalizedX = x / (float)resolution;
                var normalizedZ = z / (float)resolution;

                var worldX = terrainPosition.x + normalizedX * terrainSize.x;
                var worldZ = terrainPosition.z + normalizedZ * terrainSize.z;
                var worldY = terrain.SampleHeight(new Vector3(worldX, 0f, worldZ)) + terrainPosition.y + heightOffset;

                vertices[index] = new Vector3(worldX - terrainPosition.x, worldY - terrainPosition.y, worldZ - terrainPosition.z);
                uvs[index] = new Vector2(normalizedX, normalizedZ);
            }
        }

        var triangleIndex = 0;
        for (var z = 0; z < resolution; z++)
        {
            for (var x = 0; x < resolution; x++)
            {
                var bottomLeft = z * (resolution + 1) + x;
                var bottomRight = bottomLeft + 1;
                var topLeft = bottomLeft + resolution + 1;
                var topRight = topLeft + 1;

                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = bottomRight;
                triangles[triangleIndex++] = bottomRight;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = topRight;
            }
        }

        var mesh = new Mesh
        {
            name = "Terrain Wet Overlay Mesh",
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.sharedMesh = mesh;
    }

    void SetupMaterial()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        if (waterMaterial != null)
        {
            meshRenderer.sharedMaterial = waterMaterial;
            return;
        }

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        var material = new Material(shader)
        {
            name = "Runtime Wet Terrain Overlay",
            color = waterColor
        };

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", waterColor);

        if (material.HasProperty("_Color"))
            material.SetColor("_Color", waterColor);

        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", smoothness);

        if (material.HasProperty("_Glossiness"))
            material.SetFloat("_Glossiness", smoothness);

        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", metallic);

        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        meshRenderer.sharedMaterial = material;
    }
}

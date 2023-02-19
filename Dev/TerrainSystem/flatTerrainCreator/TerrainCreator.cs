using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCreator : MonoBehaviour
{
    [SerializeField]
    int mapSize;
    [SerializeField]
    float gridSize;
    [SerializeField]
    Material TerrainMaterial;
    [SerializeField]
    Texture2D heightMap;
    [SerializeField]
    Texture2D textureLayerMap;

    GridMatrix matrix;
    // Start is called before the first frame update
    void Start()
    {
        matrix = new GridMatrix(mapSize, gridSize);
        matrix.applyHeightMap(heightMap);
        createMesh(matrix);
    }

    private MeshData createMesh(GridMatrix matrix)
    {
        Mesh currMesh = new Mesh();
        currMesh.Clear();

        MeshData currMeshData = new MeshData();
        // set mesh
        currMeshData.createMesh(matrix, heightMap);

        currMesh.vertices = currMeshData.vertices.ToArray();
        currMesh.uv = currMeshData.uvs.ToArray();
        currMesh.triangles = currMeshData.triangles.ToArray();
        currMesh.RecalculateNormals();
        // set materials
        Material[] materialArray = new Material[] {TerrainMaterial};

        GameObject currGameObject = GameObject.Find("terrainMesh");
        if (!currGameObject)
        {
            currGameObject = new GameObject("terrainMesh");
            currGameObject.transform.parent = this.transform;
            currGameObject.AddComponent(typeof(MeshFilter));
            currGameObject.AddComponent(typeof(MeshCollider));
            currGameObject.AddComponent(typeof(MeshRenderer));
        }
        currGameObject.transform.parent = this.transform;

        currGameObject.GetComponent<MeshFilter>().sharedMesh = currMesh;
        currGameObject.GetComponent<MeshRenderer>().materials = materialArray;
        currGameObject.GetComponent<MeshCollider>().sharedMesh = currMesh;
        return currMeshData;
    }

    private void OnDrawGizmos()
    {
        if(matrix == null)
        {
            return;
        }


    }
}

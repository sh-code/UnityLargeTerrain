using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class DrawInstances : MonoBehaviour
{
    public class InstancedMesh
    {
        public Mesh mesh;
        public Material[] materials;
        public Matrix4x4[][] instanceTransforms;
    }

    Dictionary<Mesh, InstancedMesh> instancedMeshes = new Dictionary<Mesh, InstancedMesh>();
    Dictionary<Mesh, List<List<Transform>>> instancedMeshTransforms = new Dictionary<Mesh, List<List<Transform>>>();

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] found = GameObject.FindObjectsOfType<ToBeDrawnInstanced>().Select(tbd => tbd.gameObject).ToArray();
        MeshFilter[] inFound;

        int instanceArrsCount;

        //collect all meshes and materials from all tagged objects that we found
        for(int c1 = 0; c1 < found.Length; c1++)
        {
            inFound = found[c1].GetComponentsInChildren<MeshFilter>();

            for(int c2 = 0; c2 < inFound.Length; c2++)
            {
                if (!instancedMeshes.ContainsKey(inFound[c2].sharedMesh))
                {
                    instancedMeshes.Add(inFound[c2].sharedMesh, new InstancedMesh()
                    {
                        mesh = inFound[c2].sharedMesh,
                        materials = inFound[c2].gameObject.GetComponent<Renderer>().materials
                    });
                }

                if (!instancedMeshTransforms.ContainsKey(inFound[c2].sharedMesh))
                {
                    instancedMeshTransforms.Add(inFound[c2].sharedMesh, new List<List<Transform>>());
                    instancedMeshTransforms[inFound[c2].sharedMesh].Add(new List<Transform>());
                }

                instanceArrsCount = instancedMeshTransforms[inFound[c2].sharedMesh].Count;

                if (instancedMeshTransforms[inFound[c2].sharedMesh][instanceArrsCount-1].Count == 1022)
                {
                    instancedMeshTransforms[inFound[c2].sharedMesh].Add(new List<Transform>(1022));
                    instanceArrsCount++;
                }
                    
                instancedMeshTransforms[inFound[c2].sharedMesh][instanceArrsCount-1].Add(inFound[c2].transform);
            }
        }

        //get the transform's matrices and pack them to their corresponding InstancedMeshes
        foreach(KeyValuePair<Mesh, InstancedMesh> kvp in instancedMeshes)
        {
            kvp.Value.instanceTransforms = new Matrix4x4[instancedMeshTransforms[kvp.Key].Count][];

            for(int c = 0; c < kvp.Value.instanceTransforms.Length; c++)
                kvp.Value.instanceTransforms[c] = instancedMeshTransforms[kvp.Key][c].Select(t => Matrix4x4.TRS(t.position, t.rotation, t.lossyScale)).ToArray();
        }

        Debug.Log("found.Length = " + found.Length);


        //destroy those objects now
        for (int c = 0; c < found.Length; c++) GameObject.Destroy(found[c]);
    }


    void Update()
    {
        foreach(KeyValuePair<Mesh, InstancedMesh> kvp in instancedMeshes)
        {
            for(int sub = 0; sub < kvp.Value.mesh.subMeshCount; sub++)
            {
                for(int instancesChunk = 0; instancesChunk < kvp.Value.instanceTransforms.Length; instancesChunk++)
                    Graphics.DrawMeshInstanced(kvp.Value.mesh, sub, kvp.Value.materials[sub], kvp.Value.instanceTransforms[instancesChunk]);
            }
        }
    }
}

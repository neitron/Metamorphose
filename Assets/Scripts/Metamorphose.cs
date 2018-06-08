using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Metamorphose : MonoBehaviour 
{


	#region Unity properties

	[SerializeField] private Mesh _mesh;
	[SerializeField] private Mesh _meshTarget;
	[SerializeField] private Transform _originalInstance;
	[SerializeField, Range(0.001f, 1.0f)] private float _instanceScale;
	[SerializeField] private float _physicsTimeDelay;

	#endregion



	private void Start () 
	{
		//spawn();
		spawnPolies();
	}
	

	private void spawnPolies()
	{
		List<Vector3> verticies = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> texCoords = new List<Vector2>();

		_mesh.GetVertices(verticies);
		_mesh.GetTriangles(triangles, 0);
		_mesh.GetNormals(normals);
		_mesh.GetUVs(0, texCoords);

		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();

		Mesh pMesh = new Mesh
		{
			vertices = new []
			{
				Vector3.zero,
				Vector3.up, 
				Vector3.left
			}
		};
		pMesh.SetIndices(new[] { 0, 1, 2 }, MeshTopology.Triangles, 0);
		pMesh.RecalculateNormals();

		for(int t = 0; t < triangles.Count; t += 3)
		{
			GameObject p = new GameObject($"triangle_{t}");

			Vector4[] verts =
			{
				transform.TransformPoint(verticies[triangles[t]]), 
				transform.TransformPoint(verticies[triangles[t + 1]]), 
				transform.TransformPoint(verticies[triangles[t + 2]])
			};

			Vector4[] norms =
			{
				transform.TransformVector(normals[triangles[t]]), 
				transform.TransformVector(normals[triangles[t + 1]]), 
				transform.TransformVector(normals[triangles[t + 2]])
			};

			Vector4[] uvs =
			{
				verticies[triangles[t]],
				verticies[triangles[t + 1]],
				verticies[triangles[t + 2]]
			};


			//ComputeBuffer vPoses = new ComputeBuffer(verts.Length, sizeof(float) * 3);
			//vPoses.SetData(verts);

			Transform instance = p.transform;
			p.AddComponent<MeshFilter>().mesh = pMesh;
			p.AddComponent<MeshRenderer>().sharedMaterial = _originalInstance.GetComponent<MeshRenderer>().sharedMaterial;

			instance.SetParent(transform);

			Color color = Color.Lerp(new Color(Random.value, Random.value, Random.value), Color.white, 0.7f); // just do it a bit white


			// Draw the mesh with instancing.

			materialPropertyBlock.SetMatrix("_VertexPositions", new Matrix4x4(verts[0], verts[1], verts[2], Vector4.zero));
			materialPropertyBlock.SetMatrix("_VertexNormals", new Matrix4x4(norms[0], norms[1], norms[2], (norms[0] + norms[1] + norms[2]).normalized));
			materialPropertyBlock.SetMatrix("_VertexUV", new Matrix4x4(uvs[0], uvs[1], uvs[2], Vector4.zero));
			//vPoses.Release();

			//materialPropertyBlock.SetColor("_Color", color);
			//materialPropertyBlock.SetFloat("_Metallic", Random.value);
			//materialPropertyBlock.SetFloat("_Glossiness", Random.value);
			instance.GetComponent<MeshRenderer>().SetPropertyBlock(materialPropertyBlock);
		}
	}


	private void spawn()
	{
		List<Vector3> verticiesList = new List<Vector3>();
		_mesh.GetVertices(verticiesList);

		HashSet<Vector3> verticies = new HashSet<Vector3>(verticiesList);
		
		Debug.Log(verticies.Count);

		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();

		foreach (Vector3 v in verticies)
		{
			Transform instance = Instantiate(_originalInstance);
			instance.SetParent(transform);
			instance.localScale = Vector3.one * _instanceScale;
			instance.localPosition = v;

			Color color = Color.Lerp(new Color(Random.value, Random.value, Random.value), Color.white, 0.7f); // just do it a bit white

			materialPropertyBlock.SetColor("_Color", color);
			//materialPropertyBlock.SetFloat("_Metallic", Random.value);
			//materialPropertyBlock.SetFloat("_Glossiness", Random.value);
			instance.GetComponent<MeshRenderer>().SetPropertyBlock(materialPropertyBlock);

			instance.GetComponent<Rigidbody>().isKinematic = true;
		}

		StartCoroutine(runPhysicsLate());
	}


	private IEnumerator runPhysicsLate()
	{
		yield return new WaitForSeconds(_physicsTimeDelay);

		foreach (Transform child in transform)
		{
			child.GetComponent<Rigidbody>().isKinematic = false;
		}
	}


}

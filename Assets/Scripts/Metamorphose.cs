using System;
using System.Collections.Generic;
using UnityEngine;



public class Metamorphose : MonoBehaviour 
{


	#region Private Classes

	private class Polygon
	{


		public Vector4[] vertices { get; }
		public Vector4[] normals { get; }
		public Vector4[] uvs { get; }


		public Matrix4x4 localToWorld { get; set; }



		public Polygon(Vector4[] vertices, Vector4[] normals, Vector4[] uvs, Matrix4x4 localToWorld)
		{
			this.vertices = vertices;
			this.normals = normals;
			this.uvs = uvs;

			this.localToWorld = localToWorld;
		}

		
		public void calcWorldVerticies(MeshData mesh, int t0, int t1, int t2)
		{
			vertices[0] = localToWorld.MultiplyPoint3x4(mesh.vertices[mesh.triangles[t0]]);
			vertices[1] = localToWorld.MultiplyPoint3x4(mesh.vertices[mesh.triangles[t1]]);
			vertices[2] = localToWorld.MultiplyPoint3x4(mesh.vertices[mesh.triangles[t2]]);
		}


		public void calcWorldNormals(MeshData mesh, int t0, int t1, int t2)
		{
			normals[0] = localToWorld.MultiplyVector(mesh.normals[mesh.triangles[t0]]);
			normals[1] = localToWorld.MultiplyVector(mesh.normals[mesh.triangles[t1]]);
			normals[2] = localToWorld.MultiplyVector(mesh.normals[mesh.triangles[t2]]);
			normals[3] = (normals[0] + normals[1] + normals[2]).normalized;		// Polygon face's normal
		}


		public void calcUVs(MeshData mesh, int t0, int t1, int t2)
		{
			uvs[0] = mesh.uvs[mesh.triangles[t0]];
			uvs[1] = mesh.uvs[mesh.triangles[t1]];
			uvs[2] = mesh.uvs[mesh.triangles[t2]];
		}


	}


	private class MeshData
	{


		public Vector3[] vertices { get; }
		public Vector3[] normals { get; }
		public Vector2[] uvs { get; }
		public int[] triangles { get; }



		public MeshData(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int[] triangles)
		{
			this.vertices = vertices;
			this.normals = normals;
			this.uvs = uvs;
			this.triangles = triangles;
		}


	}

	#endregion

	#region Unity properties

	[SerializeField] private Transform _origin;
	[SerializeField] private Transform _target;
	[SerializeField] private Transform _parent;
	[SerializeField] private Material _material;
	[SerializeField, Range(0.0f, 1.0f)] private float _metamorphoseLevel;

	#endregion

	#region Private fields

	private MaterialPropertyBlock _propertyBlock;
	private List<MeshRenderer> _instances;
	private MeshData _originMeshData;
	private MeshData _targetMeshData;
	private Polygon[] _originPolygons;
	private Polygon[] _targetPolygons;

	private Matrix4x4 _polyPositionsData;
	private Matrix4x4 _polyNormalsData;
	private Matrix4x4 _polyUvsData;

	private int _vertexPositionsShaderPropertyId;
	private int _vertexNormalsShaderPropertyId;
	private int _vertexUvShaderPropertyId;
	private int _metamorphoseLevelShaderPropertyId;

	private float _metamorphoseLevelPrev;
	private Vector4 _interpolationBuffer;

    #endregion



    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
        _instances = new List<MeshRenderer>();

        Debug.Log("Awake");
        spawnInstances();
    }


    private void Start () 
	{
		_vertexPositionsShaderPropertyId = Shader.PropertyToID("_VertexPositions");
		_vertexNormalsShaderPropertyId = Shader.PropertyToID("_VertexNormals");
		_vertexUvShaderPropertyId = Shader.PropertyToID("_VertexUV");
		_metamorphoseLevelShaderPropertyId = Shader.PropertyToID("_MetamorphoseLevel");

		_metamorphoseLevelPrev = _metamorphoseLevel + 0.1f;

	    Debug.Log("Start");
        spawnPolies();
	}


	private void Update()
	{
		updatePolies();
	}


	private void updatePolies()
	{
		if (Math.Abs(_metamorphoseLevel - _metamorphoseLevelPrev) < 0.0001f)
		{
			return;
		}

		_material.SetFloat(_metamorphoseLevelShaderPropertyId, _metamorphoseLevel);

	    for (int instId = 0; instId < _instances.Count; instId++)
		{
			//_originPolygons[instId % _originPolygons.Length].localToWorld = _origin.localToWorldMatrix;
			//_targetPolygons[instId % _targetPolygons.Length].localToWorld = _target.localToWorldMatrix;

			// Set Position data
			Vector4[] buffer0 = _originPolygons[instId % _originPolygons.Length].vertices;
            Vector4[] buffer1 = _targetPolygons[instId % _targetPolygons.Length].vertices;
			
			Vector4 v = lerp(buffer0[0], buffer1[0], _metamorphoseLevel);
			_polyPositionsData.m00 = v.x;
			_polyPositionsData.m10 = v.y;
			_polyPositionsData.m20 = v.z;
			//_polyPositionsData.m30 = 0.0f;

			v = lerp(buffer0[1], buffer1[1], _metamorphoseLevel);
			_polyPositionsData.m01 = v.x;
			_polyPositionsData.m11 = v.y;
			_polyPositionsData.m21 = v.z;
			//_polyPositionsData.m31 = 0.0f;

			v = lerp(buffer0[2], buffer1[2], _metamorphoseLevel);
			_polyPositionsData.m02 = v.x;
			_polyPositionsData.m12 = v.y;
			_polyPositionsData.m22 = v.z;
			//_polyPositionsData.m32 = 0.0f;

			//_polyPositionsData.m03 = 0.0f;
			//_polyPositionsData.m13 = 0.0f;
			//_polyPositionsData.m23 = 0.0f;
			//_polyPositionsData.m33 = 0.0f;

			// Set normals data
			buffer0 = _originPolygons[instId % _originPolygons.Length].normals;
			buffer1 = _targetPolygons[instId % _targetPolygons.Length].normals;

			v = lerp(buffer0[0], buffer1[0], _metamorphoseLevel);
			_polyNormalsData.m00  = v.x;
			_polyNormalsData.m10  = v.y;
			_polyNormalsData.m20  = v.z;
			//_polyNormalsData[3]  = 0.0f;

			v = lerp(buffer0[1], buffer1[1], _metamorphoseLevel);
			_polyNormalsData.m01  = v.x;
			_polyNormalsData.m11  = v.y;
			_polyNormalsData.m21  = v.z;
			//_polyNormalsData[7]  = 0.0f;

			v = lerp(buffer0[2], buffer1[2], _metamorphoseLevel);
			_polyNormalsData.m02 = v.x;
			_polyNormalsData.m12 = v.y;
			_polyNormalsData.m22 = v.z;
			//_polyNormalsData[11] = 0.0f;

			v = lerp(buffer0[3], buffer1[3], _metamorphoseLevel);
			_polyNormalsData.m03 = v.x;
			_polyNormalsData.m13 = v.y;
			_polyNormalsData.m23 = v.z;
			//_polyNormalsData[15] = 0.0f;

			// Set UVs data
			buffer0 = _originPolygons[instId % _originPolygons.Length].uvs;
			buffer1 = _targetPolygons[instId % _targetPolygons.Length].uvs;

			v = lerp(buffer0[0], buffer1[0], _metamorphoseLevel);
			_polyUvsData.m00  = v.x;
			_polyUvsData.m10  = v.y;
			//_polyUvsDa.m02ta[2]  = 0.0f;
			//_polyUvsData[3]  = 0.0f;

			v = lerp(buffer0[1], buffer1[1], _metamorphoseLevel);
			_polyUvsData.m01  = v.x;
			_polyUvsData.m11  = v.y;
			//_polyUvsDa.m02ta[6]  = 0.0f;
			//_polyUvsData[7]  = 0.0f;

			v = lerp(buffer0[2], buffer1[2], _metamorphoseLevel);
			_polyUvsData.m02  = v.x;
			_polyUvsData.m12  = v.y;
			//_polyUvsData[10] = 0.0f;
			//_polyUvsData[11] = 0.0f;

			//_polyUvsData[12] = 0.0f;
			//_polyUvsData[13] = 0.0f;
			//_polyUvsData[14] = 0.0f;
			//_polyUvsData[15] = 0.0f;

			// Set per instance properties
			_propertyBlock.SetMatrix(_vertexPositionsShaderPropertyId, _polyPositionsData);
			_propertyBlock.SetMatrix(_vertexNormalsShaderPropertyId, _polyNormalsData);
			_propertyBlock.SetMatrix(_vertexUvShaderPropertyId, _polyUvsData);
			
			_instances[instId].SetPropertyBlock(_propertyBlock);
		}

		_metamorphoseLevelPrev = _metamorphoseLevel;
	}

	
	private Vector4 lerp(Vector4 a, Vector4 b, float t)
	{
		_interpolationBuffer.x = a.x + t * (b.x - a.x);
		_interpolationBuffer.y = a.y + t * (b.y - a.y);
		_interpolationBuffer.z = a.z + t * (b.z - a.z);
		_interpolationBuffer.w = a.w + t * (b.w - a.w);
		return _interpolationBuffer;
	}


    private void spawnInstances()
    {
        Mesh meshOrigin = getMesh(_origin);
        int originTrianglesLength = meshOrigin.triangles.Length;

        Mesh meshTarget = getMesh(_target);
        int targetTrianglesLength = meshTarget.triangles.Length;

        Mesh pMesh = new Mesh
        {
            vertices = new[]
            {
                Vector3.zero,
                Vector3.up,
                Vector3.left
            }
        };
        pMesh.SetIndices(new[] { 0, 1, 2 }, MeshTopology.Triangles, 0);
        pMesh.RecalculateNormals();

        for (int t = 0; t < Mathf.Max(originTrianglesLength, targetTrianglesLength); t += 3)
        {
            GameObject p = new GameObject();

            Transform instance = p.transform;
            p.AddComponent<MeshFilter>().mesh = pMesh;
            p.AddComponent<MeshRenderer>().sharedMaterial = _material;

            instance.SetParent(_parent);

            MeshRenderer meshRenderer = instance.GetComponent<MeshRenderer>();
            _instances.Add(meshRenderer);
        }
    }

    
	// TODO: Maybe - prebake animated pose to achiev a huge optimization
	private void spawnPolies()
	{
		Mesh meshOrigin = getMesh(_origin, true);
		int originTrianglesLength = meshOrigin.triangles.Length;
		_originMeshData = new MeshData(meshOrigin.vertices, meshOrigin.normals, meshOrigin.uv, meshOrigin.triangles);
		_originPolygons = new Polygon[originTrianglesLength / 3];

		Mesh meshTarget = getMesh(_target, true);
		int targetTrianglesLength = meshTarget.triangles.Length;
		_targetMeshData = new MeshData(meshTarget.vertices, meshTarget.normals, meshTarget.uv, meshTarget.triangles);
		_targetPolygons = new Polygon[targetTrianglesLength / 3];
        
		for(int t = 0, instId = 0; t < Mathf.Max(originTrianglesLength, targetTrianglesLength); t += 3, instId++)
		{
			if (t < originTrianglesLength)
			{
				_originPolygons[instId] = new Polygon(new Vector4[3], new Vector4[4], new Vector4[3], _origin.localToWorldMatrix);
				_originPolygons[instId].calcWorldVerticies(_originMeshData, t, t + 1, t + 2);
				_originPolygons[instId].calcWorldNormals(_originMeshData, t, t + 1, t + 2);
				_originPolygons[instId].calcUVs(_originMeshData, t, t + 1, t + 2);
			}

			if (t < targetTrianglesLength)
			{
				_targetPolygons[instId] = new Polygon(new Vector4[3], new Vector4[4], new Vector4[3], _target.localToWorldMatrix);
				_targetPolygons[instId].calcWorldVerticies(_targetMeshData, t, t + 1, t + 2);
				_targetPolygons[instId].calcWorldNormals(_targetMeshData, t, t + 1, t + 2);
				_targetPolygons[instId].calcUVs(_targetMeshData, t, t + 1, t + 2);
			}

			_propertyBlock.SetMatrix(_vertexPositionsShaderPropertyId, _polyPositionsData);
			_propertyBlock.SetMatrix(_vertexNormalsShaderPropertyId, _polyNormalsData);
			_propertyBlock.SetMatrix(_vertexUvShaderPropertyId, _polyUvsData);
			
		    _instances[instId].SetPropertyBlock(_propertyBlock);
		}
	}


	private Mesh getMesh(Transform tran, bool isBakeMesh = false)
	{
		MeshFilter meshFilter = tran.GetComponent<MeshFilter>();

	    if (meshFilter != null)
	    {
	        return meshFilter.mesh;
	    }

	    SkinnedMeshRenderer skinnedMeshRenderer = tran.GetComponent<SkinnedMeshRenderer>();
        
	    if (skinnedMeshRenderer != null)
	    {
	        if (!isBakeMesh)
	        {
	            return skinnedMeshRenderer.sharedMesh;
	        }

	        Mesh mesh = new Mesh();
	        skinnedMeshRenderer.BakeMesh(mesh);
	        return mesh;
	    }

	    Debug.LogError("One of transform's gameobjects does't have a Mesh");

	    return null;
	}


}

// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."
using UnityEngine;

namespace VIVE.OpenXR.Toolkits.RealisticHandInteraction
{
	/// <summary>
	/// This class is designed to automatically generate indicators.
	/// </summary>
	public class AutoGenIndicator : MonoBehaviour
	{
		private MeshRenderer meshRenderer;
		private MeshFilter meshFilter;

		private readonly Color indicatorColor = new Color(1f, 0.7960785f, 0.09411766f, 1f);
		private const float k_Length = 0.05f;
		private const float k_Width = 0.05f;

		private void Start()
		{
			meshRenderer = transform.gameObject.AddComponent<MeshRenderer>();
			meshFilter = transform.gameObject.AddComponent<MeshFilter>();
			MeshInitialize();
		}

		/// <summary>
		/// Initialize the mesh for the indicator.
		/// </summary>
		private void MeshInitialize()
		{
			Shader shader = Shader.Find("Sprites/Default");
			meshRenderer.material = new Material(shader);
			meshRenderer.material.SetColor("_Color", indicatorColor);
			meshRenderer.sortingOrder = 1;

			Mesh arrowMesh = new Mesh();
			Vector3[] vertices = new Vector3[4];
			int[] triangles = new int[3 * 2];

			vertices[0] = new Vector3(0, 0f, 0f);
			vertices[1] = new Vector3(0f, k_Length * 0.8f, 0f);
			vertices[2] = new Vector3(-k_Width * 0.5f, k_Length, 0f);
			vertices[3] = new Vector3(k_Width * 0.5f, k_Length, 0f);

			triangles[0] = 0;
			triangles[1] = 2;
			triangles[2] = 1;

			triangles[3] = 0;
			triangles[4] = 1;
			triangles[5] = 3;

			arrowMesh.vertices = vertices;
			arrowMesh.triangles = triangles;

			arrowMesh.RecalculateNormals();

			meshFilter.mesh = arrowMesh;
		}

		/// <summary>
		/// Set the pose of the indicator.
		/// </summary>
		/// <param name="position">The position vector to set.</param>
		/// <param name="direction">The direction vector to set.</param>
		public void SetPose(Vector3 position, Vector3 direction)
		{
			transform.position = position;
			transform.up = direction.normalized;
		}
	}
}

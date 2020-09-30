using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathDrawing : MonoBehaviour
{
	[SerializeField] private LineRenderer lineRenderer;

	[Tooltip("Sphere if null"), SerializeField]
	private GameObject prefab;

	[Tooltip("Defines the resolution of the path, the lower the value, the more detail the path will have"), Range(0.1f, 10f), SerializeField]
	private float subdivisionMaxDistance = 1f; //For LineRenderer drawing, you'll want this value to be lower, than with GameObject drawing. Adapt it to the polycount of your environment and NavMesh settings.

	private Stack<GameObject> spawnedGameObjects = new Stack<GameObject>();
	private NavMeshHit hit;

	public void DrawPath(Vector3[] corners, PathRenderingType pathRenderingType)
	{
		var positions = new List<Vector3>(corners);
		SubdividePath(positions);
		ShowPath(positions, pathRenderingType);
	}

	private void SubdividePath(List<Vector3> positions)
	{
		for (int i = 0; i < positions.Count - 1; i++)
		{
			var currentPos = positions[i];
			var nextPos = positions[i + 1];
			var distance = Vector3.Distance(currentPos, nextPos);
			if (distance > subdivisionMaxDistance)
			{
				NavMesh.SamplePosition((currentPos + nextPos) / 2, out hit, 100, NavMesh.AllAreas);
				positions.Insert(i + 1, hit.position);
				SubdividePath(positions);
			}
		}
	}

	private void ShowPath(List<Vector3> positions, PathRenderingType pathRenderingType)
	{
		while (spawnedGameObjects.Count > 0)
			Destroy(spawnedGameObjects.Pop());

		if (pathRenderingType == PathRenderingType.LineRenderer)
		{
			lineRenderer.positionCount = positions.Count;
			lineRenderer.SetPositions(positions.ToArray());
		}
		else
		{
			foreach (var position in positions)
			{
				GameObject instance;
				if (prefab != null)
					instance = Instantiate(prefab); //I recommend implementing object pooling for this drawing method
				else
					instance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				instance.transform.position = position;
				spawnedGameObjects.Push(instance);
			}
		}
	}
}

public enum PathRenderingType
{
	LineRenderer, GameObject
}

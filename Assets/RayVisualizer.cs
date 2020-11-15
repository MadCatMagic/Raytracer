using System.Collections.Generic;
using UnityEngine;

public class RayVisualizer : MonoBehaviour
{
	public Vector3 rayDirection;
	public RayTraceMaster rayTraceMaster;
	public bool getTex = false;
	float maxMarchDistance = 100f;
	Texture3D tex;

	private void OnValidate()
    {
		rayDirection = rayDirection.normalized;
		if (getTex)
        {
			tex = rayTraceMaster.CreateWorldTexture();
			getTex = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
		List<Vector3> intersects = March(rayDirection);
		foreach (Vector3 intersect in intersects)
        {
			Gizmos.color = Color.blue;
			if (tex != null && tex.GetPixel((int)intersect.x, (int)intersect.y, (int)intersect.z).r > 0f)
			//if (false)
            {
				Gizmos.color = Color.green;
            }
			Gizmos.DrawWireCube(intersect + Vector3.one / 2f, Vector3.one);
        }
    }

	List<Vector3> March(Vector3 direction)
	{
		List<Vector3> output = new List<Vector3>();
		Vector3 pos = Vector3Int.FloorToInt(transform.position); // 0, 0, 9
		Vector3 posEnd = Vector3Int.FloorToInt(transform.position + direction * maxMarchDistance);
		Vector3 step = new Vector3(Mathf.Sign(direction.x), Mathf.Sign(direction.y), Mathf.Sign(direction.z)); // 1, 1, -1
		Vector3 tMax = (pos + step - transform.position); // 1, inf, 190
		tMax.x /= direction.x;
		tMax.y /= direction.y;
		tMax.z /= direction.z;
		Vector3 tDelta = new Vector3(1f / direction.x, 1f / direction.y, 1f / direction.z); // 1, inf, 100
		tDelta.x *= step.x;
		tDelta.y *= step.y;
		tDelta.z *= step.z;
		if (direction.x == 0) { tMax.x = maxMarchDistance; tDelta.x = maxMarchDistance; }
		if (direction.y == 0) { tMax.y = maxMarchDistance; tDelta.y = maxMarchDistance; }
		if (direction.z == 0) { tMax.z = maxMarchDistance; tDelta.z = maxMarchDistance; }

		Vector3Int diff = Vector3Int.zero;
		if (pos.x != posEnd.x && direction.x < 0) diff.x--;
		if (pos.y != posEnd.y && direction.y < 0) diff.y--;
		if (pos.z != posEnd.z && direction.z < 0) diff.z--;

		pos += diff;

		for (int marches = 0; marches < 100; marches++)
		{
			if (tMax.x < tMax.y)
			{
				if (tMax.x < tMax.z)
				{
					pos.x = pos.x + step.x;
					tMax.x = tMax.x + tDelta.x;
				}
				else
				{
					pos.z = pos.z + step.z;
					tMax.z = tMax.z + tDelta.z;
				}
			}
			else
			{
				if (tMax.y < tMax.z)
				{
					pos.y = pos.y + step.y;
					tMax.y = tMax.y + tDelta.y;
				}
				else
				{
					pos.z = pos.z + step.z;
					tMax.z = tMax.z + tDelta.z;
				}
			}
			output.Add(pos);
		}
		return output;
	}
}

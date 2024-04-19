using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleDecorator : MonoBehaviour
{
    public Transform itemRef;
    public int frequency;
    public float radius;
	public bool lookAtCenter;
	private void Awake () 
	{
		init();
	}

	public void init()
	{
		if (frequency <= 0 || itemRef == null) {
			return;
		}

        float stepSize = 360f / frequency;

		for (int i = 0; i < frequency; i++) 
        {
			Transform item = Instantiate(itemRef) as Transform;
                
			//Vector3 position = spline.GetPoint(p * stepSize);
            var angle = i * Mathf.PI * 2 / frequency;
            Vector3 position = new Vector3 
            (
                radius * Mathf.Cos(angle),
                0f,
                radius * Mathf.Sin(angle)
            );

			item.transform.localPosition = position;
			if (lookAtCenter) 
            {
				item.transform.LookAt(Vector3.zero);
			}
			item.transform.SetParent(transform);
		}
	}
}

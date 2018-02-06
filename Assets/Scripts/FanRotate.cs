using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanRotate : MonoBehaviour {

	public float rotSpeed;

	void Start () {
		rotSpeed = 0f;
	}
	

	void Update () {
		rotSpeed += 10f;
		if (rotSpeed > 360f)
		{
			rotSpeed -= 360;
		}
		transform.localRotation = Quaternion.Euler(new Vector3(0f,0f, rotSpeed));
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blow : MonoBehaviour {

	public float blowForce = 1000f;

	public void OnTriggerStay(Collider other)
	{
        Vector3 forceVector = -1f * transform.parent.forward;
        if (other.attachedRigidbody)
        {
            other.attachedRigidbody.AddForce(forceVector * blowForce, ForceMode.Force);
        }
	}

}

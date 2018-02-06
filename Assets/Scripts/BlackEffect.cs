using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class BlackEffect : MonoBehaviour {

	//public bool fadeComplete;
	public float intensity = 0f;
	//public float speed = 0.2f;
	private Material material;

	// Creates a private material used to the effect
	void Awake ()
	{
		material = new Material( Shader.Find("Hidden/FadeEffect") );
		//fadeComplete = false;
	}

	// Postprocess the image
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
//		if (intensity == 0)
//		{
//			Graphics.Blit (source, destination);
//			return;
//		}

		//intensity = Mathf.Lerp (0f, 1f, speed * Time.time);

		material.SetFloat("_bwBlend", intensity);
		Graphics.Blit (source, destination, material);


	}
}

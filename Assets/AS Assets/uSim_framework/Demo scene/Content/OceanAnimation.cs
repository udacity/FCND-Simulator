using UnityEngine;
using System.Collections;

public class OceanAnimation : MonoBehaviour {

	public float scrollSpeed = 0.05F;
	public float scrollSpeed2 = 0.005F;
	public Renderer rend;
	void Start() {
		rend = GetComponent<Renderer>();
	}
	void Update() {
		float offset = Time.time * scrollSpeed;
		rend.material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
		rend.material.SetTextureOffset("_BumpMap", new Vector2(offset, 0));
		float offset2 = Time.time * scrollSpeed2;
		rend.material.SetTextureOffset("_Detail", new Vector2(offset2, 0));

	}
}

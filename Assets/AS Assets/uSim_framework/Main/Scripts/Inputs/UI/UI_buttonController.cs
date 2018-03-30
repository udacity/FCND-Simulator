using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UI_buttonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

	public float inputValue;
	//-1 | 1
	public int sign;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	


	}

	public void OnPointerDown(PointerEventData eventData){
		inputValue = 1f * sign;
	}
	public void OnPointerUp(PointerEventData eventData){
		inputValue = 0f;
	}
}

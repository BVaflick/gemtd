using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupText : GameBehavior {
	
	private Transform target;

	private Vector3 targetPosition;

	private Camera camera;
	
	private float age;

	private Text Text => GetComponentInChildren<Text>();

	public void Initialize(Transform target, int damage, Camera camera) {
		this.target = target;
		this.camera = camera;
		Text.text = damage + "";
	}

	public override bool GameUpdate() {
		age += Time.deltaTime;
		if (age >= 0.5f) {
			Recycle();
			return false;
		}
		int fontSize = 20 - (int) camera.transform.position.y;
		if (fontSize < 10) fontSize = 10;
		Text.fontSize = fontSize;
		if (target) {
			Vector3 pos = target.transform.position;
			// pos.z += 1f;
			targetPosition = pos;
		}
		transform.position = camera.WorldToScreenPoint(targetPosition);
		
		return true;
	}

	public override void Recycle() {
		Destroy(gameObject);
	}
}
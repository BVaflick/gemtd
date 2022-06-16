using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {
	[SerializeField]
	Slider slider;

	private Camera camera;

	private Enemy enemy;

	public void setMaxValue(int maxValue) {
		slider.maxValue = maxValue;
		slider.value = maxValue;
	}

	public void setValue(int value) {
		slider.value = value;
	}

	// public void Initialize(Enemy enemy) {
	// 	slider.maxValue = enemy.Health;
	// 	slider.value = enemy.Health;
	// 	this.enemy = enemy;
	// 	camera = Camera.main;
	// }
	//
	// public override bool GameUpdate() {
	// 	// Vector3 pos = target.transform.position;
	// 	// // pos.z += 1f;
	// 	// targetPosition = pos;
	// 	int zoom = (int) camera.transform.position.y;
	// 	float x = 120 / (zoom / 5f);
	// 	float y = 5 + (5 / (zoom / 5f));
	// 	GetComponent<RectTransform>().sizeDelta = new Vector2(x, 10);
	// 	transform.position = camera.WorldToScreenPoint(enemy.transform.position);
	//
	// 	return true;
	// }
	//
	// public override void Recycle() {
	// 	Destroy(gameObject);
	// }
}
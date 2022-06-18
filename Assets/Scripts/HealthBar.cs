using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {
	[SerializeField]
	Slider slider;

	private Camera camera;

	private Enemy enemy;

	private int zoom;

	public void setMaxValue(int maxValue) {
		slider.maxValue = maxValue;
		slider.value = maxValue;
	}

	public void setValue(int value) {
		slider.value = value;
	}

	public void Initialize(Enemy enemy) {
		slider.maxValue = enemy.Health;
		slider.value = enemy.Health;
		this.enemy = enemy;
		camera = Camera.main;
		zoom = (int) camera.transform.position.y; //от 5 до 16. Чем меньше, тем ниже камера
		GetComponent<RectTransform>().sizeDelta = new Vector2(20 + 90 / (zoom / 5f), 6 + 5 / (zoom / 5f));
	}

	public void GameUpdate() {
		if((int) enemy.Health != (int) slider.value) setValue((int) enemy.Health);
		if (zoom != (int) camera.transform.position.y) {
			zoom = (int) camera.transform.position.y; //от 5 до 16. Чем меньше, тем ниже камера
			GetComponent<RectTransform>().sizeDelta = new Vector2(20 + 90 / (zoom / 5f), 6 + 5 / (zoom / 5f));
		}
		Vector3 pos = camera.WorldToScreenPoint(enemy.transform.position);
		pos.y += 70 / (zoom / 5f);
		pos.x += (pos.x - Screen.width / 2f) / 20;
		transform.position = pos; 
	}
	
	public void Recycle() {
		Destroy(gameObject);
	}
}
using UnityEngine;
using UnityEngine.UI;

public class PopupText : GameBehavior {
	
	private Vector3 position;
	
	public Enemy enemy;

	private Camera camera;
	
	private Text Text => GetComponentInChildren<Text>();
	
	public void Initialize(Enemy enemy, int damage) {
		this.enemy = enemy;
		Vector3 pos = enemy.transform.position;
		float x = pos.x;
		float y = pos.y;
		float z = pos.z;
		position = new Vector3(x,y,z);
		camera = Camera.main;
		Text.text = damage + "";
		Destroy(gameObject, .5f);
	}

	public override bool GameUpdate() {
		transform.position = camera.WorldToScreenPoint(position);
		int fontSize = 20 - (int) camera.transform.position.y;
		if (fontSize < 10) fontSize = 10;
		Text.fontSize = fontSize;
		return true;
	}

	public override void Recycle() {
		Destroy(gameObject);
	}
}
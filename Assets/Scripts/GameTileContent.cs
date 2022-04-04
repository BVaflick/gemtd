using UnityEngine;

public class GameTileContent : MonoBehaviour {

	[SerializeField]
	GameTileContentType type = default;
	[SerializeField]
	Transform selection = default;

	GameTileContentFactory originFactory;

	public GameTileContentType Type => type;

	public bool BlocksPath => Type == GameTileContentType.Wall || Type == GameTileContentType.Tower;

	public virtual void GameUpdate () {}

	public GameTileContentFactory OriginFactory {
		get => originFactory;
		set {
			Debug.Assert(originFactory == null, "Redefined origin factory!");
			originFactory = value;
		}
	}

	public void Recycle () {
		originFactory.Reclaim(this);
	}	
	
	public void switchSelection() {
		selection.gameObject.SetActive(!selection.gameObject.activeSelf);
	}
}
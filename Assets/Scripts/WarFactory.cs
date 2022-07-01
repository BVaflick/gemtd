using UnityEngine;

[CreateAssetMenu]
public class WarFactory : GameObjectFactory {

	[SerializeField]
	Shell shellPrefab = default;

	[SerializeField]
	Explosion explosionPrefab = default;
	
	[SerializeField]
	Fire firePrefab = default;

	[SerializeField]
	Toxin toxinPrefab = default;

	[SerializeField]
	Ice icePrefab = default;

	[SerializeField]
	Corrosion corrosionPrefab = default;

	public Explosion Explosion => Get(explosionPrefab);
	
	public Fire Fire => Get(firePrefab);

	public Ice Ice => Get(icePrefab);

	public Toxin Toxin => Get(toxinPrefab);

	public Shell Shell => Get(shellPrefab);

	public Corrosion Corrosion => Get(corrosionPrefab);

	T Get<T> (T prefab) where T : WarEntity {
		T instance = CreateGameObjectInstance(prefab);
		instance.OriginFactory = this;
		return instance;
	}

	public void Reclaim (WarEntity entity) {
		Debug.Assert(entity.OriginFactory == this, "Wrong factory reclaimed!");
		Destroy(entity.gameObject);
	}
}
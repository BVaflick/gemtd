using UnityEngine;

[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory {

	[System.Serializable]
	class EnemyConfig {

		public Enemy prefab = default;

		[FloatRangeSlider(0.1f, 2f)]
		public FloatRange scale = new FloatRange(0.4f, 0.6f);

		[FloatRangeSlider(0.2f, 5f)]
		public FloatRange speed = new FloatRange(2f);

		[FloatRangeSlider(-0.4f, 0.4f)]
		public FloatRange pathOffset = new FloatRange(0f);

		[FloatRangeSlider(10f, 5000f)]
		public FloatRange health = new FloatRange(50f);

		[FloatRangeSlider(0f, 1000f)]
		public FloatRange armor = new FloatRange(5f);
	}

	[SerializeField]
	EnemyConfig knight = default, bee = default, large = default, bug = default, chest = default, chomper = default, mushroom = default, orc = default, spitter = default, turtle = default;

	EnemyConfig GetConfig(EnemyType type) {
		switch (type) {
			case EnemyType.Knight:
				return knight;
			case EnemyType.Bee:
				return bee;
			case EnemyType.Large:
				return large;
			case EnemyType.Bug:
				return bug;
			case EnemyType.Chest:
				return chest;
			case EnemyType.Chomper:
				return chomper;
			case EnemyType.Mushroom:
				return mushroom;
			case EnemyType.Orc:
				return orc;
			case EnemyType.Spitter:
				return spitter;
			case EnemyType.Turtle:
				return turtle;
		}
		Debug.Assert(false, "Unsupported enemy type!");
		return null;
	}

	public Enemy Get(EnemyType type = EnemyType.Bee) {
		EnemyConfig config = GetConfig(type);
		Enemy instance = CreateGameObjectInstance(config.prefab);
		instance.OriginFactory = this;
		instance.Initialize(
			config.scale.RandomValueInRange,
			config.speed.RandomValueInRange,
			config.pathOffset.RandomValueInRange,
			config.health.RandomValueInRange,
			config.armor.RandomValueInRange
		);
		return instance;
	}

	public void Reclaim(Enemy enemy) {
		Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
		enemy.VisualEffects.Clear();
		Destroy(enemy.gameObject);
	}
}
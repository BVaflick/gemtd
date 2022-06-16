using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory {

	[System.Serializable]
	public class EnemyConfig {

		public Enemy prefab = default;

		public EnemyType type = default;

		[FloatRangeSlider(0.1f, 2f)]
		public FloatRange scale = new FloatRange(0.4f, 0.6f);

		[FloatRangeSlider(0.2f, 5f)]
		public FloatRange speed = new FloatRange(2f);

		[FloatRangeSlider(1f, 5000f)]
		public FloatRange health = new FloatRange(50f);

		[FloatRangeSlider(0f, 1000f)]
		public FloatRange armor = new FloatRange(5f);
	}

	[SerializeField]
	List<EnemyConfig> enemies = default;

	EnemyConfig GetConfig(EnemyType type) {
		return enemies.Find(enemy => enemy.type == type);
	}

	public Enemy Get(EnemyType type, int wave) {
		EnemyConfig config = GetConfig(type);
		Enemy instance = CreateGameObjectInstance(config.prefab);
		instance.OriginFactory = this;
		instance.Initialize(
			config.scale.RandomValueInRange,
			config.speed.RandomValueInRange,
			(1 + wave) * 20,
			wave
			// config.health.RandomValueInRange,
			// config.armor.RandomValueInRange
		);
		return instance;
	}

	public void Reclaim(Enemy enemy) {
		Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
		enemy.VisualEffects.Clear();
		Destroy(enemy.gameObject);
	}
}
using UnityEngine;

[System.Serializable]
public class EnemySpawnSequence {
	[SerializeField]
	EnemyFactory factory = default;

	[SerializeField]
	EnemyType type = EnemyType.Bee;

	[SerializeField, Range(1, 100)]
	int amount = 1;

	[SerializeField, Range(0.1f, 10f)]
	float cooldown = 1f;

	public State Begin() => new State(this);

	[System.Serializable]
	public struct State {
		EnemySpawnSequence sequence;

		int count;

		private int amount;

		float cooldown;

		public State(EnemySpawnSequence sequence) {
			this.sequence = sequence;
			count = 0;
			amount = Game.getCurrentProgress();
			cooldown = sequence.cooldown;
		}

		public bool Progress(float deltaTime) {
			if (count >= amount) return false;
			
			cooldown += deltaTime;
			if (cooldown >= sequence.cooldown) {
				cooldown -= sequence.cooldown;
				count += 1;
				Game.SpawnEnemy(sequence.factory, sequence.type);
			}

			return true;
		}
	}
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameScenario : ScriptableObject {

    [SerializeField]
    List<EnemySpawnSequence> waves = new List<EnemySpawnSequence>();

    [SerializeField, Range(0, 10)]
    int cycles = 1;

    public State Begin() => new State(this);

    [System.Serializable]
    public struct State {

        GameScenario scenario;

        int cycle, index;

        bool waveIsInProgress;

        public int CurrentWave() => index;

        public bool WaveIsInProgress() => waveIsInProgress;

        EnemySpawnSequence.State wave;

        public State(GameScenario scenario) {
            this.scenario = scenario;
            cycle = 0;
            index = 0;
            waveIsInProgress = false;
            Debug.Assert(scenario.waves.Count > 0, "Empty scenario!");
            wave = scenario.waves[0].Begin();
        }

        public bool Progress() {
            waveIsInProgress = wave.Progress(Time.deltaTime);
            if (!waveIsInProgress) {
                if (index >= scenario.waves.Count - 1) {
                    return false;                    
                }
            }
            return true;
        }

        public void NextWave() {
            if (index != scenario.waves.Count - 1) {
                wave = scenario.waves[++index].Begin();
            }
        }
    }
}
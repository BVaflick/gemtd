using UnityEngine;

[System.Serializable]
public abstract class Buff : ScriptableObject {

    public string name1 { get; set; }

    public int level { get; set; }
}
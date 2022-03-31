using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public abstract class Buff : ScriptableObject {

    public string name1 { get; set; }

    public int level { get; set; }
    
    public Sprite icon { get; set; }
}
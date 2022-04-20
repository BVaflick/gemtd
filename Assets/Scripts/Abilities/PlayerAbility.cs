using UnityEngine;
using UnityEngine.Events;

public class PlayerAbility : MonoBehaviour {

    [SerializeField, Range(1, 4)]
    protected int level;

    [SerializeField]
    public UnityEvent action = default;
    
    [SerializeField]
    public Sprite icon = default;
}
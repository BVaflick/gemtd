using UnityEngine;

public class Corrosion : WarEntity {

    [SerializeField, Range(0f, 10f)]
    float duration = 6f;    

    public override bool GameUpdate() {
        age += Time.deltaTime;
        if (age >= duration || target == null) {
            OriginFactory.Reclaim(this);
            return false;
        }
        target.Enemy.additionalArmor -= 10f;   
        return true;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Damage : TowerBuff {

   public override void Modify(Tower tower) {
      tower.Damage = 10f;
   }

}

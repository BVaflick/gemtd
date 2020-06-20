using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splitshot : TowerBuff {

   public override void Modify(Tower tower) {
      tower.TargetNumber = 4 * level;
   }

}

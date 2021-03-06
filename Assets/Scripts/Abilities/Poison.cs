﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Poison : EnemyBuff {

   public override void Modify(TargetPoint target, float damage) {
      WarEntity b = (WarEntity) target.Enemy.Effects.Behaviors.Find(effect => ((WarEntity) effect).name1 == this.GetType().Name + level);
      if (b != null) {
         b.age = 0f;
      } else {
         Toxin toxin = Game.SpawnToxin();
         toxin.Initialize(target, this.GetType().Name + level);
         target.Enemy.Effects.Add(toxin);
      }      
   }
}
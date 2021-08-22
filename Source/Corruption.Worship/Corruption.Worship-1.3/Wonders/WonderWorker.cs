using Corruption.Core.Gods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Corruption.Worship.Wonders
{
   public class WonderWorker
    {
        public WonderDef Def;
        
        public virtual bool TryExecuteWonderInt(GodDef god, int worshipPoints) { return false; }
        public bool TryExecuteWonder(GodDef god, int worshipPoints)
        {
            var activated = this.TryExecuteWonderInt(god, worshipPoints);
            if (activated)
            {
                GlobalWorshipTracker.Current.GlobalWonderCooldownTicks[this.Def] = this.Def.CooldownTicks;
            }
            return activated;
        }




    }
}

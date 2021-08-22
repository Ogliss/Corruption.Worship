using Corruption.Core;
using Corruption.Core.Gods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_CleansePossession : WonderWorker_Targetable
    {
        protected override void TryDoEffectOnTarget(int worshipPoints)
        {
            Pawn pawn = this.target.Thing as Pawn;
            if (pawn != null)
            {
                PossessionUtiltiy.TryRemovePossession(pawn, 1f);
            }
        }
    }
}

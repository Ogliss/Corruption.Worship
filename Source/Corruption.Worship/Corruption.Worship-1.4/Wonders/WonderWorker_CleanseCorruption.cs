using Corruption.Core;
using Corruption.Core.Gods;
using Corruption.Core.Soul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_CleanseCorruption : WonderWorker_Targetable
    {
        protected override bool TryDoEffectOnTarget(GodDef god, int worshipPoints)
        {
            Pawn pawn = this.target.Thing as Pawn;
            if (pawn != null)
            {
                pawn.Soul()?.GainCorruption(god.favourCorruptionFactor * worshipPoints, null, false);
               return PossessionUtiltiy.TryRemovePossession(pawn, 1f);
            }
            return false;
        }
    }
}

using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_HealAllPawns : WonderWorker
    {
        public override bool TryExecuteWonderInt(GodDef god, int worshipPoints)
        {
            List<Pawn> AllColonists = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.ToList();

            for (int i = 0; i < AllColonists.Count; i++)
            {
                List<Hediff> hediffs = AllColonists[i].health.hediffSet.hediffs;
                for (int j = 0; j < hediffs.Count; j++)
                {
                    hediffs[j].Heal(50f);
                }
            }
            return true;        
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Corruption.Core.Gods;
using RimWorld;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_AddHediff : WonderWorker_Targetable
    {
        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters()
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                canTargetItems = false,
                canTargetLocations = false
            };
        }

        protected override bool TryDoEffectOnTarget(GodDef god, int worshipPoints)
        {
            Pawn pawn = (Pawn)this.target.Thing;
            for (int i = 0; i < this.Def.hediffsToAdd.Count; i++)
            {
                pawn?.health.AddHediff(this.Def.hediffsToAdd[i]);
            }
            return pawn != null;
        }
    }

    public class WonderWorker_AddHediffMap : WonderWorker_Targetable
    {
        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters()
            {
                canTargetPawns = true,
                canTargetBuildings = true,
                canTargetItems = true,
                canTargetLocations = true
            };
        }

        protected override bool TryDoEffectOnTarget(GodDef god, int worshipPoints)
        {
            Map map = this.target.Map;
            foreach (var colonist in map.mapPawns.FreeColonistsSpawned)
            {
                for (int i = 0; i < this.Def.hediffsToAdd.Count; i++)
                {
                    colonist.health.AddHediff(this.Def.hediffsToAdd[i]);
                }
            }
            return true;
        }
    }
}

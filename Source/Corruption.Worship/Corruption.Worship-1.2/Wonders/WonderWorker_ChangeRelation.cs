using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_ChangeRelation : WonderWorker_Targetable
    {
        protected override bool TryDoEffectOnTarget(GodDef god, int worshipPoints)
        {
            Pawn pawn = this.target.Thing as Pawn;
            if (pawn != null && pawn.Faction != Faction.OfPlayer)
            {
                float change = this.Def.ResolveWonderPoints(this.target.Map, worshipPoints);
                return pawn.Faction.TryAffectGoodwillWith(Faction.OfPlayer, (int)change);

            }
            else
            {
                Messages.Message("MessageInvalidWonderTargetDesc".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return GlobalWorshipTracker.Current.TryAddFavor(god, worshipPoints);
            }

        }

    }
}

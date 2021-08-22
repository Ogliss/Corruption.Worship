using Corruption.Core.Gods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_RaisePassion : WonderWorker_Targetable
    {
        protected override bool TryDoEffectOnTarget(GodDef god, int worshipPoints)
        {
            Pawn pawn = this.target.Thing as Pawn;
            if (pawn != null)
            {
                var nonPassionateSkills = pawn.skills.skills.Where(x => x.passion < RimWorld.Passion.Major);
                if (nonPassionateSkills.Count() > 0)
                {
                    nonPassionateSkills.RandomElement().passion = RimWorld.Passion.Major;
                    return true;
                }
            }
            return false;
        }

    }
}

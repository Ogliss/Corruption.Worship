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
    public class WonderWorker_AmbitiousShuffle : WonderWorker_Targetable
    {
        protected override bool TryDoEffectOnTarget(GodDef god, int worshipPoints)
        {
            Pawn pawn = this.target.Thing as Pawn;
            if (pawn != null)
            {
                float siphonedPoints = 1000f;
                foreach (var skill in pawn.skills.skills.Where(x => x.def != SkillDefOf.Social && x.def != SkillDefOf.Intellectual))
                {
                    skill.Level -= 1;
                    siphonedPoints += skill.xpSinceLastLevel + SkillRecord.XpRequiredToLevelUpFrom(skill.Level);
                }

                pawn.skills.Learn(SkillDefOf.Social, siphonedPoints / 2f);
                pawn.skills.Learn(SkillDefOf.Intellectual, siphonedPoints / 2f);
                return true;
            }
            return false;
        }
    }
}

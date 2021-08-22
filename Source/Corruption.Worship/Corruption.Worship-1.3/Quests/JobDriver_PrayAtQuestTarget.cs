using Corruption.Core;
using Corruption.Core.Soul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Corruption.Worship.Quests
{
    public class JobDriver_PrayAtQuestTarget : JobDriver_PrayToShrine
    {
        protected override void AddPrayerFinish(CompSoul soul, Toil lastToil)
        {
            base.AddPrayerFinish(soul, lastToil);
            QuestUtility.SendQuestTargetSignals(this.Shrine.questTags, "QuestPrayerOffered", this.Named("SUBJECT"));
            QuestUtility.SendQuestTargetSignals(this.pawn.questTags, "QuestPrayerOffered", this.pawn.Named("SUBJECT"));
            MoteMaker.MakeStaticMote(soul.parent.Position, soul.parent.Map, CoreThingDefOf.Mote_HolyExorcise);
        }

    }
}

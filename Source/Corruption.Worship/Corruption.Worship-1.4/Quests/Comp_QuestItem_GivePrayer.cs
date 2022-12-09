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
    public class Comp_QuestItem_GivePrayer : ThingComp
    {
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            Job job = null;
            job = JobMaker.MakeJob(WorshipJobDefOf.Corruption_PrayAtPilgrimage);
            job.targetA = this.parent.InteractionCell;
            job.targetB = this.parent;


            yield return new FloatMenuOption("PrayAtSiteOfPilgrimage".Translate(parent.Label).CapitalizeFirst(), delegate
            {
                selPawn.jobs.TryTakeOrderedJob(job);
            });
        }
    }
}


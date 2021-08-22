using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.Worship
{
    public class JobGiver_HoldSermon : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            Lord lord = pawn.GetLord();
            LordJob_Sermon lordJob = lord?.LordJob as LordJob_Sermon;
            if (lordJob != null)
            {
                    return new Job(WorshipJobDefOf.HoldSermon, lordJob.altar, lordJob.altar.InteractionCell);                
            }
            return null;
        }
    }
}

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
    public class JobGiver_AssistSermon : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            Lord lord = pawn.GetLord();
            LordJob_Sermon lordJob = lord?.LordJob as LordJob_Sermon;
            if (lordJob != null)
            {
                return new Job(WorshipJobDefOf.AssistPreacher, lordJob.altar);
            }
            return null;
        }
    }
}

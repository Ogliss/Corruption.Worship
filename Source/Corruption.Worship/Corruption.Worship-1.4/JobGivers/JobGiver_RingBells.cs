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
    public class JobGiver_RingBells : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            Lord lord = pawn.GetLord();
            LordJob_Joinable_Sermon lordJob = lord?.LordJob as LordJob_Joinable_Sermon;
            if (lordJob != null)
            {
                var belltowers = pawn.Map.listerBuildings.allBuildingsColonist.Where(x => x.TryGetComp<CompBellTower>() != null);
                if (belltowers.Count() > 0)
                {
                    var belltower = belltowers.MinBy(x => x.Position.DistanceTo(pawn.Position));
                    return new Job(WorshipJobDefOf.Corruption_RingBellTower, belltower);
                }
            }
            return null;
        }
    }
}

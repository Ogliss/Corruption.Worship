using Corruption.Core.Soul;
using System.Collections.Generic;
using Verse;

namespace Corruption.Worship
{
    public class RitualWorker_Interrogation : RitualWorker
    {
        public RitualWorker_Interrogation(RitualDef def) : base(def)
        {
        }

        public override void FinishRitual(Pawn ritualLeader, LocalTargetInfo localTargetInfo, List<Pawn> participants)
        {
            Pawn pawn = localTargetInfo.Pawn;
            if (pawn != null)
            {
                var soul = pawn.Soul();
                if (soul != null && !soul.KnownToPlayer)
                {
                    soul.KnownToPlayer = true;
                }
            }
        }
    }
}

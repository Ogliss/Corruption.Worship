using System.Collections.Generic;
using Verse;

namespace Corruption.Worship
{
    public abstract class RitualWorker
    {
        public RitualWorker(RitualDef def)
        {
            this.Def = def;
        }

        public RitualDef Def;

        public abstract void FinishRitual(Pawn ritualLeader, LocalTargetInfo localTargetInfo, List<Pawn> participants);
    }
}

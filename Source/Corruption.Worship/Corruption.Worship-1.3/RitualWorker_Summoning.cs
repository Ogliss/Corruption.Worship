using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Corruption.Worship
{
    public class RitualWorker_Summoning : RitualWorker
    {
        public RitualWorker_Summoning(RitualDef def) : base(def)
        {
        }

        public override void FinishRitual(Pawn ritualLeader, LocalTargetInfo localTargetInfo, List<Pawn> participants)
        {
            var pawns = this.Def.pawnGroupMaker.GeneratePawns(this.Def.pawnGroupMakerParms);
            if (pawns.Any())
            {
                MoteMaker.MakeStaticMote(localTargetInfo.Cell, ritualLeader.Map, this.Def.effectMote);

                foreach (var pawn in pawns)
                {
                    GenSpawn.Spawn(pawn, localTargetInfo.Cell, ritualLeader.Map, WipeMode.VanishOrMoveAside);
                }
            }
        }
    }
}

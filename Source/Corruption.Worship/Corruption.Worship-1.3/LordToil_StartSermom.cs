using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Corruption.Worship
{
    public class LordToil_StartSermom : LordToil_Speech
    {
        private Pawn preacher;

        private BuildingAltar altar;

        // GatheringDefOf.Sermon
        public LordToil_StartSermom(Pawn organizer, BuildingAltar altar, GatheringDef ritual, LordJob_Ritual lordJob) : base(altar.Position, ritual, organizer)
        {
            this.preacher = organizer;
            this.altar = altar;
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                Pawn pawn = lord.ownedPawns[i];
                if (pawn == preacher)
                {
                    pawn.mindState.duty = new PawnDuty(WorshipDutyDefOf.Corruption_HoldSermon, altar.InteractionCell, altar);
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
                else
                {
                    PawnDuty pawnDuty = pawn == altar.CurrentActiveSermon.Assistant ? new PawnDuty(WorshipDutyDefOf.Corruption_AssistSermon, altar) : new PawnDuty(WorshipDutyDefOf.Corruption_AttendSermon, altar);
                    pawnDuty.spectateRect = Data.spectateRect;
                    pawnDuty.spectateRectAllowedSides = Data.spectateRectAllowedSides;
                    pawnDuty.spectateRectPreferredSide = Data.spectateRectPreferredSide;
                    pawn.mindState.duty = pawnDuty;
                }
            }
        }

        public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
        {
            if (p == this.preacher)
            {
                return WorshipDutyDefOf.Corruption_HoldSermon.hook;
            }
            return WorshipDutyDefOf.Corruption_AttendSermon.hook;
        }
    }
}

using RimWorld;
using Verse;
using Verse.AI;

namespace Corruption.Worship
{
    public class LordToil_Speech : LordToil_Gathering
    {
        public LordToilData_Speech Data
        {
            get
            {
                return (LordToilData_Speech)this.data;
            }
        }

        // Token: 0x06003443 RID: 13379 RVA: 0x00121ECA File Offset: 0x001200CA
        public LordToil_Speech(IntVec3 spot, GatheringDef gatheringDef, Pawn organizer) : base(spot, gatheringDef)
        {
            this.organizer = organizer;
            this.data = new LordToilData_Speech();
        }

        // Token: 0x06003444 RID: 13380 RVA: 0x00121EE8 File Offset: 0x001200E8
        public override void Init()
        {
            base.Init();
            this.Data.spectateRect = CellRect.CenteredOn(this.spot, 0);
            Rot4 rotation = this.spot.GetFirstThing(this.organizer.MapHeld, null).Rotation;
            SpectateRectSide asSpectateSide = rotation.Opposite.AsSpectateSide;
            this.Data.spectateRectAllowedSides = (SpectateRectSide.All & ~asSpectateSide);
            this.Data.spectateRectPreferredSide = rotation.AsSpectateSide;
        }

        // Token: 0x06003445 RID: 13381 RVA: 0x00121F60 File Offset: 0x00120160
        public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
        {
            if (p == this.organizer)
            {
                return WorshipDutyDefOf.GiveSpeech.hook;
            }
            return WorshipDutyDefOf.Spectate.hook;
        }

        // Token: 0x06003446 RID: 13382 RVA: 0x00121F80 File Offset: 0x00120180
        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                Pawn pawn = this.lord.ownedPawns[i];
                if (pawn == this.organizer)
                {
                    Building_Throne firstThing = this.spot.GetFirstThing(base.Map, null) as Building_Throne;
                    pawn.mindState.duty = new PawnDuty(WorshipDutyDefOf.GiveSpeech, this.spot, firstThing, -1f);
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
                }
                else
                {
                    PawnDuty pawnDuty = new PawnDuty(WorshipDutyDefOf.Spectate);
                    pawnDuty.spectateRect = this.Data.spectateRect;
                    pawnDuty.spectateRectAllowedSides = this.Data.spectateRectAllowedSides;
                    pawnDuty.spectateRectPreferredSide = this.Data.spectateRectPreferredSide;
                    pawn.mindState.duty = pawnDuty;
                }
            }
        }

        // Token: 0x04001C2B RID: 7211
        public Pawn organizer;
    }
}

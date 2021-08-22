using Corruption.Core.Soul;
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
    public class LordJob_Sermon : LordJob_Joinable_Speech
    {
        public BuildingAltar altar;

        public bool Ending;

        public LordJob_Sermon() { }

        public LordJob_Sermon(BuildingAltar altar, IntVec3 spot, Pawn organizer, GatheringDef gatheringDef) : base(spot, organizer, gatheringDef)
        {
            this.altar = altar;
        }

        public override void ApplyOutcome(float progress)
        {
            this.Ending = true;
            if (progress < 0.5f)
            {
                Messages.Message("MessageSermonCancelled".Translate(), MessageTypeDefOf.RejectInput, false);
                this.altar.EndSermon();
                return;
            }
            ThoughtDef key = OutcomeThoughtChances.RandomElementByWeight((KeyValuePair<ThoughtDef, float> t) => (!PositiveOutcome(t.Key)) ? OutcomeThoughtChances[t.Key] : (OutcomeThoughtChances[t.Key] * organizer.GetStatValue(StatDefOf.SocialImpact) * progress)).Key;
            foreach (Pawn ownedPawn in lord.ownedPawns)
            {
                if (ownedPawn != organizer && organizer.Position.InHorDistOf(ownedPawn.Position, 18f))
                {
                    ownedPawn.needs.mood.thoughts.memories.TryGainMemory(key, organizer);
                }
            }
            SermonUtility.HoldSermonTickCheckEnd(this.organizer, lord.ownedPawns, altar.CurrentActiveSermon.DedicatedTo, this.altar);
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil lordToil = CreateGatheringToil(spot, organizer, gatheringDef);
            stateGraph.AddToil(lordToil);
            LordToil_End lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);
            float speechDuration = altar.CurrentActiveSermon.SermonDurationHours * GenDate.TicksPerHour;
            Transition transition = new Transition(lordToil, lordToil_End);
            transition.AddTrigger(new Trigger_TickCondition(ShouldBeCalledOff));
            transition.AddTrigger(new Trigger_PawnKilled());
            transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.LeftVoluntarily, organizer));
            transition.AddPreAction(new TransitionAction_Custom((Action)delegate
            {
                ApplyOutcome((float)lord.ticksInToil / speechDuration);
            }));
            stateGraph.AddTransition(transition);
            timeoutTrigger = new Trigger_TicksPassedAfterConditionMet((int)speechDuration, () => GatheringsUtility.InGatheringArea(organizer.Position, spot, organizer.Map), 60);
            Transition transition2 = new Transition(lordToil, lordToil_End);
            transition2.AddTrigger(timeoutTrigger);
            transition2.AddPreAction(new TransitionAction_Custom((Action)delegate
            {
                ApplyOutcome((float)lord.ticksInToil / speechDuration);
            }));
            transition2.AddTrigger(new Trigger_Memo("ForceEndSermon"));
            stateGraph.AddTransition(transition2);
            return stateGraph;
        }

        public override float VoluntaryJoinPriorityFor(Pawn p)
        {
            if (!GatheringsUtility.ShouldPawnKeepGathering(p, gatheringDef))
            {
                return 0f;
            }
            if (spot.IsForbidden(p))
            {
                return 0f;
            }
            if (!lord.ownedPawns.Contains(p) && IsGatheringAboutToEnd())
            {
                return 0f;
            }

            float devotionFactor = p.Soul()?.DevotionDegree * 5 ?? 0;
            float result = 100f + VoluntarilyJoinableLordJobJoinPriorities.SocialGathering + devotionFactor;
            return result;
        }

        private bool IsGatheringAboutToEnd()
        {
            if (timeoutTrigger.TicksLeft < 600)
            {
                return true;
            }
            return false;
        }

        private bool PositiveOutcome(ThoughtDef outcome)
        {
            return outcome != WorshipThoughtDefOf.DisturbingSermon && outcome != WorshipThoughtDefOf.UninspiredSermon;
        }

        public override string GetReport(Pawn pawn)
        {
            if (pawn != organizer)
            {
                return "LordReportListeningSermon".Translate(organizer.Named("ORGANIZER"));
            }
            return "LordReportGivingSermon".Translate();
        }

        public override LordToil CreateGatheringToil(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
        {
            return new LordToil_Sermon(spot, gatheringDef, organizer);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<BuildingAltar>(ref this.altar, "altar");
            Scribe_Values.Look<bool>(ref this.Ending, "Ending");
        }


        private static readonly Dictionary<ThoughtDef, float> SermonOutcomeThoughtChances = new Dictionary<ThoughtDef, float>
        {
            {
                WorshipThoughtDefOf.UninspiredSermon,
                0.2f
            },
            {
                WorshipThoughtDefOf.EncouragingSermon,
                0.6f
            },
            {
                WorshipThoughtDefOf.InspiringSermon,
                0.2f
            }
        };
    }

    public class LordToil_Sermon : LordToil_Gathering
    {
        public Pawn organizer;

        public LordToilData_Speech Data => (LordToilData_Speech)data;

        public LordToil_Sermon(IntVec3 spot, GatheringDef gatheringDef, Pawn organizer) : base(spot, gatheringDef)
        {
            this.organizer = organizer;
            data = new LordToilData_Speech();
        }

        public override void Init()
        {
            base.Init();
            Data.spectateRect = CellRect.CenteredOn(spot, 0);
            Rot4 rotation = spot.GetFirstThing<BuildingAltar>(organizer.MapHeld).Rotation;
            SpectateRectSide asSpectateSide = rotation.Opposite.AsSpectateSide;
            Data.spectateRectAllowedSides = (SpectateRectSide.All & ~asSpectateSide);
            Data.spectateRectPreferredSide = rotation.AsSpectateSide;
        }

        public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
        {
            if (p == organizer)
            {
                return Worship.DutyDefOf.HoldSermon.hook;
            }
            return RimWorld.DutyDefOf.Spectate.hook;
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Pawn pawn = lord.ownedPawns[i];
                if (pawn == organizer)
                {
                    BuildingAltar firstThing = spot.GetFirstThing<BuildingAltar>(base.Map);
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.HoldSermon, firstThing.InteractionCell, firstThing);
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
                else
                {
                    PawnDuty pawnDuty = new PawnDuty(DutyDefOf.AttendSermon);
                    pawnDuty.spectateRect = Data.spectateRect;
                    pawnDuty.spectateRectAllowedSides = Data.spectateRectAllowedSides;
                    pawnDuty.spectateRectPreferredSide = Data.spectateRectPreferredSide;
                    pawn.mindState.duty = pawnDuty;
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            }
        }
    }
}

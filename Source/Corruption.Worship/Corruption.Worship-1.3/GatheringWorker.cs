using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace Corruption.Worship
{
    public class GatheringWorker_Sermon : GatheringWorker
    {
        public override LordJob CreateLordJob(IntVec3 spot, Pawn organizer)
        {
            BuildingAltar altar = spot.GetFirstThing<BuildingAltar>(organizer.Map);

            return new LordJob_Sermon(altar, new TargetInfo(altar), organizer, null/*def*/, null, null, false);
        }

        public override bool TryExecute(Map map, Pawn organizer = null)
        {
            if (organizer == null)
            {
                organizer = FindOrganizer(map);
            }
            if (organizer == null)
            {
                return false;
            }
            if (!TryFindGatherSpot(organizer, out IntVec3 spot))
            {
                return false;
            }
            BuildingAltar altar = spot.GetFirstThing<BuildingAltar>(map);
            if (altar == null)
            {
                return false;
            }
            LordJob_Sermon lordJob = CreateLordJob(spot, organizer) as LordJob_Sermon;
            Lord lord = LordMaker.MakeNewLord(organizer.Faction, lordJob, organizer.Map, (!lordJob.OrganizerIsStartingPawn) ? null : new Pawn[1]
            {
            organizer
            });

            lord.AddBuilding(altar);
            if (altar.CurrentActiveSermon?.Assistant != null)
            {
                lord.AddPawn(altar.CurrentActiveSermon.Assistant);
            }

            //foreach (var allyLord in map.lordManager.lords.Where(x => x.LordJob is LordJob_VisitColony))
            //{
            //    var defendToil = allyLord.Graph.lordToils.FirstOrDefault(x => x is LordToil_DefendPoint);
            //    if (defendToil != null)
            //    {

            //        //allyLord.Graph.AttachSubgraph(lord.Graph);

            //        //Transition transitionIn = new Transition(defendToil, lord.Graph.StartingToil);
            //        //transitionIn.AddTrigger(new Trigger_Memo("JoinSermon"));

            //        //transitionIn.AddPreAction(new TransitionAction_Message("VisitorsJoinedSermon"));

            //        //Transition transitionOut = new Transition(lord.Graph.lordToils.FirstOrDefault(x => x is LordToil_End), defendToil);
            //        //transitionOut.AddTrigger(new Trigger_Custom(x => lordJob.Ending));
            //        //transitionOut.AddPreAction(new TransitionAction_Message("VisitorsReturnToDuty"));

            //        //allyLord.Graph.AddTransition(transitionIn, true);
            //        //allyLord.Graph.AddTransition(transitionOut, true);
            //        //allyLord.Graph.ErrorCheck();
            //        //allyLord.ReceiveMemo("JoinSermon");
            //    }
            //}


            SendLetter(spot, organizer);
            return true;
        }

        public override bool CanExecute(Map map, Pawn organizer = null)
        {
            if (organizer == null)
            {
                return false;
            }
            if (!TryFindGatherSpot(organizer, out IntVec3 _))
            {
                return false;
            }
            return true;
        }

        public override bool TryFindGatherSpot(Pawn organizer, out IntVec3 spot)
        {
            BuildingAltar altar = organizer.Map.listerBuildings.allBuildingsColonist.FirstOrDefault(x => x is BuildingAltar innerAltar && innerAltar.CurrentActiveSermon?.Preacher == organizer) as BuildingAltar;
            if (altar != null)
            {
                spot = altar.Position;
                return true;
            }
            spot = IntVec3.Invalid;
            return false;
        }

        public override void SendLetter(IntVec3 spot, Pawn organizer)
        {
            Find.LetterStack.ReceiveLetter(def.letterTitle, def.letterText.Formatted(organizer.Named("ORGANIZER")) + "\n\n" + OutcomeBreakdownForPawn(organizer), LetterDefOf.PositiveEvent, new TargetInfo(spot, organizer.Map));
        }

        public static string OutcomeBreakdownForPawn(Pawn organizer)
        {
            return "AbilitySpeechStatInfo".Translate(organizer.Named("ORGANIZER"), StatDefOf.SocialImpact.label) + ": " + organizer.GetStatValue(StatDefOf.SocialImpact).ToStringPercent() + "\n\n" + "AbilitySpeechPossibleOutcomes".Translate() + ":\n" + (from o in LordJob_Sermon.OutcomeChancesForPawn(organizer).Reverse() select o.Item1.stages[0].LabelCap + " " + o.Item2.ToStringPercent()).ToLineList("  - ");
        }
    }
}

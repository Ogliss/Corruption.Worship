using Corruption.Core.Soul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class IncidentWorker_GrowingFlock : IncidentWorker_PawnsArrive
    {

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            List<Faction> potentialFactions = new List<Faction>()
            {
                Find.FactionManager.FirstFactionOfDef(Corruption.Core.FactionsDefOf.IoM_NPC),
                Find.FactionManager.RandomNonHostileFaction(false, false, true),
                Find.FactionManager.RandomNonHostileFaction(false, false, true)
            };
            parms.faction = potentialFactions.RandomElement();
            parms.points = Math.Min(parms.points, 500);
            
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Settlement, parms, false);
            List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, true).ToList<Pawn>();
            Map map = (Map)parms.target;
            IntVec3 loc;
            if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Neutral, out loc))
            {
                return false;
            }

            foreach (Pawn pawn in list)
            {
                CompSoul soul = pawn.Soul();
                soul.ChosenPantheon = GlobalWorshipTracker.Current.PlayerPantheon;
                pawn.SetFaction(Faction.OfPlayer);
                pawn.workSettings.EnableAndInitialize();
                GenSpawn.Spawn(pawn, loc, map);
            }
            string text = "GrowingFlockMessage".Translate(new NamedArgument(list.Count, "COUNT"));
            string label = "LetterLabelGrowingFlock".Translate();
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, list[0], null);
            return true;
        
        }
    }
}

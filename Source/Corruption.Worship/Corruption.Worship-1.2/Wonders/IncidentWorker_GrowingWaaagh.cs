using Corruption.Core.Soul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship.Wonders
{
    class IncidentWorker_GrowingWaaagh : IncidentWorker
    {
        public override bool TryExecuteWorker(IncidentParms parms)
        {
            if (ModLister.GetActiveModWithIdentifier("Ogliss.AdMech.Xenobiologis.Orkz") == null)
            {
                Messages.Message("Zog. There ain't the required Ork mod installed!", MessageTypeDefOf.NegativeEvent, false);
                return false;
            }

            var factionDefs = DefDatabase<FactionDef>.AllDefsListForReading;
            var orkWarband = factionDefs.FirstOrDefault(x => x.defName.Equals("OG_Ork_Tek_Faction"));
            var tribalOrks = factionDefs.FirstOrDefault(x => x.defName.Equals("OG_Ork_Feral_Faction"));
;
            List<Faction> potentialFactions = new List<Faction>();

            if (orkWarband != null)
            {
                potentialFactions.Add(Find.FactionManager.FirstFactionOfDef(orkWarband));
            }
            if (tribalOrks != null)
            {
                potentialFactions.Add(Find.FactionManager.FirstFactionOfDef(tribalOrks));
            }

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
            return true;
        }
    }
}

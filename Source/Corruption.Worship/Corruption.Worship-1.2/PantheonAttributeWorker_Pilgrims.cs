using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship
{
    public class PantheonAttributeTickWorker_Pilgrims : PantheonAttributeTickWorker
    {
        private static SimpleCurve WealthEvaluation = new SimpleCurve()
        {
            {100,0 },
            {1000,0.1f },
            {5000,0.2f },
            {10000,0.3f },
            {10000,0.3f },
            {50000,0.5f }         
        };

        public override void TickDay()
        {
            var faction = Find.FactionManager.FirstFactionOfDef(Corruption.Core.FactionsDefOf.IoM_NPC);
            Map map = Find.RandomPlayerHomeMap;
            int curTick = Find.TickManager.TicksGame;

            float totalSpritualWealth = 0f;
            var altars = map.listerBuildings.allBuildingsColonist.Where(x => x.TryGetComp<CompShrine>() != null);
            foreach (var altar in altars)
            {
                Room room = altar.GetRoom();
                if (room != null)
                {
                    totalSpritualWealth += room.GetStat(RoomStatDefOf.Wealth);
                }
            }

            if (totalSpritualWealth == 0f)
            {
                return;
            }

            if (Rand.Value < WealthEvaluation.Evaluate(totalSpritualWealth) && faction.lastTraderRequestTick + GenDate.TicksPerDay * 7 > curTick)
            {
                IncidentParms parms = new IncidentParms
                {
                    target = map,
                    faction = faction,
                    forced = true
                };
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.VisitorGroup, curTick + 120000, parms, 24000);
                faction.lastTraderRequestTick = curTick;
            }
        }
    }
}

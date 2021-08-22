using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace Corruption.Worship.Quests
{
    public class QuestNode_RaidFixed : QuestNode_Raid
    {
        public override void RunInt()
        {
            base.RunInt();
            Slate slate = QuestGen.slate;
            Map target = QuestGen.slate.Get<Map>("map");
            List<Pawn> pawns = slate.Get<List<Pawn>>("raiders");
            var part = QuestGen.quest.PartsListForReading.FirstOrDefault(x => x is QuestPart_Incident) as QuestPart_Incident;
            if (part != null)
            {
                var parms = typeof(QuestPart_Incident).GetField("incidentParms", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(part) as IncidentParms;

                parms.target = target;

                parms.pawnGroups = new Dictionary<Pawn, int>();

                foreach (var pawn in pawns)
                {
                    parms.pawnGroups.Add(pawn, 0);
                }
                part.SetIncidentParmsAndRemoveTarget(parms);
            }
        }
    }
}

using Corruption.Core.Gods;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_PoliticalUpheaval : WonderWorker
    {
        public override bool TryExecuteWonderInt(GodDef god, int worshipPoints)
        {
            FieldInfo fieldInfo = typeof(Faction).GetField("relations", BindingFlags.NonPublic| BindingFlags.Instance);
            foreach (var faction in Find.FactionManager.AllFactionsVisible)
            {
                List<FactionRelation> relations = (List<FactionRelation>)fieldInfo.GetValue(faction);
                foreach (var relation in relations.Where(x => !x.other.def.hidden && (faction != x.other)))
                {
                    bool sent;
                    relation.goodwill = Rand.Range(-100, 50);
                    relation.CheckKindThresholds(faction, true, "TzeentchRevolutionDescription".Translate(), GlobalTargetInfo.Invalid, out sent);                    
                }
            }

            return true;

        }
    }
}

using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class IncidentWorker_NurgleBreath : IncidentWorker_DiseaseHuman
    {

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            this.ShuffleDiseaseDef();
            if( base.TryExecuteWorker(parms))
            {
                GlobalWorshipTracker.Current.TryAddFavor(GodDefOf.Nurgle, 1000);
                return true;
            }
            return false;
        }

        private void ShuffleDiseaseDef()
        {
            List<IncidentDef> diseases = DefDatabase<IncidentDef>.AllDefsListForReading.FindAll(x => x.category == IncidentCategoryDefOf.DiseaseHuman);
            this.def.diseaseIncident = diseases.RandomElement().diseaseIncident;
        }
    }
}

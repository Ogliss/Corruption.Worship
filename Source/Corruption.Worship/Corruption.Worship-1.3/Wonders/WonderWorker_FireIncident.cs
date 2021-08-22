using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_FireIncident : WonderWorker_Targetable
    {
        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters()
            {
                canTargetLocations = true,
                canTargetPawns = true,
                validator = ((TargetInfo x) => true)
            };
        }

        protected override bool TryDoEffectOnTarget(GodDef god, int worshipPoints)
        {
            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = this.target.Map;
            incidentParms.points = this.Def.ResolveWonderPoints(target.Map, worshipPoints);
            if (!this.Def.incident.Worker.CanFireNow(incidentParms))
            {
                return GlobalWorshipTracker.Current.TryAddFavor(god, worshipPoints);

            }
            if (this.Def.incident.Worker.TryExecute(incidentParms) == false)
            {
                return GlobalWorshipTracker.Current.TryAddFavor(god, worshipPoints);
            }
            return false;

        }
    }
}

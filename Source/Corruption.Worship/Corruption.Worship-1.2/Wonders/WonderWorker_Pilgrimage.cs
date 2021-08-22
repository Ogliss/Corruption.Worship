using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{

    public class WonderWorker_Pilgrimage : WonderWorker_Targetable
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
            Dialog_FormAndSendPilgrims PilgrimageDialog = new Dialog_FormAndSendPilgrims(this.target.Map, god);
            Find.WindowStack.Add(PilgrimageDialog);
            return true;
        }
    }
}

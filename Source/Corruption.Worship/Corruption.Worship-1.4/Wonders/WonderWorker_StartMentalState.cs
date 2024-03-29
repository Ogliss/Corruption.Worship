﻿using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_StartMentalState : WonderWorker_Targetable
    {
        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters()
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                canTargetItems = false,
                canTargetLocations = false
            };
        }

        protected override bool TryDoEffectOnTarget(GodDef god, int worshipPoints)
        {
            Pawn pawn = (Pawn)this.target.Thing;
            return pawn.mindState.mentalStateHandler.TryStartMentalState(this.Def.mentalStateToStart, null, true);
        }
    }
}

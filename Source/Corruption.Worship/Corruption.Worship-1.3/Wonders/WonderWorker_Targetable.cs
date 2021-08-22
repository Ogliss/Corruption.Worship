using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_Targetable : WonderWorker
    {
        protected TargetInfo target;

        private bool cancelled = false;

        protected virtual TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,                
                validator = ((TargetInfo x) => BaseTargetValidator(x.Thing))
            };
        }

        protected void StartTargeting(GodDef god, int worshipPoints)
        {
            cancelled = false;
            if (this.target == null || this.target == TargetInfo.Invalid)
            {
                return;
            }
            if (!this.GetTargetingParameters().CanTarget(this.target))
            {
                GlobalWorshipTracker.Current.TryAddFavor(god, worshipPoints);
                return;
            }
            if (this.target.HasThing)
            {
                MoteMaker.MakeAttachedOverlay(this.target.Thing, god.effectMote, Vector3.zero);
            }
            else
            {
                MoteMaker.MakeStaticMote(this.target.Cell, this.target.Map, god.effectMote);
            }
            if (TryDoEffectOnTarget(god, worshipPoints))
            {
                GlobalWorshipTracker.Current.TryAddFavor(god, this.Def.yieldFavour);
            }
        }

        protected virtual bool TryDoEffectOnTarget(GodDef god, int worshipPoints)
        {
            return true;
        }

        public bool BaseTargetValidator(Thing t)
        {
            return true;
        }

        public override bool TryExecuteWonderInt(GodDef god, int worshipPoints)
        {
            cancelled = true;
            Find.Targeter.BeginTargeting(this.GetTargetingParameters(), delegate (LocalTargetInfo t)
            {
                this.target = t.ToTargetInfo(Find.CurrentMap);
                this.StartTargeting(god, worshipPoints);
            },null, delegate { this.CheckCancelled(god, worshipPoints); }, null);
            return true;
        }

        private void CheckCancelled(GodDef god, int worshipPoints)
        {
            if (this.cancelled)
            {
                GlobalWorshipTracker.Current.TryAddFavor(god, worshipPoints);
            }
        }
    }
}

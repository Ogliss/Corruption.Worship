using Corruption.Core.Gods;
using Corruption.Core.Soul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_AddSpecialTrait : WonderWorker_Targetable
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
            Pawn pawn = this.target.Thing as Pawn;
            if (pawn != null)
            {
                foreach (var incompatible in this.Def.traitToGive.conflictingTraits)
                {
                    Trait incTrait = pawn.story.traits.GetTrait(incompatible);
                    if (incTrait != null)
                    {
                        pawn.story.traits.allTraits.Remove(incTrait);
                    }
                }

                Trait trait = new Trait(this.Def.traitToGive, this.Def.traitToGive.degreeDatas.FirstOrDefault().degree, true);

                var traitData = trait.CurrentData as Corruption.Core.Soul.SoulTraitDegreeOptions;

                if (traitData != null)
                {
                    foreach (var ability in traitData.abilityUnlocks)
                    {
                        pawn.abilities.GainAbility(ability);
                        pawn.Soul()?.LearnedAbilities.Add(ability);
                    }
                }

                if(this.Def.taleToCreate != null && this.Def.traitToGive is SoulTraitDef sDef)
                {

                    var tale = TaleFactory.MakeRawTale(this.Def.taleToCreate, pawn);
                    var traitTale = TaleRecorder.RecordTale(WorshipTaleDefOfs.TraitProgressionTale, sDef, traitData.degree, tale);
                    Log.Message(traitTale.ShortSummary);
                }

                if (traitData.associatedTrait != null)
                {
                    var existingTrait = pawn.story.traits.GetTrait(traitData.associatedTrait);
                    if (existingTrait == null)

                        pawn.story.traits.GainTrait(new Trait(traitData.associatedTrait, traitData.associatedTraitDegree, true));
                }


                pawn.story.traits.GainTrait(trait);
                return true;
            }
            return false;
        }
    }
}

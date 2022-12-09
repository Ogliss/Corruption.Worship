using Corruption.Core.Gods;
using Corruption.Core.Soul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Corruption.Worship
{

    public class JobDriver_HoldSermon : JobDriver
    {
        private TargetIndex AltarIndex = TargetIndex.A;
        private TargetIndex AltarInteractionCell = TargetIndex.B;
        private static readonly IntRange MoteInterval = new IntRange(300, 500);

        private Effecter effecter;
        private CompShrine CompShrine => ((ThingWithComps)this.TargetA).GetComp<CompShrine>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<TargetIndex>(ref this.AltarIndex, "AltarIndex", TargetIndex.A);
            Scribe_Values.Look<TargetIndex>(ref this.AltarInteractionCell, "AltarInteractionCell", TargetIndex.B);
            Scribe_References.Look<Pawn>(ref this.pawn, "pawn", false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            BuildingAltar altar = this.TargetA.Thing as BuildingAltar;
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_General.Do(delegate
            {
                job.SetTarget(TargetIndex.B, altar.InteractionCell + altar.Rotation.FacingCell);
            });
            Toil toil = new Toil();
            toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            toil.FailOn(() => altar == null);
            toil.FailOn(() => altar.CurrentActiveSermon == null);
            toil.FailOn(() => altar.CurrentActiveSermon?.Preacher != pawn);
            toil.PlaySustainerOrSound(Core.CoreSoundDefOf.PrayerSustainer);
            GodDef god = altar.CurrentActiveSermon.DedicatedTo;
            toil.tickAction = delegate
            {
                if (god != null)
                {
                    pawn.GainComfortFromCellIfPossible();
                    pawn.skills.Learn(SkillDefOf.Social, 0.3f);
                    if (pawn.IsHashIntervalTick(MoteInterval.RandomInRange))
                    {
                        MoteMaker.MakeSpeechBubble(pawn, god.PrayerMote);
                    }

                    if (this.CompShrine != null)
                    {
                        this.pawn.Soul()?.GainCorruption(god.favourCorruptionFactor * this.CompShrine.Props.worshipFactor, god);
                    }
                    rotateToFace = TargetIndex.B;
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
        }


        protected void ThrowPreacherMote(Pawn pawn, GodDef god)
        {
            if (pawn.IsHashIntervalTick(MoteInterval.RandomInRange))
            {
                MoteMaker.MakeSpeechBubble(pawn, god.PrayerMote);
            }

            MoteBubble moteBubble2 = (MoteBubble)ThingMaker.MakeThing(ThingDefOf.Mote_Speech, null);
            moteBubble2.SetupMoteBubble(god.PrayerMote, pawn);
            moteBubble2.Attach(pawn);
            GenSpawn.Spawn(moteBubble2, pawn.Position, pawn.Map);
        }
    }
}

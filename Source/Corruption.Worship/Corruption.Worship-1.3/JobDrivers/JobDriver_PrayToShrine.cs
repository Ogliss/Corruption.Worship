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
    public class JobDriver_PrayToShrine : JobDriver_Prayer
    {
        protected Thing Shrine
        {
            get
            {

                if (this.TargetB != LocalTargetInfo.Invalid && this.TargetB.HasThing)
                {
                    return this.TargetB.Thing;
                }
                return null;
            }
        }

        protected CompShrine CompShrine
        {
            get
            {
                if (Shrine != null)
                {
                    return this.Shrine.TryGetComp<CompShrine>();
                }
                return null;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {

            return base.TryMakePreToilReservations(errorOnFailed) && (this.TargetB.HasThing == false || pawn.CanReserveAndReach(this.TargetB, PathEndMode.ClosestTouch, Danger.None, 1));
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            CompSoul soul = this.GetActor().Soul();
            var toils = base.MakeNewToils().ToList();

            Toil lastToil = toils.Last();

            if (this.CompShrine != null)
            {
                lastToil.initAction = delegate
                {
                    float angle = (CompShrine.parent.Position - pawn.Position).ToVector3().AngleFlat();
                    faceDir = Pawn_RotationTracker.RotFromAngleBiased(angle);
                    soul?.PrayerTracker.StartRandomPrayer(this.job, true);
                };
            }

            lastToil.AddFinishAction(delegate { AddPrayerFinish(soul, lastToil); });


            return toils;
        }

        protected virtual void AddPrayerFinish(CompSoul soul, Toil lastToil)
        {
            if (soul != null)
            {
                var shrine = this.CompShrine;
                float num = 100f + lastToil.defaultDuration / 3600f;
                if (shrine != null)
                {
                    num *= shrine.Props.worshipFactor;
                    var effigy = shrine.InstalledEffigy?.TryGetComp<CompEffigy>();

                    if (effigy != null)
                    {
                        foreach (var pantheonMember in effigy.Props.dedicatedPantheon.GodsListForReading)
                        {
                            soul.GainCorruption(num * pantheonMember.favourCorruptionFactor / effigy.Props.dedicatedPantheon.GodsListForReading.Count, pantheonMember);
                        }

                        if (effigy.Props.dedicatedTo != null)
                        {
                            soul.GainCorruption(num * effigy.Props.dedicatedTo.favourCorruptionFactor, effigy.Props.dedicatedTo);
                        }
                    }
                }
                else
                {
                    soul.GainCorruption(num * targetedGod.favourCorruptionFactor, targetedGod);
                }
            }
        }
    }
}

using Corruption.Core.Gods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption.Worship
{
    public class CompEffigy : ThingComp
    {
        public CompProperties_Effigy Props => this.props as CompProperties_Effigy;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            IEnumerator<FloatMenuOption> enumerator = base.CompFloatMenuOptions(selPawn).GetEnumerator();
            while (enumerator.MoveNext())
            {
                FloatMenuOption current = enumerator.Current;
                yield return current;
            }
            if (!selPawn.CanReserve(this.parent, 1))
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption("CannotUseReserved".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            if (!selPawn.CanReach(this.parent, PathEndMode.InteractionCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                FloatMenuOption floatMenuOption2 = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption2;
            }
            string label = "JobTakeEffigy".Translate(this.parent.Label);
            Action action = delegate
            {
                Job job = new Job(RimWorld.JobDefOf.TakeInventory, this.parent);
                job.count = 1;
                selPawn.jobs.TryTakeOrderedJob(job);
            };
            yield return new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null);
        }
    }


    public class CompProperties_Effigy : CompProperties
    {
        public GodDef dedicatedTo;

        public PantheonDef dedicatedPantheon;

        public float worshipFactor = 1f;

        public CompProperties_Effigy()
        {
            this.compClass = typeof(CompEffigy);
        }
    }
}

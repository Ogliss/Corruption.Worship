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
    public class ItemEffigy : ThingWithComps
    {
        public CompEffigy EffigyComp => this.GetComp<CompEffigy>();


        public static Job InstallEffigyJob(Pawn p, Thing t, Thing container)
        {
            ThingOwner thingOwner = container.TryGetInnerInteractableThingOwner();
            if (thingOwner == null)
            {
                Log.Error(container.ToStringSafe() + " gave null ThingOwner.");
                return null;
            }
            Job job = JobMaker.MakeJob(Worship.WorshipJobDefOf.Corruption_InstallEffigy, t, container);
            job.count = 1;
            job.haulMode = HaulMode.ToContainer;
            return job;
        }

    }
}

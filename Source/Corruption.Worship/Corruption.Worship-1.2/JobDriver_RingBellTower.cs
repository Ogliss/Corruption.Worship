using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.Worship
{
    class JobDriver_RingBellTower : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

		public override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			CompBellTower compBellTower = null;
			ThingWithComps thingWithComps = (ThingWithComps)this.TargetA.Thing;
			for (int i = 0; i < thingWithComps.AllComps.Count; i++)
			{
				compBellTower = thingWithComps.AllComps[i] as CompBellTower;
			}
			this.FailOn(() => compBellTower == null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(15).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			Toil finalize = new Toil();
			finalize.AddPreTickAction(delegate
			{
				Lord lord = pawn.GetLord();
				LordJob_Sermon lordJob = lord?.LordJob as LordJob_Sermon;
				if (lordJob != null)
				{
					compBellTower.Notify_SermonStarting(lordJob.altar);
				}
			});
			Pawn actor = finalize.actor;
			finalize.tickAction = delegate
			{
				if (compBellTower != null)
				{
					compBellTower.StartRinging();
				}
			};
			finalize.defaultCompleteMode = ToilCompleteMode.Delay;
			finalize.defaultDuration = 600;
			yield return finalize;
		}
    }
}

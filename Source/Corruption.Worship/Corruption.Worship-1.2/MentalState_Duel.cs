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
    public class MentalState_Duel : MentalState_SocialFighting
    {
		public override void MentalStateTick()
		{
			base.MentalStateTick();
			if (otherPawn != null && otherPawn.Dead)
			{
				RecoverFromState();
			}
			if (pawn.IsHashIntervalTick(120) && !IsotherPawnStillValidAndReachable())
			{
				Messages.Message("MessageDuelBreak".Translate(pawn.NameShortColored, otherPawn.Label, pawn.Named("PAWN"), otherPawn.Named("otherPawn")).Resolve().AdjustedFor(pawn), pawn, MessageTypeDefOf.NegativeEvent);
				base.MentalStateTick();
			}
		}

		private bool IsotherPawnStillValidAndReachable()
		{
			if (otherPawn != null && otherPawn.SpawnedParentOrMe != null && (!(otherPawn.SpawnedParentOrMe is Pawn) || otherPawn.SpawnedParentOrMe == otherPawn))
			{
				return pawn.CanReach(otherPawn.SpawnedParentOrMe, PathEndMode.Touch, Danger.Deadly, canBash: true);
			}
			return false;
		}
	}
}

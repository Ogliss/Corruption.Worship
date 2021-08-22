using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship
{
    public static class CorpseGenerator
    {
        public static Corpse GenerateDessicatedCorpse(PawnKindDef kindDef, IntRange ageRange, Faction faction = null)
        {
            var pawn = PawnGenerator.GeneratePawn(kindDef, faction ?? Find.FactionManager.FirstFactionOfDef(Core.FactionsDefOf.IoM_NPC));
            pawn.Kill(null);
            pawn.Corpse.Age = ageRange.RandomInRange;
            var rottable = pawn.Corpse.TryGetComp<CompRottable>();
            rottable.RotProgress = rottable.PropsRot.TicksToDessicated + 60000;
            return pawn.Corpse;
        }
    }
}

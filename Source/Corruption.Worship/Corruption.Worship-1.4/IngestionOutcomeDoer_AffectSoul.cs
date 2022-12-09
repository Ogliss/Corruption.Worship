using Corruption.Core.Gods;
using Corruption.Core.Soul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship
{
    public class IngestionOutcomeDoer_AffectSoul : IngestionOutcomeDoer
    {
        public GodDef pleasesGod;

        public float gain = 10f;

        public override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            CompSoul soul = pawn.Soul();
            if (soul != null)
            {
                soul.GainCorruption(gain, pleasesGod);
            }
        }
    }
}

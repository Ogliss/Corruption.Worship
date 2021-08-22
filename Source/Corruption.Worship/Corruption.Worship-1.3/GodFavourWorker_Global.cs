using Corruption.Core.Gods;
using Corruption.Core.Soul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship
{
    public class GodFavourWorker_Global : GodFavourWorker
    {
        public override void PostGainFavour(CompSoul soul, float change, GodDef god = null)
        {
            base.PostGainFavour(soul, change);
            GlobalWorshipTracker.Current.TryAddFavor(god, change / 4f, soul.Pawn);
        }
    }
}

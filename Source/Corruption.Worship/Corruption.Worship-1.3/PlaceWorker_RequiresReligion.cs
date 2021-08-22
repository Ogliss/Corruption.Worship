using Corruption.Core.Gods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship
{
    public abstract class PlaceWorker_RequiresReligion : PlaceWorker
    {
        protected abstract PantheonDef RequiredPantheon { get; }

        public override bool IsBuildDesignatorVisible(BuildableDef def)
        {
            return GlobalWorshipTracker.Current.PlayerPantheon == this.RequiredPantheon;
        }
    }

    public class PlaceWorker_RequiresImperialCult : PlaceWorker_RequiresReligion
    {
        protected override PantheonDef RequiredPantheon => PantheonDefOf.ImperialCult;
    }

    public class PlaceWorker_RequiresChaos : PlaceWorker_RequiresReligion
    {
        protected override PantheonDef RequiredPantheon => PantheonDefOf.Chaos;
    }

}

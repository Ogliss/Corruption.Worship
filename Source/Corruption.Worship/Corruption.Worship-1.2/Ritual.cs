using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship
{
    public class Ritual : IExposable
    {

        public Ritual()
        {

        }

        public Ritual(RitualDef def)
        {
            this.Def = def;
        }

        public RitualDef Def;

        public virtual void ExposeData()
        {
            Scribe_Defs.Look<RitualDef>(ref this.Def, "def");
        }
    }
}

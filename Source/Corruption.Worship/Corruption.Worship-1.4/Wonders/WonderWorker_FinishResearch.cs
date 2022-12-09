using Corruption.Core.Gods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship.Wonders
{
    internal class WonderWorker_FinishResearch : WonderWorker
    {
        public override bool TryExecuteWonderInt(GodDef god, int worshipPoints)
        {
            var manager = Find.ResearchManager;
            if (manager != null)
            {
                if (manager.currentProj != null)
                {
                    manager.ResearchPerformed(worshipPoints * 100, null);
                    return true;
                }
            }
            return false;
        }
    }
}

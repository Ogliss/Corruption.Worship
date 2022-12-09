using Corruption.Core.Gods;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_Quest : WonderWorker
    {
        protected virtual QuestScriptDef QuestScriptDef
        {
            get
            {
                var selected = this.Def.questScriptDefs.RandomElement();
                return selected;
            }
        }

        public override bool TryExecuteWonderInt(GodDef god, int worshipPoints)
        {
            var slate = new Slate();
            slate.Set("god", god);
            slate.Set("godName", god.LabelCap);
            QuestUtility.SendLetterQuestAvailable(QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDef, slate));
            return true;
        }
    }
}

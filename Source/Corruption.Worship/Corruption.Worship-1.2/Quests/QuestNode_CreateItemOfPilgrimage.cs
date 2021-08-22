using Corruption.Core.Gods;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Verse;

namespace Corruption.Worship.Quests
{
    public class QuestNode_CreatePlaceOfPilgrimage : QuestNode
    {
        public SlateRef<IEnumerable<ThingDef>> potentialItems;
        public override void RunInt()
        {
            Slate slate = QuestGen.slate;
            slate.Set("objectOfPilgrimageDef", CreateItem(slate));
        }

        public override bool TestRunInt(Slate slate)
        {
            return true;
        }

        private ThingDef CreateItem(Slate slate)
        {
            IEnumerable<ThingDef> value = potentialItems.GetValue(slate);
            if (value != null)
            {
                return value.RandomElement();
            }
            return null;
        }
    }
}


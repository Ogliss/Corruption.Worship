using Corruption.Core.Gods;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship.Quests
{
    public class QuestNode_AwardFavour : QuestNode
    {
        public SlateRef<float> favourToGain;

        public override void RunInt()
        {
            var slate = QuestGen.slate;
            var quest = QuestGen.quest;
            var favour = favourToGain.GetValue(slate);
            var god = slate.Get<GodDef>("god", GodDefOf.Emperor);

            Slate.VarRestoreInfo restoreInfo = slate.GetRestoreInfo("inSignal");
            var inSignal = slate.Get<string>("inSignal");
            slate.Set("inSignal", QuestGenUtility.HardcodedSignalWithQuestID(inSignal));

            try
            {

                var part = new QuestPart_AwardFavour();
                part.inSignalChoiceUsed = slate.Get<string>("inSignal");
                part.God = god;
                part.Favour = favour;
                quest.AddPart(part);
            }
            finally
            {
                slate.Restore(restoreInfo);
            }
        }

        public override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }

    public class QuestPart_AwardFavour : QuestPart
    {
        public string inSignalChoiceUsed;
        public float Favour;
        public GodDef God;
        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (!(signal.tag == inSignalChoiceUsed))
            {
                return;
            }
            GlobalWorshipTracker.Current.TryAddFavor(God, Favour);

        }
    }
}

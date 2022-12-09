using Corruption.Core;
using Corruption.Core.Soul;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;

namespace Corruption.Worship.Quests
{
    public class SitePartWorker_Pilgrimage : SitePartWorker
	{
		public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
		{
			base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
			ThingDef objectOfPilgrimage = slate.Get<ThingDef>("objectOfPilgrimageDef");

			var item = this.CreateItem(objectOfPilgrimage);

			var list = new List<Thing>();
			list.Add(item);
			var qPart = new QuestPart_InjectComponent();
			qPart.SetComponent(item, typeof(Comp_QuestItem_GivePrayer));			
			QuestGen.quest.AddPart(qPart);

			part.things = new ThingOwner<Thing>(part, oneStackOnly: false);
			part.things.TryAddRangeOrTransfer(list, canMergeWithExistingStacks: false);
			slate.Set("generatedItemStashThings", list);
		}

		private ThingWithComps CreateItem(ThingDef objectOfPilgrimage)
		{
			var item = objectOfPilgrimage;
			var thingWithComps = ThingMaker.MakeThing(item, GenStuff.RandomStuffFor(item)) as ThingWithComps;
			string tag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("objectOfPilgrimage");
			var qualityComp = thingWithComps.TryGetComp<CompQuality>();
			qualityComp?.SetQuality(QualityUtility.GenerateQualityTraderItem(), ArtGenerationContext.Outsider);
			QuestUtility.AddQuestTag(ref thingWithComps.questTags, tag);
			return thingWithComps;
		}

		public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
		{
			string text = base.GetPostProcessedThreatLabel(site, sitePart);
			if (site.HasWorldObjectTimeout)
			{
				text += " (" + "DurationLeft".Translate(site.WorldObjectTimeoutTicksLeft.ToStringTicksToPeriod()) + ")";
			}
			return text;
		}
	}
}

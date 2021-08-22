using Corruption.Core.Abilities;
using Corruption.Core.Soul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Corruption.Worship
{
    public class Window_ChosenAbilities : Window_TraitAbilities
    {
        private Dictionary<TraitDegreeData, Tale_TraitProgression> Tales = new Dictionary<TraitDegreeData, Tale_TraitProgression>();

        public Window_ChosenAbilities(CompSoul soul, Trait soulTrait) : base(soul, soulTrait)
        {
            var tales = Find.TaleManager.AllTalesListForReading.FindAll(x => x is Tale_TraitProgression).Cast<Tale_TraitProgression>().ToList();
            var passedDegrees = this.TraitDef.degreeDatas.Where(x => x.degree <= soulTrait.Degree);
            foreach (var tale in tales)
            {
                var options = passedDegrees.FirstOrDefault(x => x.degree == tale.ObtainedAtDegree);
                if (options != null)
                {
                    this.Tales.Add(options, tale);
                }
            }
        }

        protected override TaggedString LearnActionText => "AcceptGiftedAbility".Translate();

        protected override Rect GetNodeParentRect(ref Rect inRect, float curY)
        {
            return new Rect(0f, curY, inRect.width - 250f, 128f);
        }

        protected override Rect DrawNodesRect(Rect containerRect)
        {
            float maxWidth = containerRect.width;

            Rect actualNodeRect = new Rect(0f, 64f, containerRect.width, containerRect.height);

            Rect progressRect = new Rect(16f, 16f, maxWidth - 32f, 32f);
            GUI.DrawTexture(progressRect, BackgroundTile);

            Widgets.FillableBar(progressRect.ContractedBy(4f), (float)(this.Trait.Degree - minDegree) / (float)(this.maxDegree - minDegree), ProgressTex);

            foreach (var degree in this.TraitDef.degreeDatas.Where(x => x is SoulTraitDegreeOptions).Cast<SoulTraitDegreeOptions>().OrderBy(x => x.degree))
            {
                DrawProgressMarker(maxWidth, degree);
            }
            GUI.BeginGroup(actualNodeRect);

            foreach (var degree in this.TraitDef.degreeDatas.Where(x => x is SoulTraitDegreeOptions).Cast<SoulTraitDegreeOptions>().OrderBy(x => x.degree))
            {
                DrawProgressMarker(maxWidth, degree);
                DrawNode(maxWidth, degree, 0f);
            }
            GUI.EndGroup();
            return containerRect;
        }

        private void DrawProgressMarker(float maxWidth, SoulTraitDegreeOptions degree)
        {
            var nodeRect = GetNodeRect(maxWidth, degree, 0f).ContractedBy(8f);
            GUI.DrawTexture(nodeRect, NodeBG);
            if (this.Tales.ContainsKey(degree))
            {
                var tale = this.Tales[degree];
                Rect iconRect = nodeRect.ContractedBy(4f);
                GUI.DrawTexture(iconRect, this.TraitDef.associatedGod.SmallTexture);
                TooltipHandler.TipRegion(nodeRect, new TipSignal(tale.Text));
            }
            
        }
    }
}

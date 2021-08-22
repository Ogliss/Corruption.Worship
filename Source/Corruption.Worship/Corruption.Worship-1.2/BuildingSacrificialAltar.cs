using RimWorld;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace Corruption.Worship
{
	[StaticConstructorOnStartup]
    public class BuildingSacrificialAltar : Building
    {
        public Ritual CurrentRitual;

		public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (this.Faction.IsPlayer)
            {
                var command = new Command_Action();
                command.defaultDesc = "StartRitualDesc".Translate();
                command.defaultLabel = "StartRitual".Translate();

                command.action = delegate
                {
                    Find.WindowStack.Add(new Dialog_StartRitual(this));
                };

                yield return command;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<Ritual>(ref this.CurrentRitual, "CurrentRitual");
        }
    }

	[StaticConstructorOnStartup]
    public class Dialog_StartRitual : Window
	{
        private static Vector2 infoScrollPosition;
        private static Vector2 RitualsScrollPos;
		private static readonly Texture2D HiglightTex = ContentFinder<Texture2D>.Get("UI/HeroArt/Storytellers/Highlight");
		private BuildingSacrificialAltar Altar;

        private RitualDef SelectedRitualDef;

        private List<RitualDef> AvailableRituals = new List<RitualDef>();
        private List<RitualCategoryDef> Categories = new List<RitualCategoryDef>();

        public Dialog_StartRitual(BuildingSacrificialAltar altar)
        {
            Altar = altar;
            this.AvailableRituals = DefDatabase<RitualDef>.AllDefsListForReading;
            this.Categories = DefDatabase<RitualCategoryDef>.AllDefsListForReading;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect mainRect = new Rect(0f, 0f, this.InitialSize.x, this.InitialSize.y - 32f).ContractedBy(5f);

            GUI.BeginGroup(mainRect);
            Rect rect2 = new Rect(0f, 0f, mainRect.width * 0.35f, mainRect.height).Rounded();
            DoRitualSelectionList(rect2);
            DrawRitualInfo(new Rect(rect2.xMax + 17f, 0f, mainRect.width - rect2.width - 17f, mainRect.height).Rounded(), SelectedRitualDef, ref infoScrollPosition);
            GUI.EndGroup();
            DoBottomButtons(inRect, null, "ScenarioEditor".Translate());
        }

        private void DoBottomButtons(Rect inRect, object p, TaggedString taggedString)
        {
            throw new NotImplementedException();
        }


        private float totalRitualListHeight;
        private void DoRitualSelectionList(Rect rect)
        {
            rect.xMax += 2f;
            Rect rect2 = new Rect(0f, 0f, rect.width - 16f - 2f, totalRitualListHeight + 250f);
            Widgets.BeginScrollView(rect, ref RitualsScrollPos, rect2);
            Rect rect3 = rect2.AtZero();
            rect3.height = 999999f;
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = rect2.width;
            listing_Standard.Begin(rect3);
            Text.Font = GameFont.Small;

            foreach (var category in this.Categories)
            {
                Text.Font = GameFont.Small;
                listing_Standard.Label(category.LabelCap);
                ListRitualsOnListing(listing_Standard, this.AvailableRituals.Where(x => x.Category == category));
                listing_Standard.Gap();
            }
            Widgets.EndScrollView();
        }

        private void ListRitualsOnListing(Listing_Standard listing, IEnumerable<RitualDef> ritualDefs)
        {
            bool flag = false;
            foreach (RitualDef def in ritualDefs)
            {
                    if (flag)
                    {
                        listing.Gap();
                    }
                    Rect rect = listing.GetRect(62f);
                    DoRitualListEntry(rect, def);
                    flag = true;
                
            }
            if (!flag)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                listing.Label("(" + "NoneLower".Translate() + ")");
                GUI.color = Color.white;
            }
        }

        private void DoRitualListEntry(Rect rect, RitualDef def)
        {
            bool flag = SelectedRitualDef == def;
            Widgets.DrawOptionBackground(rect, flag);
            MouseoverSounds.DoRegion(rect);
            Rect rect2 = rect.ContractedBy(4f);
            Text.Font = GameFont.Small;
            Rect rect3 = rect2;
            rect3.height = Text.CalcHeight(def.LabelCap, rect3.width);
            Widgets.Label(rect3, def.LabelCap);
            Text.Font = GameFont.Tiny;
            Rect rect4 = rect2;
            rect4.yMin = rect3.yMax;
            Widgets.Label(rect4, def.description);
            WidgetRow widgetRow = new WidgetRow(rect.xMax, rect.y, UIDirection.LeftThenDown);
            if (!flag && Widgets.ButtonInvisible(rect))
            {
                SelectedRitualDef = def;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
        }

        private static void DrawRitualInfo(Rect rect, RitualDef ritualDef, ref Vector2 infoScrollPosition)
        {
            Widgets.DrawMenuSection(rect);
            rect = rect.GetInnerRect();
            if (ritualDef != null)
            {
                string fullInformationText = ritualDef.FullDescription;
                float width = rect.width - 16f;
                float height = 30f + Text.CalcHeight(fullInformationText, width) + 100f;
                Rect viewRect = new Rect(0f, 0f, width, height);
                Widgets.BeginScrollView(rect, ref infoScrollPosition, viewRect);
                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(0f, 0f, viewRect.width, 30f), ritualDef.LabelCap);
                Text.Font = GameFont.Small;
                Widgets.Label(new Rect(0f, 30f, viewRect.width, viewRect.height - 30f), fullInformationText);
                Widgets.EndScrollView();
            }
        }
    }

}

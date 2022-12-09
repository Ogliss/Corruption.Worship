
using Corruption.Core;
using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Corruption.Worship
{
    [StaticConstructorOnStartup]
    public class TempleCardUtility
    {
        public static Vector2 TempleCardSize = new Vector2(570f, 400f);
        private static Vector2 SermonsScrollPos;

        public static void DrawTempleCard(Rect rect, BuildingAltar altar)
        {
            GUI.BeginGroup(rect);
            Rect rect2 = new Rect(rect.x, rect.y + 20f, 250f, 55f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect2, altar.RoomName);
            Text.Font = GameFont.Small;
            if (altar.Faction == Faction.OfPlayer)
            {
                Rect rect8 = new Rect(rect.width - 60f, 0f, 30f, 30f);
                TooltipHandler.TipRegion(rect8, () => "RenameTemple".Translate(), altar.thingIDNumber);
                if (Widgets.ButtonImage(rect8, TexUI_Core.Rename))
                {
                    Find.WindowStack.Add(new Dialog_RenameTemple(altar));
                }
            }

            Text.Font = GameFont.Medium;
            Rect overviewRect = new Rect(0f, rect2.yMax + 4f, rect.width, Text.LineHeight);
            Widgets.Label(overviewRect, "SermonsOverview".Translate());
            Text.Font = GameFont.Small;

            Rect outerRect = new Rect(0f, overviewRect.yMax + 8f, rect.width, rect.height - overviewRect.yMax - 8f);
            Rect viewRect = new Rect(0f, 0f, outerRect.width - 16f, altar.Templates.Count * 232f);


            Widgets.DrawMenuSection(outerRect);

            Widgets.BeginScrollView(outerRect, ref SermonsScrollPos, viewRect);

            float curY = 0f;
            foreach (var template in altar.Templates)
            {
                curY = DrawSermonTemplate(altar, curY, viewRect.width, template);
            }
            Widgets.EndScrollView();

            GUI.EndGroup();
        }

        private static float DrawSermonTemplate(BuildingAltar altar, float curY, float width, SermonTemplate sermon)
        {
            Rect nameRect = new Rect(4f, curY + 4f, width, Text.LineHeight);
            Widgets.Label(nameRect, "SermonTitle".Translate(sermon.Name));
            if (altar.Faction == Faction.OfPlayer)
            {
                Rect renameRect = new Rect(width - 34f, curY, 30f, 30f);
                TooltipHandler.TipRegion(renameRect, () => "RenameSermon".Translate(), altar.thingIDNumber);
                if (Widgets.ButtonImage(renameRect, TexUI_Core.Rename))
                {
                    Find.WindowStack.Add(new Dialog_RenameSermon(sermon));
                }
            }

            Rect preacherLabel = new Rect(4f, nameRect.yMax + 8f, 200f, Text.LineHeight + 2f);
            Widgets.Label(preacherLabel, "Preacher".Translate());


            Rect preacherButtonRect = new Rect(4f, preacherLabel.yMax + 4f, 200f, 35f);

            if (Widgets.ButtonText(preacherButtonRect, sermon.Preacher?.LabelCap ?? "None".Translate()))
            {
                OpenPreacherSelectMenu(altar, sermon);
            }

            Rect dedicationRect = new Rect(4f, preacherButtonRect.yMax + 8f, 200f, 35f);

            Widgets.Label(dedicationRect, "SermonDedication".Translate());
            Rect dedicationBtnRect = new Rect(dedicationRect);
            dedicationBtnRect.x = width - 208f;

            if (Widgets.ButtonText(dedicationBtnRect, sermon.DedicatedTo.LabelCap))
            {
                OpenDedicationSelectMenu(altar, sermon);
            }

            GUI.color = Color.white;

            Rect sliderRect = new Rect(16f, dedicationRect.yMax + 8f, width - 32f, 16f);
            float startingTime = Widgets.HorizontalSlider(sliderRect, sermon.PreferredStartTime, 0f, 23f, false, "SermonStartingTime".Translate(sermon.PreferredStartTime.ToString("D2")), "0", "23", 1);
            sermon.SetStartTime((int)startingTime);

            Rect durationRect = new Rect(sliderRect);
            durationRect.y = sliderRect.yMax + 8f;

            string currentDurationText = sermon.SermonDurationHours < 1f ? string.Concat(sermon.SermonDurationHours * 60, " min") : string.Concat(sermon.SermonDurationHours.ToString("0.0"), " h");

            float duration = Widgets.HorizontalSlider(durationRect, sermon.SermonDurationHours, 0.5f, 8f, false, "SermonDuration".Translate(currentDurationText), "30 min", "8h", 0.5f);
            sermon.SermonDurationHours = duration;

            Rect activeRect = new Rect(4f, durationRect.yMax + 8f, width, Text.LineHeight);
            Widgets.CheckboxLabeled(activeRect, "SermonActive".Translate(), ref sermon.Active, false, null, null, true);


            Rect debugRect = new Rect(activeRect.xMax - 196f, activeRect.y, 196f, 25f);
            if (altar.CurrentActiveSermon == sermon)
            {
                if (Widgets.ButtonText(debugRect, "EndSermon".Translate()))
                {
                    Lord lord = altar.Map.lordManager.LordOf(altar);
                    lord.ReceiveMemo("ForceEndSermon");
                    altar.EndSermon();
                }
            }
            else if (Prefs.DevMode)
            {
                if (Widgets.ButtonText(debugRect, "Debug: Force Start"))
                {
                    altar.TryStartSermon(sermon);
                }
            }

            Widgets.DrawLineHorizontal(6f, activeRect.yMax + 4f, width - 6f);
            return activeRect.yMax + 16f;
        }

        public static void OpenDedicationSelectMenu(BuildingAltar altar, SermonTemplate template)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            foreach (GodDef god in GlobalWorshipTracker.Current.PlayerPantheon.GodsListForReading.Where(x => x.acceptsPrayers && (altar.CompShrine.Props.dedicatedTo.NullOrEmpty() || altar.CompShrine.Props.dedicatedTo.Contains(x))))
            {
                string text1 = god.LabelCap;

                Action action;
                action = delegate
                {
                    template.DedicatedTo = god;
                };
                list.Add(new FloatMenuOption(text1, action, MenuOptionPriority.Default, null, null, 0f, null));

            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        public static void OpenPreacherSelectMenu(BuildingAltar altar, SermonTemplate sermon)
        {
            Find.WindowStack.Add(new Dialog_AssignPreacher(altar, sermon));
        }
        public static void OpenAssistantSelectMenu(BuildingAltar altar, SermonTemplate sermon)
        {
            Find.WindowStack.Add(new Dialog_AssignAssistant(altar, sermon));
        }
    }
}

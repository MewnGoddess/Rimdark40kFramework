using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using ColourPicker;
using UnityEngine;
using Verse;
using Verse.Sound;
using VFECore;

namespace Core40k
{
    [StaticConstructorOnStartup]
    public class Dialog_PaintSecondaryColour : Window
    {
        private Pawn pawn;

        Vector3 PortraitOffset = new Vector3(0f, 0f, 0.15f);

        private Vector2 apparelColorScrollPosition;

        private float viewRectHeight;

        private static readonly Vector2 ButSize = new Vector2(200f, 40f);

        public override Vector2 InitialSize => new Vector2(950f, 750f);

        private bool DevMode => Prefs.DevMode;
        
        private List<ColourPresetDef> presets;

        private readonly List<ApparelColourTwoTabDef> allTabs;
        
        private readonly List<TabRecord> tabs;

        private static readonly string MainTab = "BEWH.Framework.ApparelColourTwo.MainTab".Translate();

        private string curTab;

        public Dialog_PaintSecondaryColour()
        {
        }

        public Dialog_PaintSecondaryColour(Pawn pawn)
        {
            this.pawn = pawn;
            
            presets = DefDatabase<ColourPresetDef>.AllDefs.ToList();
            
            allTabs = DefDatabase<ApparelColourTwoTabDef>.AllDefsListForReading;
            
            tabs = new List<TabRecord>();
            
            var mainTabRecord = new TabRecord(MainTab, delegate
            {
                curTab = MainTab;
            }, curTab == MainTab);
            
            tabs.Add(mainTabRecord);
            
            foreach (var item in pawn.apparel.WornApparel.Where(a => a is ApparelColourTwo).Cast<ApparelColourTwo>())
            {
                item.SetOriginals();
                if (!item.def.HasModExtension<DefModExtension_EnableTabDef>())
                {
                    continue;
                }
                
                var tabDef = item.def.GetModExtension<DefModExtension_EnableTabDef>().tabDef;
                if (!allTabs.Contains(tabDef))
                {
                    continue;
                }
                
                var tabRecord = new TabRecord(tabDef.label, delegate
                {
                    curTab = tabDef.label;
                }, curTab == tabDef.label);

                if (!Enumerable.Any(tabs, tab => tab.label == tabRecord.label))
                {
                    tabs.Add(tabRecord);
                }
            }

            curTab = tabs.FirstOrDefault()?.label;
            
            Find.TickManager.Pause();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            var rect = new Rect(inRect)
            {
                height = Text.LineHeight * 2f
            };
            Widgets.Label(rect, "StylePawn".Translate().CapitalizeFirst() + ": " + Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name));
            Text.Font = GameFont.Small;
            inRect.yMin = rect.yMax + 4f;
            var rect2 = inRect;
            rect2.width *= 0.3f;
            rect2.yMax -= ButSize.y + 4f;
            DrawPawn(rect2);
            var rect3 = inRect;
            rect3.xMin = rect2.xMax + 10f;
            rect3.yMax -= ButSize.y + 4f;
            
            Widgets.DrawMenuSection(rect3);
            TabDrawer.DrawTabs(rect3, tabs);
            rect3 = rect3.ContractedBy(18f);
            
            if (curTab == MainTab)
            {
                DrawApparelColor(rect3);
            }
            else
            {
                var tab = allTabs.FirstOrDefault(def => def.label == curTab).tabDrawerClass;
                var tabDrawer = (ApparelColourTwoTabDrawer)Activator.CreateInstance(tab);
                tabDrawer.DrawTab(rect3, pawn, ref apparelColorScrollPosition);
            }
            
            DrawBottomButtons(inRect);
        }

        private void DrawPawn(Rect rect)
        {
            Widgets.BeginGroup(rect);
            for (var i = 0; i < 4; i++)
            {
                var position = new Rect(0f, rect.height / 4f * i, rect.width, rect.height / 4f).ContractedBy(4f);
                var image = PortraitsCache.Get(pawn, new Vector2(position.width, position.height), new Rot4(3 - i), PortraitOffset, 1.1f, supersample: true, compensateForUIScale: true, true, true, null, null, stylingStation: true);
                GUI.DrawTexture(position, image);
            }
            Widgets.EndGroup();
        }

        private void DrawApparelColor(Rect rect)
        {
            var viewRect = new Rect(rect.x, rect.y, rect.width - 16f, viewRectHeight);
            Widgets.BeginScrollView(rect, ref apparelColorScrollPosition, viewRect);
            var curY = rect.y;
            var apparelColourTwos = pawn.apparel.WornApparel.Where(a => a is ApparelColourTwo).Cast<ApparelColourTwo>();
            foreach (var item in apparelColourTwos)
            {
                //Item name
                var nameRect = new Rect(rect.x, curY, viewRect.width, 30f);
                nameRect.width /= 2;
                nameRect.x += nameRect.width / 2;
                Widgets.DrawMenuSection(nameRect);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(nameRect, item.Label);
                Text.Anchor = TextAnchor.UpperLeft;
                
                //Select button
                var selectPresetRect = new Rect(rect.x, curY, viewRect.width, 30f);
                selectPresetRect.width /= 5;
                selectPresetRect.x = nameRect.xMax + nameRect.width/20;
                if (Widgets.ButtonText(selectPresetRect, "BEWH.Framework.ApparelColourTwo.SelectPreset".Translate()))
                {
                    var list = new List<FloatMenuOption>();
                    foreach (var preset in presets)
                    {
                        var menuOption = new FloatMenuOption(preset.label, delegate
                        {
                            item.DrawColor = preset.primaryColour;
                            item.SetSecondaryColor(preset.secondaryColour);
                            
                        }, Core40kUtils.ColourPreview(preset.primaryColour, preset.secondaryColour), Color.white);
                        list.Add(menuOption);
                    }
                    
                    var gameComp = Current.Game.GetComponent<GameComponent_SavedPresets>();
                    foreach (var preset in gameComp.colourPresetDefs)
                    {
                        var menuOption = new FloatMenuOption(preset.label.CapitalizeFirst(), delegate
                        {
                            item.DrawColor = preset.primaryColour;
                            item.SetSecondaryColor(preset.secondaryColour);
                            
                        }, Core40kUtils.ColourPreview(preset.primaryColour, preset.secondaryColour), Color.white);
                        list.Add(menuOption);
                    }
                
                    if (!list.NullOrEmpty())
                    {
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                }
                
                //Save button
                var savePresetRect = new Rect(rect.x, curY, viewRect.width, 30f);
                savePresetRect.width /= 5;
                savePresetRect.x = nameRect.xMin - savePresetRect.width - nameRect.width/20;
                if (Widgets.ButtonText(savePresetRect, "BEWH.Framework.ApparelColourTwo.EditPreset".Translate()))
                {
                    var list = new List<FloatMenuOption>();
                    
                    var gameComp = Current.Game.GetComponent<GameComponent_SavedPresets>();
                    //Delete or override existing
                    foreach (var preset in gameComp.colourPresetDefs)
                    {
                        var menuOption = new FloatMenuOption(preset.label, delegate
                        {
                            gameComp.UpdatePreset(preset, item.DrawColor, item.DrawColorTwo);
                        }, Widgets.PlaceholderIconTex, Color.white);
                        menuOption.extraPartWidth = 30f;
                        menuOption.extraPartOnGUI = rect1 => Core40kUtils.DeletePreset(rect1, gameComp, preset);
                        menuOption.tooltip = "BEWH.Framework.ApparelColourTwo.OverridePreset".Translate(preset.label);
                        list.Add(menuOption);
                    }
                    
                    //Create new
                    var newPreset = new FloatMenuOption("BEWH.Framework.ApparelColourTwo.NewPreset".Translate(), delegate
                    {
                        var newColourPreset = new ColourPreset
                        {
                            primaryColour = item.DrawColor,
                            secondaryColour = item.DrawColorTwo,
                        };
                        Find.WindowStack.Add( new Dialog_EnterNewName(gameComp, newColourPreset));
                    }, Widgets.PlaceholderIconTex, Color.white);
                    list.Add(newPreset);
                
                    if (!list.NullOrEmpty())
                    {
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                }
                
                
                curY += nameRect.height + 3f;
                var itemRect = new Rect(rect.x, curY, viewRect.width, 92f);
                curY += itemRect.height;
                
                if (!pawn.apparel.IsLocked(item) || DevMode)
                {
                    //Primary Color
                    var colorOneRect = new Rect(itemRect);
                    colorOneRect.width /= 2;
                    colorOneRect.x = itemRect.xMin + 1f;
                    colorOneRect.y = itemRect.yMin + 2f;
                    Widgets.DrawMenuSection(colorOneRect.ContractedBy(-1));
                    Widgets.DrawRectFast(colorOneRect, item.DrawColor);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(colorOneRect, "BEWH.Framework.ApparelColourTwo.PrimaryColor".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                    if (Widgets.ButtonInvisible(colorOneRect))
                    {
                        Find.WindowStack.Add( new Dialog_ColourPicker( item.DrawColor, ( newColour ) =>
                        {
                            item.DrawColor = newColour;
                        } ) );
                    }
                    
                    //Secondary Color
                    var colorTwoRect = new Rect(itemRect);
                    colorTwoRect.width /= 2;
                    colorTwoRect.x = itemRect.xMax - colorTwoRect.width - 1f;
                    colorTwoRect.y = itemRect.yMin + 2f;
                    Widgets.DrawMenuSection(colorTwoRect.ContractedBy(-1));
                    Widgets.DrawRectFast(colorTwoRect, item.DrawColorTwo);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(colorTwoRect, "BEWH.Framework.ApparelColourTwo.SecondaryColor".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                    if (Widgets.ButtonInvisible(colorTwoRect))
                    {
                        Find.WindowStack.Add( new Dialog_ColourPicker( item.DrawColorTwo, ( newColour ) =>
                        {
                            item.SetSecondaryColor(newColour);
                        } ) );
                    }
                }
                else
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    var rect3 = itemRect;
                    rect3.x += 100f;
                    Widgets.Label(rect3, ((string)"ApparelLockedCannotRecolor".Translate(pawn.Named("PAWN"), item.Named("APPAREL"))).Colorize(ColorLibrary.RedReadable));
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                curY += 34f;
            }
            if (Event.current.type == EventType.Layout)
            {
                viewRectHeight = curY - rect.y;
            }
            Widgets.EndScrollView();
        }

        private void DrawBottomButtons(Rect inRect)
        {
            if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Cancel".Translate()))
            {
                Close();
                Reset();
            }
            if (Widgets.ButtonText(new Rect(inRect.xMin + inRect.width / 2f - ButSize.x / 2f, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Reset".Translate()))
            {
                Reset();
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            }
            if (Widgets.ButtonText(new Rect(inRect.xMax - ButSize.x, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Accept".Translate()))
            {
                ApplyApparelColors();
                Close();
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            if (closeOnCancel || closeOnClickedOutside)
            {
                Reset();
            }

            foreach (var tab in allTabs)
            {
                if (tab.label == MainTab)
                {
                    continue;
                }
                var tabDrawer = (ApparelColourTwoTabDrawer)Activator.CreateInstance(tab.tabDrawerClass);
                tabDrawer.OnClose(closeOnCancel, closeOnClickedOutside);
            }
            
            base.Close(doCloseSound);
        }

        private void Reset(bool resetColors = true)
        {
            if (resetColors)
            {
                foreach (var item in pawn.apparel.WornApparel.Where(a => a is ApparelColourTwo).Cast<ApparelColourTwo>())
                {
                    item.Reset();
                }
            }
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        private void ApplyApparelColors()
        {
            foreach (var item in pawn.apparel.WornApparel.Where(a => a is ApparelColourTwo).Cast<ApparelColourTwo>())
            {
                item.Notify_ColorChanged();
                item.SetOriginals();
            }

            foreach (var tab in allTabs)
            {
                if (tab.label == MainTab)
                {
                    continue;
                }
                var tabDrawer = (ApparelColourTwoTabDrawer)Activator.CreateInstance(tab.tabDrawerClass);
                tabDrawer.OnAccept();
            }
        }
    }
}
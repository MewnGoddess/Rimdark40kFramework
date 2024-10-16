using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Core40k
{
    [StaticConstructorOnStartup]
    public class Dialog_PaintSecondaryColour : Window
    {
        private Pawn pawn;

        private Thing stylingStation;

        private bool showHeadgear;

        private bool showClothes;

        Vector3 PortraitOffset = new Vector3(0f, 0f, 0.15f);

        private Vector2 apparelColorScrollPosition;

        private float viewRectHeight;

        private static readonly Texture2D FavoriteColorTex = ContentFinder<Texture2D>.Get("UI/Icons/ColorSelector/ColorFavourite");

        private static readonly Texture2D IdeoColorTex = ContentFinder<Texture2D>.Get("UI/Icons/ColorSelector/ColorIdeology");

        private static readonly Vector2 ButSize = new Vector2(200f, 40f);

        public override Vector2 InitialSize => new Vector2(950f, 750f);

        private bool DevMode => Prefs.DevMode;

        private List<Color> allColors;

        private List<Color> AllColors
        {
            get
            {
                if (allColors == null)
                {
                    allColors = new List<Color>();
                    if (pawn.Ideo != null && !Find.IdeoManager.classicMode)
                    {
                        allColors.Add(pawn.Ideo.ApparelColor);
                    }
                    if (pawn.story != null && !pawn.DevelopmentalStage.Baby() && pawn.story.favoriteColor.HasValue && !allColors.Any((Color c) => pawn.story.favoriteColor.Value.IndistinguishableFrom(c)))
                    {
                        allColors.Add(pawn.story.favoriteColor.Value);
                    }
                    foreach (ColorDef colDef in DefDatabase<ColorDef>.AllDefs.Where((ColorDef x) => x.colorType == ColorType.Ideo || x.colorType == ColorType.Misc || (DevMode && !ModsConfig.IdeologyActive && x.colorType == ColorType.Structure)))
                    {
                        if (!allColors.Any((Color x) => x.IndistinguishableFrom(colDef.color)))
                        {
                            allColors.Add(colDef.color);
                        }
                    }
                    allColors.SortByColor((Color x) => x);
                }
                return allColors;
            }
        }

        public Dialog_PaintSecondaryColour()
        { }

        public Dialog_PaintSecondaryColour(Pawn pawn, Thing stylingStation)
        {
            this.pawn = pawn;
            this.stylingStation = stylingStation;
            showClothes = true;
            showHeadgear = true;
            foreach (ApparelColourTwo item in pawn.apparel.WornApparel.Where(a => a is ApparelColourTwo).Cast<ApparelColourTwo>())
            {
                item.SetOriginalColor(item.DrawColorTwo);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect rect = new Rect(inRect)
            {
                height = Text.LineHeight * 2f
            };
            Widgets.Label(rect, "StylePawn".Translate().CapitalizeFirst() + ": " + Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name));
            Text.Font = GameFont.Small;
            inRect.yMin = rect.yMax + 4f;
            Rect rect2 = inRect;
            rect2.width *= 0.3f;
            rect2.yMax -= ButSize.y + 4f;
            DrawPawn(rect2);
            Rect rect3 = inRect;
            rect3.xMin = rect2.xMax + 10f;
            rect3.yMax -= ButSize.y + 4f;
            DrawApparelColor(rect3);
            DrawBottomButtons(inRect);

        }

        private void ColorSelecterExtraOnGUI(Color color, Rect boxRect)
        {
            Texture2D texture2D = null;
            TaggedString taggedString = null;
            bool flag = Mouse.IsOver(boxRect);
            Pawn_StoryTracker story = pawn.story;
            if (story != null && story.favoriteColor.HasValue && color.IndistinguishableFrom(pawn.story.favoriteColor.Value))
            {
                texture2D = FavoriteColorTex;
                if (flag)
                {
                    taggedString = "FavoriteColorPickerTip".Translate(pawn.Named("PAWN"));
                }
            }
            else if (pawn.Ideo != null && !Find.IdeoManager.classicMode && color.IndistinguishableFrom(pawn.Ideo.ApparelColor))
            {
                texture2D = IdeoColorTex;
                if (flag)
                {
                    taggedString = "IdeoColorPickerTip".Translate(pawn.Named("PAWN"));
                }
            }
            if (texture2D != null)
            {
                Rect position = boxRect.ContractedBy(4f);
                GUI.color = Color.black.ToTransparent(0.2f);
                GUI.DrawTexture(new Rect(position.x + 2f, position.y + 2f, position.width, position.height), texture2D);
                GUI.color = Color.white.ToTransparent(0.8f);
                GUI.DrawTexture(position, texture2D);
                GUI.color = Color.white;
            }
            if (!taggedString.NullOrEmpty())
            {
                TooltipHandler.TipRegion(boxRect, taggedString);
            }
        }

        private void DrawPawn(Rect rect)
        {
            Rect rect2 = rect;
            rect2.yMin = rect.yMax - Text.LineHeight * 2f;
            Widgets.CheckboxLabeled(new Rect(rect2.x, rect2.y, rect2.width, rect2.height / 2f), "ShowHeadgear".Translate(), ref showHeadgear);
            Widgets.CheckboxLabeled(new Rect(rect2.x, rect2.y + rect2.height / 2f, rect2.width, rect2.height / 2f), "ShowApparel".Translate(), ref showClothes);
            rect.yMax = rect2.yMin - 4f;
            Widgets.BeginGroup(rect);
            for (int i = 0; i < 3; i++)
            {
                Rect position = new Rect(0f, rect.height / 3f * (float)i, rect.width, rect.height / 3f).ContractedBy(4f);
                RenderTexture image = PortraitsCache.Get(pawn, new Vector2(position.width, position.height), new Rot4(2 - i), PortraitOffset, 1.1f, supersample: true, compensateForUIScale: true, showHeadgear, showClothes, null, null, stylingStation: true);
                GUI.DrawTexture(position, image);
            }
            Widgets.EndGroup();
        }

        private void DrawApparelColor(Rect rect)
        {
            Rect viewRect = new Rect(rect.x, rect.y, rect.width - 16f, viewRectHeight);
            Widgets.BeginScrollView(rect, ref apparelColorScrollPosition, viewRect);
            float curY = rect.y;
            IEnumerable<ApparelColourTwo> apparelColourTwos = pawn.apparel.WornApparel.Where(a => a is ApparelColourTwo).Cast<ApparelColourTwo>();
            foreach (ApparelColourTwo item in apparelColourTwos)
            {
                var value = item.DrawColorTwo;
                Rect rect2 = new Rect(rect.x, curY, viewRect.width, 92f);
                curY += rect2.height + 10f;
                if (!pawn.apparel.IsLocked(item) || DevMode)
                {
                    Widgets.ColorSelector(rect2, ref value, AllColors, out var _, null, 22, 2, ColorSelecterExtraOnGUI);
                    float num2 = rect2.x;
                    if (pawn.Ideo != null && !Find.IdeoManager.classicMode)
                    {
                        rect2 = new Rect(num2, curY, 200f, 24f);
                        if (Widgets.ButtonText(rect2, "SetIdeoColor".Translate()))
                        {
                            value = pawn.Ideo.ApparelColor;
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        }
                        num2 += 210f;
                    }
                    Pawn_StoryTracker story = pawn.story;
                    if (story != null && story.favoriteColor.HasValue)
                    {
                        rect2 = new Rect(num2, curY, 200f, 24f);
                        if (Widgets.ButtonText(rect2, "SetFavoriteColor".Translate()))
                        {
                            value = pawn.story.favoriteColor.Value;
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        }
                    }
                    item.SetSecondaryColor(value);
                }
                else
                {
                    Widgets.ColorSelectorIcon(new Rect(rect2.x, rect2.y, 88f, 88f), item.def.uiIcon, value);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Rect rect3 = rect2;
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
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            if (closeOnCancel || closeOnClickedOutside)
            {
                Reset();
            }
            base.Close();
        }

        private void Reset(bool resetColors = true)
        {
            if (resetColors)
            {
                foreach (ApparelColourTwo item in pawn.apparel.WornApparel.Where(a => a is ApparelColourTwo).Cast<ApparelColourTwo>())
                {
                    item.ResetSecondaryColor();
                }
            }
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        private void ApplyApparelColors()
        {
            foreach (ApparelColourTwo item in pawn.apparel.WornApparel.Where(a => a is ApparelColourTwo).Cast<ApparelColourTwo>())
            {
                item.SetSecondaryColor(item.DrawColorTwoTemp);
                item.Notify_ColorChanged();
            }
            base.Close();
        }
    }
}
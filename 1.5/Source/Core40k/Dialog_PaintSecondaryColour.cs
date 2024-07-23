using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;


namespace Core40k
{
    [StaticConstructorOnStartup]
    public class Dialog_PaintSecondaryColour : Window
    {
        private readonly ApparelColourTwo apparel;

        private Pawn pawn;

        private Color chosenColour;

        private static readonly Texture2D FavoriteColorTex = ContentFinder<Texture2D>.Get("UI/Icons/ColorSelector/ColorFavourite");

        private static readonly Texture2D IdeoColorTex = ContentFinder<Texture2D>.Get("UI/Icons/ColorSelector/ColorIdeology");

        private static readonly Vector2 ButSize = new Vector2(200f, 40f);

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

        public Dialog_PaintSecondaryColour(ApparelColourTwo apparel, Pawn pawn)
        {
            this.apparel = apparel;
            this.pawn = pawn;
            chosenColour = apparel.DrawColorTwo;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect rect = new Rect(inRect);
            rect.height = Text.LineHeight * 2f;
            Widgets.Label(rect, "BEWH.ChangeSecondaryColour".Translate().CapitalizeFirst());
            Text.Font = GameFont.Small;
            inRect.yMin = rect.yMax + 4f;
            Rect rect2 = inRect;
            DrawApparelColor(rect2);
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

        private void DrawApparelColor(Rect rect)
        {
            float curY = rect.y;
            Rect rect2 = new Rect(rect.x, curY, rect.width, 92f);
            curY += rect2.height + 10f;
            if (!pawn.apparel.IsLocked(apparel) || DevMode)
            {
                Widgets.ColorSelector(rect2, ref chosenColour, AllColors, out var _, Widgets.GetIconFor(apparel.def, null, apparel.StyleDef), 22, 2, ColorSelecterExtraOnGUI);
                float num2 = rect2.x;
                if (pawn.Ideo != null && !Find.IdeoManager.classicMode)
                {
                    rect2 = new Rect(num2, curY, 200f, 24f);
                    if (Widgets.ButtonText(rect2, "SetIdeoColor".Translate()))
                    {
                        chosenColour = pawn.Ideo.ApparelColor;
                    }
                    num2 += 210f;
                }
                Pawn_StoryTracker story = pawn.story;
                if (story != null && story.favoriteColor.HasValue)
                {
                    rect2 = new Rect(num2, curY, 200f, 24f);
                    if (Widgets.ButtonText(rect2, "SetFavoriteColor".Translate()))
                    {
                        chosenColour = pawn.story.favoriteColor.Value;
                    }
                }
            }
            else
            {
                Widgets.ColorSelectorIcon(new Rect(rect2.x, rect2.y, 88f, 88f), apparel.def.uiIcon, chosenColour);
                Text.Anchor = TextAnchor.MiddleLeft;
                Rect rect3 = rect2;
                rect3.x += 100f;
                Widgets.Label(rect3, ((string)"ApparelLockedCannotRecolor".Translate(pawn.Named("PAWN"), apparel.Named("APPAREL"))).Colorize(ColorLibrary.RedReadable));
                Text.Anchor = TextAnchor.UpperLeft;
            }
            curY += 34f;
        }

        private void DrawBottomButtons(Rect inRect)
        {
            if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Cancel".Translate()))
            {
                Close();
            }
            if (Widgets.ButtonText(new Rect(inRect.xMax - ButSize.x, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Accept".Translate()))
            {
                apparel.SetSecondaryColor(chosenColour);
            }
        }
    }
}
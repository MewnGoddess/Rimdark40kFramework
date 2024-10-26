using RimWorld;
using UnityEngine;
using Verse;


namespace Core40k
{
    [StaticConstructorOnStartup]
    public class ApparelColourTwo : Apparel
    {
        private Color originalColor = Color.black;

        private Color drawColorTwo = Color.red;

        private Color drawColorOne = Color.white;

        private Color drawColorTwoToSet = Color.red;

        public override Color DrawColor
        {
            get
            {
                var comp = GetComp<CompColorable>();
                if (comp != null && comp.Active)
                {
                    return comp.Color;
                }
                return drawColorOne;
            }
        }

        public override Color DrawColorTwo => drawColorTwo;

        public Color DrawColorTwoTemp => drawColorTwoToSet;

        public void SetOriginalColor(Color color)
        {
            originalColor = color;
        }

        public void SetSecondaryColor(Color color)
        {
            drawColorTwo = color;
            drawColorTwoToSet = color;
            Notify_ColorChanged();
        }

        public void ResetSecondaryColor()
        {
            drawColorTwo = originalColor;
            Notify_ColorChanged();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref originalColor, "originalColor", Color.black);
            Scribe_Values.Look(ref drawColorTwoToSet, "drawColorTwoToSet", Color.red);
            Scribe_Values.Look(ref drawColorTwo, "drawColorTwo", Color.red);
            Scribe_Values.Look(ref drawColorOne, "drawColorOne", Color.white);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Notify_ColorChanged();
            }
        }
    }
}
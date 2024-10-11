using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;


namespace Core40k
{
    [StaticConstructorOnStartup]
    public class ApparelColourTwo : Apparel
    {
        private Color drawColorTwo = Color.red;

        private Color drawColorOne = Color.white;

        private static readonly Texture2D ChangePlumeColourIcon = ContentFinder<Texture2D>.Get("UI/Genes/AeldariPsyker_icon");

        public override Color DrawColor
        {
            get
            {
                CompColorable comp = GetComp<CompColorable>();
                if (comp != null && comp.Active)
                {
                    return comp.Color;
                }
                return drawColorOne;
            }
        }

        public override Color DrawColorTwo
        {
            get
            {
                return drawColorTwo;
            }
        }
        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            Command_Action command_Action1 = new Command_Action();
            command_Action1.defaultLabel = "BEWH.ChangeSecondaryColour".Translate();
            command_Action1.defaultDesc = "BEWH.ChangeSecondaryColourDesc".Translate(def.label);
            command_Action1.icon = ChangePlumeColourIcon;
            command_Action1.action = delegate
            {
                Find.WindowStack.Add(new Dialog_PaintSecondaryColour(this, this.Wearer));
            };
            yield return command_Action1;
        }

        public void SetSecondaryColor(Color color)
        {
            drawColorTwo = color;
            Notify_ColorChanged();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref drawColorTwo, "drawColorTwo", Color.red);
            Scribe_Values.Look(ref drawColorOne, "drawColorOne", Color.white);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Notify_ColorChanged();
            }
        }
    }
}
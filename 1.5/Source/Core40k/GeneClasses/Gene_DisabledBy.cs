using System.Linq;
using Verse;

namespace Core40k
{
    public class Gene_DisabledBy : Gene
    {
        public override bool Active
        {
            get
            {
                if (def.HasModExtension<DefModExtension_GeneDisabledBy>())
                {
                    var disabledByGenes = def.GetModExtension<DefModExtension_GeneDisabledBy>().geneDisabledBy;
                    if (Enumerable.Any(disabledByGenes, gene => pawn.genes.HasActiveGene(gene)))
                    {
                        return false;
                    }
                }
                return base.Active;
            }
        }
    }
}
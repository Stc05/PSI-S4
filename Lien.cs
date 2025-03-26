using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rendu2_PSI
{
    public class Lien
    {
        private Noeud initiale;
        private Noeud terminale;
        private double duree;

        public Lien(Noeud initiale, Noeud terminale, double duree)
        {
            this.initiale = initiale;
            this.terminale = terminale;
            this.duree = duree;
        }
        #region propriété
        public Noeud Initiale{get { return this.initiale; } }
        public Noeud Terminale{get { return this.terminale; }}
        public double Duree{ get { return this.duree; } }
        #endregion

    }
}

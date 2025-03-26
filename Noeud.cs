using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rendu2_PSI
{
    public class Noeud
    {
        private int id;
        private string ligne;
        private string nom;
        private double longitude;
        private double latitude;
        private string commune;
        private string postal;
        private string sens;

        public Noeud(int id, string ligne, string nom, double longitude, double latitude, string commune, string postal, string sens)
        {
            this.id = id;
            this.ligne = ligne;
            this.nom = nom;
            this.longitude = longitude;
            this.latitude = latitude;
            this.postal = postal;
            this.commune = commune;
            if(sens != null)
            {
                this.sens = sens;
            }
        }
        #region propriétés
        public int Id{ get { return this.id; }}
        public string Ligne{get { return this.ligne; }}
        public string Nom{get { return this.nom; }}
        public double Longitude{get { return this.longitude; }}
        public double Latitude{get { return this.latitude; }}
        public string Commune{get { return this.commune; }}
        public string Postal{get { return this.postal; }}

        public string Sens { get { return this.sens; } }

        #endregion
    }
}

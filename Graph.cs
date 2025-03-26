using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rendu2_PSI
{
    public class Graph
    {
        private string lien_fichier;
        private Dictionary<int, Noeud> noeuds;
        private List<Lien> liens;
        private double[,] matriceAdjacente;

        public Graph(string lien_fichier)
        {
            this.noeuds = Creation_Noeud(lien_fichier);
            this.liens = Creation_Lien(Liste_stations_par_ligne(this.Noeuds));
            this.matriceAdjacente = Matrice_Adjacente();
        }

        #region propriétés
        public Dictionary<int, Noeud> Noeuds { get { return this.noeuds; } }
        public List<Lien> Liens { get { return this.liens; } }
        public double[,] MatriceAdjacente { get { return this.matriceAdjacente; } }
        #endregion 
        public Dictionary<int, Noeud> Creation_Noeud(string lien_fichier)
        {
            Dictionary<int, Noeud> noeuds = new Dictionary<int, Noeud>();
            try
            {
                foreach (string ligne in File.ReadLines(lien_fichier))
                {

                    string[] tab = ligne.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    if (tab.Length == 8)
                    {
                        Noeud noeud = new Noeud(int.Parse(tab[0]), tab[1], tab[2], double.Parse(tab[3].Trim(), CultureInfo.InvariantCulture), double.Parse(tab[4].Trim(), CultureInfo.InvariantCulture), tab[5], tab[6], tab[7]);
                        noeuds[noeud.Id] = noeud;
                    }
                    else
                    {
                        Noeud noeud = new Noeud(int.Parse(tab[0]), tab[1], tab[2], double.Parse(tab[3].Trim(), CultureInfo.InvariantCulture), double.Parse(tab[4].Trim(), CultureInfo.InvariantCulture), tab[5], tab[6], null);
                        noeuds[noeud.Id] = noeud;
                    }
                }
                return noeuds;

            }
            catch (FileNotFoundException f)
            {
                Console.WriteLine("Le fichier n'existe pas");
                Console.WriteLine(f.Message);
                return noeuds;
            }
        }

        public Dictionary<string, List<Noeud>> Liste_stations_par_ligne(Dictionary<int, Noeud> noeuds)
        {
            Dictionary<string, List<Noeud>> liste_listes = new Dictionary<string, List<Noeud>>();
            List<string> lignes = new List<string>();
            foreach (Noeud n in this.noeuds.Values)
            {
                if (!lignes.Contains(n.Ligne))
                {
                    List<Noeud> liste = new List<Noeud> { noeuds[n.Id] };
                    liste_listes[n.Ligne] = liste;
                    lignes.Add(n.Ligne);
                }
                else
                {
                    liste_listes[n.Ligne].Add(noeuds[n.Id]);
                }
            }
            return liste_listes;
        }
        public double Distance(Noeud noeud1, Noeud noeud2)
        {
            double R = 6371;
            double DeltaPhi = (noeud2.Latitude - noeud1.Latitude) * Math.PI / 180.0;
            double DeltaLambda = (noeud2.Longitude - noeud1.Longitude) * Math.PI / 180.0;
            double distance = 2 * R * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(DeltaPhi / 2), 2) + Math.Cos(noeud2.Latitude * Math.PI / 180.0) * Math.Cos(noeud1.Latitude * Math.PI / 180.0) * Math.Pow(Math.Sin(DeltaLambda / 2), 2)));
            return distance;
        }
        public double[,] Matrice_Distance(List<Noeud> liste, List<Noeud> interdit)
        {
            double[,] matrice = new double[liste.Count, liste.Count];
            for (int i = 0; i < liste.Count; i++)
            {
                for (int j = 0; j < liste.Count; j++)
                {
                    if (interdit.Contains(liste[i]) || interdit.Contains(liste[j]))
                    {
                        matrice[i, j] = 50;
                    }
                    else
                    {
                        matrice[i, j] = Distance(liste[i], liste[j]);
                    }

                }
            }
            return matrice;
        }
        public Noeud Noeud_Plus_Proche(double[,] matrice_Distance, Noeud terminal1, Noeud depart)
        {
            int indexe_depart = depart.Id - terminal1.Id;
            double distance_min = 50;
            int indexe_max = 0;
            for (int i = 0; i < matrice_Distance.GetLength(0); i++)
            {
                if (matrice_Distance[indexe_depart, i] != 0 && matrice_Distance[indexe_depart, i] < distance_min)
                {
                    distance_min = matrice_Distance[indexe_depart, i];
                    indexe_max = i;
                }
            }
            return this.Noeuds[indexe_max + terminal1.Id];
        }
        public List<Lien> Creation_Lien(Dictionary<string, List<Noeud>> list_station_par_ligne)
        {
            List<Lien> liens = new List<Lien>();
            foreach (KeyValuePair<string, List<Noeud>> n in list_station_par_ligne)
            {
                if (n.Key != "7bis" && n.Key != "10")
                {
                    List<Noeud> interdit = new List<Noeud>();
                    int taille = n.Value.Count;
                    Noeud terminal1 = this.Noeuds[n.Value[0].Id];
                    Noeud terminal2 = this.Noeuds[n.Value[taille - 1].Id];
                    for (int i = 0; i < taille - 1; i++)
                    {
                        if (!interdit.Contains(this.Noeuds[this.Noeuds[n.Value[0].Id].Id + i]))
                        {
                            Noeud plus_proche = Noeud_Plus_Proche(Matrice_Distance(n.Value, interdit), terminal1, this.Noeuds[i + n.Value[0].Id]);
                            interdit.Add(this.Noeuds[i + n.Value[0].Id]);
                            Lien lien1 = new Lien(this.Noeuds[i + n.Value[0].Id], plus_proche, Distance(this.Noeuds[i + n.Value[0].Id], plus_proche) / 25);
                            Lien lien2 = new Lien(plus_proche, this.Noeuds[i + n.Value[0].Id], Distance(this.Noeuds[i + n.Value[0].Id], plus_proche) / 25);
                            liens.Add(lien1);
                            liens.Add(lien2);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < 2; j++)
                    {
                        string sens = "";
                        if (j == 0)
                        {
                            sens = "-1";
                        }
                        else
                        {
                            sens = "1";
                        }
                        List<Noeud> interdit = Interdit(n.Value, sens);
                        int taille = n.Value.Count;
                        Noeud terminal1 = this.Noeuds[n.Value[0].Id];
                        Noeud terminal2 = this.Noeuds[n.Value[taille - 1].Id];
                        for (int i = 0; i < taille - 1; i++)
                        {
                            if (!interdit.Contains(this.Noeuds[this.Noeuds[n.Value[0].Id].Id + i]))
                            {
                                Noeud plus_proche = Noeud_Plus_Proche(Matrice_Distance(n.Value, interdit), terminal1, this.Noeuds[i + n.Value[0].Id]);
                                interdit.Add(this.Noeuds[i + n.Value[0].Id]);
                                Lien lien = new Lien(this.Noeuds[i + n.Value[0].Id], plus_proche, Distance(this.Noeuds[i + n.Value[0].Id], plus_proche) / 25);
                                liens.Add(lien);
                            }
                        }
                    }
                }
            }
            liens.AddRange(Creation_Lien_Changements());
            return liens;
        }

        public List<Lien> Creation_Lien_Changements()
        {
            List<Lien> liens2 = new List<Lien>();
            foreach (Noeud n in this.Noeuds.Values)
            {
                foreach (Noeud l in this.Noeuds.Values)
                {
                    if (n!=l && l.Nom == n.Nom && l.Id != n.Id)
                    {
                        Lien lien = new Lien(l, n, Distance(l, n)/25);
                        liens2.Add(lien);
                    }
                }
            }
            return liens2;
        }
        public void AfficherLiens2(List<Lien> liens)
        {
            Console.WriteLine("\nListe des liens entre les stations :");
            foreach (Lien lien in liens)
            {
                Console.WriteLine($"De {lien.Initiale.Nom} ({lien.Initiale.Id}) → {lien.Terminale.Nom} ({lien.Terminale.Id})");
            }
        }
        public void AfficherLiens()
        {
            Console.WriteLine("\nListe des liens entre les stations :");
            foreach (Lien lien in this.Liens)
            {
                Console.WriteLine($"De {lien.Initiale.Nom} ({lien.Initiale.Id}) → {lien.Terminale.Nom} ({lien.Terminale.Id})");
            }
        }

        public List<Noeud> Interdit(List<Noeud> noeuds, string sens)
        {
            List<Noeud> interdit = new List<Noeud>();
            foreach (Noeud n in noeuds)
            {
                if (n.Sens == sens)
                {
                    interdit.Add(n);
                }
            }
            return interdit;
        }

        /// <summary>
        /// créer la matrice adjacente associé au graphe
        /// </summary>
        /// <returns>matrice dentier</returns>
        public double[,] Matrice_Adjacente()
        {
            int taille = this.Noeuds.Count;
            double[,] matrice_adjacente = new double[taille + 1, taille + 1];
            for (int i = 0; i <= taille; i++)
            {
                for (int j = 0; j <= taille; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        matrice_adjacente[i, j] = 0;
                    }
                    else if (i == 0)
                    {
                        matrice_adjacente[i, j] = this.Noeuds[j].Id;
                    }
                    else if(j==0)
                    {
                        matrice_adjacente[i, j] = this.Noeuds[i].Id;
                    }
                    else
                    {
                        foreach (Lien l in this.liens)
                        {
                            if (l.Initiale.Id == this.Noeuds[i].Id && l.Terminale.Id == this.Noeuds[j].Id)
                            {
                                matrice_adjacente[i, j] = l.Duree* 60; //en minutes
                            }
                        }
                    }
                }
            }
            return matrice_adjacente;
        }


        /// <summary>
        /// affiche la matrice d'incidence
        /// </summary>
        public void Affichage_MatriceAdjacente()
        {
            for (int i = 0; i < this.MatriceAdjacente.GetLength(0); i++)
            {
                for (int j = 0; j < this.MatriceAdjacente.GetLength(1); j++)
                {
                    Console.Write(Math.Round(this.MatriceAdjacente[i, j], 2) + " ");
                }
                Console.WriteLine();
            }
        }

    }

}

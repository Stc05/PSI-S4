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

        /// <summary>
        /// Créer un dictionnaire dont la clé est lindice de la station et la valeur le noeud initialisé à partir d'un fichier excel pour faciliter l'accès à la référence d'un noeud à partir de son id
        /// </summary>
        /// <param name="lien_fichier">fichier excel des stations de métro</param>
        /// <returns>dictionnaire dont la clé est lindice de la station et la valeur le noeud </returns>
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

        /// <summary>
        /// Créer un dictionnaire permettant de regroupper les noeuds par leur ligne
        /// </summary>
        /// <param name="noeuds">Dictionnaire qui associe l'id de la station au noeud</param>
        /// <returns>dictionnaire dont la clé est un string de la ligne de la station et la valeur est la liste des stations appartenant à la ligne</returns>
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

        /// <summary>
        /// Calcule la distance entre deux station
        /// </summary>
        /// <param name="noeud1">station 1</param>
        /// <param name="noeud2">station 2</param>
        /// <returns>un double</returns>
        public double Distance(Noeud noeud1, Noeud noeud2)
        {
            double R = 6371;
            double DeltaPhi = (noeud2.Latitude - noeud1.Latitude) * Math.PI / 180.0;
            double DeltaLambda = (noeud2.Longitude - noeud1.Longitude) * Math.PI / 180.0;
            double distance = 2 * R * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(DeltaPhi / 2), 2) + Math.Cos(noeud2.Latitude * Math.PI / 180.0) * Math.Cos(noeud1.Latitude * Math.PI / 180.0) * Math.Pow(Math.Sin(DeltaLambda / 2), 2)));
            return distance;
        }

        /// <summary>
        /// Créer une matrice qui prend comme valeur la distance entre chacune des stations dune meme ligne à partir de leur coordonnées
        /// </summary>
        /// <param name="liste">liste de noeud qui correspond à l'ensemble des stations dune meme ligne</param>
        /// <param name="interdit">liste de noeuds que l'on souhaite ne pas prendre en compte pour le calcule de la distance</param>
        /// <returns>matrice de double</returns>
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

        /// <summary>
        /// Cherche la station la plus proche d'une station considéré
        /// </summary>
        /// <param name="matrice_Distance">matrice des distancees pour une ligne de métro</param>
        /// <param name="terminal1">premiere station de la ligne considérée</param>
        /// <param name="depart">station considéré</param>
        /// <returns>neoud de la station</returns>
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

        /// <summary>
        /// Créer les arc du graph en prenant en compte les spécificité de la ligne 10 et 7bis
        /// </summary>
        /// <param name="list_station_par_ligne">liste des stations d'une ligne de métro</param>
        /// <returns>lsite des liens entre chaque station de la ligne</returns>
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

        /// <summary>
        /// Créer les arcs pour une correspondance (station qui porte le meme nom et qui appartiennent pas à la meme ligne)
        /// </summary>
        /// <returns></returns>
        public List<Lien> Creation_Lien_Changements()
        {
            List<Lien> liens2 = new List<Lien>();
            foreach (Noeud n in this.Noeuds.Values)
            {
                foreach (Noeud l in this.Noeuds.Values)
                {
                    if (n!=l && l.Nom == n.Nom && l.Id != n.Id)
                    {
                        Lien lien = new Lien(l, n,0.065); //environ 4min par changements
                        liens2.Add(lien);
                    }
                }
            }
            return liens2;
        }

        /// <summary>
        /// Permet de faire la liste des stations dont on ne peut emprunter que dans un sens et ainsi ne pas le prendre en considération lorsque l'on cherche la station la plus proche dans la matrice distance
        /// </summary>
        /// <param name="noeuds"></param>
        /// <param name="sens">valeur null = double sens et valeur de 1 ou -1 pour savoir quel sens est ce que lon peut parcourir la ligne (valeur ajouté dans l'excel)</param>
        /// <returns>la liste des noeuds quon ne peut pas emprunter pour une certaine direction pour créer les arcs par la suite</returns>
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

        /// <summary>
        /// Créer un dictionnaire qui associe à chaque id dune station une liste de noeuds correspondant aux stations qui la précède 
        /// </summary>
        /// <param name="noeuds">dictionnaire qui associe l'id de la station à son noeud</param>
        /// <returns>dictionnaire</returns>
        public Dictionary<int, List<Noeud>> Liste_Noeud_precedant(Dictionary<int, Noeud> noeuds)
        {
            Dictionary<int, List<Noeud>> liste_Noeud_precedant = new Dictionary<int, List<Noeud>>();
            foreach (int n in this.Noeuds.Keys)
            {
                foreach(Lien lien in this.Liens)
                {
                    if(!liste_Noeud_precedant.ContainsKey(n) && lien.Terminale == this.Noeuds[n])
                    {
                        List<Noeud> liste = new List<Noeud> {lien.Terminale};
                        liste_Noeud_precedant[n] = liste;
                    }
                    if(liste_Noeud_precedant.ContainsKey(n) && lien.Terminale == this.Noeuds[n])
                    {
                        liste_Noeud_precedant[n].Add(lien.Terminale);
                    }
                }
            }
            return liste_Noeud_precedant;
        }

        /// <summary>
        /// Cherche le plus court chemin en utilisant l'algo de Dijkstra
        /// </summary>
        /// <param name="depart">station de départ</param>
        /// <param name="arrivee">tation d'arrivée</param>
        /// <returns>liste de noeudds du parcours du chemin le plus court</returns>
        public List<Noeud> Dijkstra(Noeud depart, Noeud arrivee)
        {
            int taille = this.Noeuds.Count;
            Dictionary<Noeud, double> distances = new Dictionary<Noeud, double>();
            Dictionary<Noeud, Noeud?> precedents = new Dictionary<Noeud, Noeud?>();
            HashSet<Noeud> visites = new HashSet<Noeud>();

            foreach (var noeud in this.Noeuds.Values)
            {
                distances[noeud] = double.MaxValue;
                precedents[noeud] = null;
            }
            distances[depart] = 0;

            while (visites.Count < taille)
            {
                Noeud courant = distances.Where(kvp => !visites.Contains(kvp.Key))
                                         .OrderBy(kvp => kvp.Value)
                                         .First().Key;

                visites.Add(courant);

                if (courant == arrivee) break;

                foreach (var voisin in this.Noeuds.Values)
                {
                    int indice1 = courant.Id;
                    int indice2 = voisin.Id;

                    double poids = this.MatriceAdjacente[indice1, indice2];

                    if (poids > 0 && poids < double.MaxValue && !visites.Contains(voisin))
                    {
                        double nouvelleDistance = distances[courant] + poids;
                        if (nouvelleDistance < distances[voisin])
                        {
                            distances[voisin] = nouvelleDistance;
                            precedents[voisin] = courant;
                        }
                    }
                }
            }

            List<Noeud> chemin = new List<Noeud>();
            Noeud? etape = arrivee;
            while (etape != null)
            {
                chemin.Insert(0, etape);
                etape = precedents[etape];
            }

            return chemin;
        }

        /// <summary>
        /// Permet d'afficher le chemin le plus court en faisant apparaitre les noms des stations avec le numéro de leur ligne
        /// </summary>
        /// <param name="chemin">lsite noeuds du parcours le plus court</param>
        public void AfficherChemin(List<Noeud> chemin)
        {
            Console.Write("Chemin le plus court : ");
            int compteur = 0;
            foreach(Noeud noeud in chemin)
            {
                Console.Write(this.Noeuds[noeud.Id].Nom +"(" + this.Noeuds[noeud.Id].Ligne +")");
                if (compteur<chemin.Count-1)
                {
                    Console.Write(" -> ");
                }
                compteur++;
            }
            Console.WriteLine();
        }

        /// <summary>
        /// calcule le temps du plus court chemin
        /// </summary>
        /// <param name="noeuds">lsite des noeuds du plus court chemin</param>
        /// <returns>un double qui correspond au temps en minutes arrondi au dixieme pres</returns>
        public double Temps_Plus_Court_Chemin(List<Noeud> noeuds)
        {
            double temps= 0;
            for(int i=0; i<noeuds.Count-1; i++)   
            {
                if(noeuds[i].Nom ==noeuds[i + 1].Nom)
                {
                    temps += 0.065;
                }
                else
                {
                    temps += Distance(noeuds[i], noeuds[i + 1])/25;
                }
            }
            return Math.Round(temps*60,1);//en min
        }

        public void Affichage_Chemin_Plus_Court(Noeud depart, Noeud arrivee)
        {
            List<Noeud> list = this.Dijkstra(depart, arrivee);
            AfficherChemin(list);
            Console.Write("\nDurée du trajet le plus court : ");
            this.Temps_Plus_Court_Chemin(list);
        }

        /// <summary>
        /// Cherche le plus court chemin en utilisant l'algo de BellemFord
        /// </summary>
        /// <param name="depart">station de depart</param>
        /// <param name="arrrivee">station darrivé</param>
        /// <returns></returns>
        /// <exception cref="Exception">en cas de cycle absorbant</exception>
        public List<Noeud> BellmanFord(Noeud depart, Noeud arrrivee)
        {
            int taille = this.Noeuds.Count;
            Dictionary<Noeud, double> distances = new Dictionary<Noeud, double>();
            Dictionary<Noeud, Noeud?> precedents = new Dictionary<Noeud, Noeud?>();

            foreach (var noeud in this.Noeuds.Values)
            {
                distances[noeud] = double.MaxValue;
                precedents[noeud] = null;
            }
            distances[depart] = 0;

            for (int i = 0; i < taille - 1; i++)
            {
                foreach (var noeud1 in this.Noeuds.Values)
                {
                    foreach (var noeud2 in this.Noeuds.Values)
                    {
                        double poids = this.MatriceAdjacente[noeud1.Id, noeud2.Id];

                        if (poids > 0 && poids < double.MaxValue && distances[noeud1] != double.MaxValue)
                        {
                            double newDist = distances[noeud1] + poids;
                            if (newDist < distances[noeud2])
                            {
                                distances[noeud2] = newDist;
                                precedents[noeud2] = noeud1;
                            }
                        }
                    }
                }
            }

            foreach (var noeud1 in this.Noeuds.Values)
            {
                foreach (var noeud2 in this.Noeuds.Values)
                {
                    double poids = this.MatriceAdjacente[noeud1.Id, noeud2.Id];
                    if (poids > 0 && poids < double.MaxValue && distances[noeud1] != double.MaxValue &&
                        distances[noeud1] + poids < distances[noeud2])
                    {
                        throw new Exception("Graph contains a negative-weight cycle");
                    }
                }
            }

            List<Noeud> chemin = new List<Noeud>();
            Noeud? n = arrrivee;
            while (n != null)
            {
                chemin.Insert(0, n);
                n = precedents[n];
            }

            return chemin;
        }

        /// <summary>
        /// Cherche le plus court chemin en utilisant l'algo de FloydWarshall
        /// </summary>
        /// <param name="depart">station de départ</param>
        /// <param name="arrivee">station d'arrrivé</param>
        /// <returns>liste des noeuds du chemin le plus court</returns>
        public List<Noeud> FloydWarshall(Noeud depart, Noeud arrivee)
        {
            int n = this.Noeuds.Count;
            double[,] distances = new double[n + 1, n + 1];
            int[,] suivants = new int[n + 1, n + 1];

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (i == j)
                    {
                        distances[i, j] = 0;
                        suivants[i, j] = j;
                    }
                    else if (this.MatriceAdjacente[i, j] != 0)
                    {
                        distances[i, j] = this.MatriceAdjacente[i, j];
                        suivants[i, j] = j;
                    }
                    else
                    {
                        distances[i, j] = double.MaxValue / 2; 
                        suivants[i, j] = -1;
                    }
                }
            }

            for (int k = 1; k <= n; k++)
            {
                for (int i = 1; i <= n; i++)
                {
                    for (int j = 1; j <= n; j++)
                    {
                        if (distances[i, k] + distances[k, j] < distances[i, j])
                        {
                            distances[i, j] = distances[i, k] + distances[k, j];
                            suivants[i, j] = suivants[i, k];
                        }
                    }
                }
            }

            int idDepart = depart.Id;
            int idArrivee = arrivee.Id;

            int iDepart = TrouverIndice(idDepart);
            int iArrivee = TrouverIndice(idArrivee);

            if (suivants[iDepart, iArrivee] == -1)
                return new List<Noeud>(); 

            List<Noeud> chemin = new List<Noeud>();
            int courant = iDepart;

            while (courant != iArrivee)
            {
                chemin.Add(this.Noeuds[(int)this.MatriceAdjacente[courant, 0]]);
                courant = suivants[courant, iArrivee];
            }
            chemin.Add(this.Noeuds[(int)this.MatriceAdjacente[iArrivee, 0]]);

            return chemin;
        }

        /// <summary>
        /// permet de retruver l'indice de la matrice correspondant à l'id dune station
        /// </summary>
        /// <param name="id">id de la station</param>
        /// <returns>position de la ligne dans la matrice</returns>
        private int TrouverIndice(int id)
        {
            for (int i = 1; i <= this.Noeuds.Count; i++)
            {
                if ((int)this.MatriceAdjacente[i, 0] == id)
                    return i;
            }
            return -1;
        }



    }
}


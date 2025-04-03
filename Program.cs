namespace Rendu2_PSI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Graph graphe = new Graph("métro.CSV");
            List<Noeud> list = graphe.Dijkstra(graphe.Noeuds[2], graphe.Noeuds[314]);
            //List<Noeud> list = graphe.BellmanFord(graphe.Noeuds[115], graphe.Noeuds[307]);
            //Console.WriteLine(graphe.Temps_Plus_Court_Chemin(list));
            //var (distances, predecesseurs) = graphe.FloydWarshall();
            //List<Noeud> chemin = graphe.ReconstructPath(graphe.Noeuds[115], graphe.Noeuds[307], graphe.Liste_Noeud_precedant(graphe.Noeuds), graphe.Noeuds);
            //List<Noeud> list = graphe.FloydWarshall(graphe.Noeuds[2], graphe.Noeuds[314]);
            //foreach (Noeud noeud in list)
            //{
            //    Console.WriteLine(graphe.Noeuds[noeud.Id].Nom + " ");
            //}
            //graphe.Affichage_MatriceAdjacente();
            graphe.AfficherChemin(list);
            //List<Lien> lien = graphe.Creation_Lien_Changements();
            //Console.WriteLine(lien[1].Initiale.Id + "," + lien[0].Terminale.Id);
            // Console.WriteLine(lien.Count);

        }

    }
}


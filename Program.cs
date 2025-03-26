namespace Rendu2_PSI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string lien = "métro.CSV";
            //Console.Write(File.ReadAllLines(lien).Length);
            Graph graphe = new Graph(lien);
            graphe.Affichage_MatriceAdjacente();
            Console.WriteLine("Press any key to exit");
        }
    }
}

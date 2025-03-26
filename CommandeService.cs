using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rendu2_PSI
{
    public class CommandeService
    {
        private string connectionString = "SERVER=127.0.0.1;PORT=3306;" +
                                          "DATABASE=LivInParis;" +
                                          "UID=root;PASSWORD=Granite666!";

        public void PassOrder(int[] platIds, int[] quantites)
        {
            int idUtilisateur = AuthService.CurrentUserId;
            if (idUtilisateur == -1)
            {
                Console.WriteLine("Utilisateur non trouvé.");
                return;
            }

            string stationClient = new AuthService().GetStationProche(idUtilisateur);
            string[] stations = new string[platIds.Length];
            string[] dates = new string[platIds.Length];

            for (int i = 0; i < platIds.Length; i++)
            {
                Console.WriteLine($"Souhaitez-vous être livré à votre station habituelle pour le plat {platIds[i]} ? (oui/non)");
                string rep = Console.ReadLine().ToLower();
                if (rep == "oui")
                    stations[i] = stationClient;
                else
                {
                    Console.WriteLine("Entrez la station de livraison pour ce plat :");
                    stations[i] = Console.ReadLine();
                }

                Console.WriteLine("Entrez la date de livraison pour le plat " + platIds[i] + " (format YYYY-MM-DD) :");
                dates[i] = Console.ReadLine();
            }

            decimal prixTotal = 0;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string queryCommande = "INSERT INTO Commande (Prix_Total, Payé, Id_Utilisateur, Date_Commande) " +
                                           "VALUES (@PrixTotal, @Payé, @IdUtilisateur, @DateCommande);";

                    MySqlCommand cmdCommande = new MySqlCommand(queryCommande, connection);
                    cmdCommande.Parameters.AddWithValue("@PrixTotal", prixTotal);
                    cmdCommande.Parameters.AddWithValue("@Payé", false);
                    cmdCommande.Parameters.AddWithValue("@IdUtilisateur", idUtilisateur);
                    cmdCommande.Parameters.AddWithValue("@DateCommande", DateTime.Now.Date);

                    int resultCommande = cmdCommande.ExecuteNonQuery();

                    if (resultCommande > 0)
                    {
                        cmdCommande.CommandText = "SELECT LAST_INSERT_ID();";
                        int idCommande = Convert.ToInt32(cmdCommande.ExecuteScalar());

                        for (int i = 0; i < platIds.Length; i++)
                        {
                            string queryPrixPlat = "SELECT Prix FROM Plat WHERE Id_Plat = @IdPlat";
                            MySqlCommand cmdPrixPlat = new MySqlCommand(queryPrixPlat, connection);
                            cmdPrixPlat.Parameters.AddWithValue("@IdPlat", platIds[i]);

                            decimal prixPlat = (decimal)cmdPrixPlat.ExecuteScalar();
                            prixTotal += prixPlat * quantites[i];

                            string queryLigne = "INSERT INTO Ligne_de_commande " +
                                "(Quantité, Date_Livraison, Station_Livraison, Prix, Id_Commande, Id_Plat) " +
                                "VALUES (@Quantité, @DateLivraison, @StationLivraison, @Prix, @IdCommande, @IdPlat);";

                            MySqlCommand cmdLigne = new MySqlCommand(queryLigne, connection);
                            cmdLigne.Parameters.AddWithValue("@Quantité", quantites[i]);
                            cmdLigne.Parameters.AddWithValue("@DateLivraison", dates[i]);
                            cmdLigne.Parameters.AddWithValue("@StationLivraison", stations[i]);
                            cmdLigne.Parameters.AddWithValue("@Prix", prixPlat);
                            cmdLigne.Parameters.AddWithValue("@IdCommande", idCommande);
                            cmdLigne.Parameters.AddWithValue("@IdPlat", platIds[i]);
                            cmdLigne.ExecuteNonQuery();

                            string queryStationCuisinier = "SELECT u.Station_Proche FROM Utilisateur u " +
                                "JOIN Plat p ON u.Id_Utilisateur = p.Id_Utilisateur WHERE p.Id_Plat = @IdPlat";
                            MySqlCommand cmdStation = new MySqlCommand(queryStationCuisinier, connection);
                            cmdStation.Parameters.AddWithValue("@IdPlat", platIds[i]);
                            string stationCuisinier = cmdStation.ExecuteScalar()?.ToString();

                            Console.WriteLine($"Livraison du plat {platIds[i]} : {stationCuisinier} ---> {stations[i]}");
                        }

                        string updateCommandePrix = "UPDATE Commande SET Prix_Total = @PrixTotal WHERE Id_Commande = @IdCommande";
                        MySqlCommand cmdUpdatePrix = new MySqlCommand(updateCommandePrix, connection);
                        cmdUpdatePrix.Parameters.AddWithValue("@PrixTotal", prixTotal);
                        cmdUpdatePrix.Parameters.AddWithValue("@IdCommande", idCommande);
                        cmdUpdatePrix.ExecuteNonQuery();

                        Console.WriteLine($"Le prix total de la commande est de {prixTotal} EUR.");
                        Console.WriteLine("Commande passée avec succès !");
                    }
                    else
                    {
                        Console.WriteLine("Erreur lors de la commande.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur lors du passage de la commande : " + ex.Message);
                }
            }
        }
    }
}

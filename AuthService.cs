using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rendu2_PSI
{
    public class AuthService
    {
        private string connectionString = "SERVER=127.0.0.1;PORT=3306;" +
                                          "DATABASE=LivInParis;" +
                                          "UID=root;PASSWORD=Granite666!";

        public static int CurrentUserId { get; private set; }
        public static string CurrentUserRole { get; private set; }

        public bool Register(string nom, string prénom, string email, string motDePasse, string adresse, string stationProche, bool estCuisinier, string specialite)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "INSERT INTO Utilisateur (Nom, Prénom, Email, Mot_de_passe, Adresse, Station_Proche) " +
                                   "VALUES (@Nom, @Prénom, @Email, @MotDePasse, @Adresse, @StationProche)";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Nom", nom);
                    cmd.Parameters.AddWithValue("@Prénom", prénom);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@MotDePasse", motDePasse);
                    cmd.Parameters.AddWithValue("@Adresse", adresse);
                    cmd.Parameters.AddWithValue("@StationProche", stationProche);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        string queryGetId = "SELECT Id_Utilisateur FROM Utilisateur WHERE Email = @Email";
                        MySqlCommand cmdGetId = new MySqlCommand(queryGetId, connection);
                        cmdGetId.Parameters.AddWithValue("@Email", email);
                        CurrentUserId = Convert.ToInt32(cmdGetId.ExecuteScalar());

                        string queryClient = "INSERT INTO Client (Id_Utilisateur, Particulier) VALUES (@Id, @Particulier)";
                        MySqlCommand cmdClient = new MySqlCommand(queryClient, connection);
                        cmdClient.Parameters.AddWithValue("@Id", CurrentUserId);
                        cmdClient.Parameters.AddWithValue("@Particulier", true);
                        cmdClient.ExecuteNonQuery();

                        if (estCuisinier && !string.IsNullOrEmpty(specialite))
                        {
                            string queryCuisinier = "INSERT INTO Cuisinier (Id_Utilisateur, Spécialité) VALUES (@IdUtilisateur, @Spécialité)";
                            MySqlCommand cmdCuisinier = new MySqlCommand(queryCuisinier, connection);
                            cmdCuisinier.Parameters.AddWithValue("@IdUtilisateur", CurrentUserId);
                            cmdCuisinier.Parameters.AddWithValue("@Spécialité", specialite);
                            cmdCuisinier.ExecuteNonQuery();
                        }

                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur lors de l'inscription : " + ex.Message);
                    return false;
                }
            }
        }

        public bool Login(string email, string motDePasse)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM Utilisateur WHERE Email = @Email AND Mot_de_passe = @MotDePasse";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@MotDePasse", motDePasse);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    if (count > 0)
                    {
                        // Récupération de l'ID
                        string queryGetId = "SELECT Id_Utilisateur FROM Utilisateur WHERE Email = @Email";
                        MySqlCommand cmdGetId = new MySqlCommand(queryGetId, connection);
                        cmdGetId.Parameters.AddWithValue("@Email", email);
                        CurrentUserId = Convert.ToInt32(cmdGetId.ExecuteScalar());

                        // Récupération du rôle
                        string queryRole = "SELECT Role FROM Utilisateur WHERE Email = @Email";
                        MySqlCommand cmdRole = new MySqlCommand(queryRole, connection);
                        cmdRole.Parameters.AddWithValue("@Email", email);
                        CurrentUserRole = cmdRole.ExecuteScalar()?.ToString();

                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur lors de la connexion : " + ex.Message);
                    return false;
                }
            }
        }

        public bool IsCuisinier(string email)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Cuisinier c JOIN Utilisateur u ON c.Id_Utilisateur = u.Id_Utilisateur WHERE u.Email = @Email";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Email", email);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur lors de la vérification du rôle : " + ex.Message);
                    return false;
                }
            }
        }

        public string GetStationProche(int idUtilisateur)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Station_Proche FROM Utilisateur WHERE Id_Utilisateur = @IdUtilisateur";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@IdUtilisateur", idUtilisateur);
                    return cmd.ExecuteScalar()?.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur lors de la récupération de la station : " + ex.Message);
                    return null;
                }
            }
        }
    }

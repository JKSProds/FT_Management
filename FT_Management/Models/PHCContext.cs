using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace FT_Management.Models
{
    public class PHCContext
    {
        private string ConnectionString { get; set; }
        public PHCContext(string connectionString)
        {
            this.ConnectionString = connectionString;
            SqlConnection cnn;

            try
            {
                cnn = new SqlConnection(connectionString);
                Console.WriteLine("Connectado á Base de Dados PHC com sucesso!");
            }
            catch
            {
                Console.WriteLine("Não foi possivel conectar á BD! Verifique se a base de dados existe e o IP está correto. ");
            }
        }

        public List<Produto> ObterProdutos(string referencia)
        {
            List<Produto> LstProdutos = new List<Produto>();

            SqlConnection conn = new SqlConnection(ConnectionString);

            conn.Open();

            SqlCommand command = new SqlCommand("SELECT * FROM sa where armazem=32", conn);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine(reader["ref"]);
                }
            }

            conn.Close();

            return LstProdutos;
        }

    }
}

using System;
using MySql.Simple;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class FT_ManagementContext
    {

        public string ConnectionString { get; set; }

        public FT_ManagementContext(string connectionString)
        {
            this.ConnectionString = connectionString;

            try
            {
                Database db = ConnectionString;
            }
            catch
            {
                throw new Exception("Não foi possivel conectar á BD! Verifique se a base de dados existe e o IP está correto.");
            }
        }

    }
}

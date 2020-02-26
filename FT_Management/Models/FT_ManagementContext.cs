using System;
using MySql.Simple;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.IO;

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

        public List<Produto> ObterListaProdutos()
        {
            List<Produto> LstProdutos = new List<Produto>();

            Database db = ConnectionString;

            using (var result = db.Query("SELECT * FROM dat_produtos;"))
            {
                while (result.Read())
                {
                    LstProdutos.Add(new Produto()
                    {
                        Ref_Produto = result["ref_produto"],
                        Designacao_Produto = result["designacao_produto"],
                        Stock_Fisico = result["stock_fisico"],
                        Stock_PHC = result["stock_phc"],
                        Pos_Stock = result["pos_stock"],
                        Obs_Produto = result["obs"]
                    });
                }
            }

            return LstProdutos;
        }

        public void CarregarFicheiroDB(string FilePath)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(FilePath)))
            {
                //ExcelWorksheet workSheet = package.Workbook.Worksheets["Table1"];
                ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
                int totalRows = workSheet.Dimension.Rows;

                var LstProdutos = new List<Produto>();

                for (int i = 1; i <= totalRows; i++)
                {
                    double stock_Rececao = 0;
                    double.TryParse(workSheet.Cells[i, 3].Value.ToString(), out double stock_PHC);
                   if (workSheet.Dimension.End.Column == 4) {
                    double.TryParse(workSheet.Cells[i, 4].Value.ToString(), out stock_Rececao);
                    }

                    LstProdutos.Add(new Produto
                    {
                        Ref_Produto = workSheet.Cells[i, 1].Value.ToString().Replace(" ", ""),
                        Designacao_Produto = workSheet.Cells[i, 2].Value.ToString().Trim(),
                        Stock_PHC = stock_PHC + stock_Rececao,
                       Stock_Fisico = 0.0,
                       Pos_Stock = "",
                       Obs_Produto = ""
                    });
                }

                atualizarListaArtigos(LstProdutos);
            }
        }

        public void atualizarListaArtigos(List<Produto> LstProdutos)
        {
            string sql = "INSERT INTO dat_produtos (ref_produto, designacao_produto, stock_phc, stock_fisico, pos_stock, obs) VALUES ";

            foreach (var item in LstProdutos)
            {
                sql += ("('"+item.Ref_Produto+"', '"+item.Designacao_Produto+"', '"+item.Stock_PHC.ToString().Replace(",", ".") +"', '"+item.Stock_Fisico.ToString().Replace(",", ".") + "', '" +item.Pos_Stock+"', '"+item.Obs_Produto+"'), \r\n");
            }
            sql = sql.Remove(sql.Count() - 4); 
            sql += " ON DUPLICATE KEY UPDATE designacao_produto = VALUES(designacao_produto), stock_phc = VALUES(stock_phc);";

            Database db = ConnectionString;

            db.Execute(sql);
        }

    }
}

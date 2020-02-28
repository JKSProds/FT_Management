using System;
using MySql.Simple;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;

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

        public Produto ObterProduto(string referencia)
        {
            Produto produto = new Produto();
            Database db = ConnectionString;

            var result = db.Query("SELECT * FROM dat_produtos Where ref_produto = '" + referencia + "';");

            while (result.Read()) {

                produto = new Produto()
                {
                    Ref_Produto = result["ref_produto"],
                    Designacao_Produto = result["designacao_produto"],
                    Stock_Fisico = result["stock_fisico"],
                    Stock_PHC = result["stock_phc"],
                    Pos_Stock = result["pos_stock"],
                    Obs_Produto = result["obs"]
                };
            }
            return produto;
        }

        public List<Produto> ObterListaProdutos(string referencia, string desig)
        {
            List<Produto> LstProdutos = new List<Produto>();

            Database db = ConnectionString;

            using (var result = db.Query("SELECT * FROM dat_produtos Where ref_produto like '%"+ referencia + "%' AND designacao_produto like '%"+desig+"%';"))
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
                    string ref_prod = workSheet.Cells[i, 1].Value.ToString().Replace(" ", "");
                    string desig = workSheet.Cells[i, 2].Value.ToString().Trim();
                    double stock_Rececao = 0;
                    double.TryParse(workSheet.Cells[i, 3].Value.ToString(), out double stock_PHC);
                   if (workSheet.Dimension.End.Column == 4) {
                    double.TryParse(workSheet.Cells[i, 4].Value.ToString(), out stock_Rececao);
                    }

                   if (LstProdutos.Where(p => p.Ref_Produto == ref_prod).Count() == 0)
                    {
                        LstProdutos.Add(new Produto
                        {
                            Ref_Produto = ref_prod,
                            Designacao_Produto = desig,
                            Stock_PHC = stock_PHC + stock_Rececao,
                            Stock_Fisico = 0.0,
                            Pos_Stock = "",
                            Obs_Produto = ""
                        });

                    } else
                    {
                        LstProdutos.Where(p => p.Ref_Produto == ref_prod).First().Stock_PHC += stock_PHC;
                        LstProdutos.Where(p => p.Ref_Produto == ref_prod).First().Designacao_Produto = desig;
                    }
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

        public Bitmap desenharEtiqueta80x50 (Produto produto)
        {
          
            Bitmap bm = new Bitmap(300, 188);
            Font fontHeader = new Font("Tahoma", 18, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 8, FontStyle.Regular);

            StringFormat format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;

            int x = 10;
            int y = 0;

            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);

                x = 30;
                Image img = System.Drawing.Image.FromFile(@"wwwroot\img\ft_logo.png", true);
                gr.DrawImage(img, x, y, 85, 50);

                y += 10;
                gr.DrawString("Food-Tech", fontHeader, Brushes.Black, x + 85 + 10, y);
          
                x = 10;
                y += 40;
                gr.DrawString(produto.Designacao_Produto, fontBody, Brushes.Black, new Rectangle(x, y, 280, 45), format);


                y += 50;
                gr.DrawString(produto.Ref_Produto, fontBody, Brushes.Black, x, y);


                y += 70;
                gr.DrawString("geral@food-tech.pt", fontBody, Brushes.Black, new Rectangle(x, y, 280, 20), format);

            }

            return bm;
        }

    }
}

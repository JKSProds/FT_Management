using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class TrelloConector
    {
        private string API_KEY;
        private string TOKEN;

        public TrelloConector(string _API_KEY, string _TOKEN)
        {
            API_KEY = _API_KEY;
            TOKEN = _TOKEN;
        }

        public List<TrelloQuadros> ObterQuadros()
        {
            List<TrelloQuadros> trelloQuadros = new List<TrelloQuadros>();

            foreach (var quadro in GetTrelloJson("https://api.trello.com/1/members/me/boards?fields=name,dateLastActivity&key=" + API_KEY + "&token=" + TOKEN + ""))
            {
                trelloQuadros.Add(new TrelloQuadros
                {
                    IdQuadro = quadro.id,
                    NomeQuadro = quadro.name,
                    DataUltimaAtividade = quadro.dateLastActivity
                });
            }

            return trelloQuadros;
        }
        public List<TrelloListas> ObterListas(string IdQuadro)
        {
            List<TrelloListas> trelloListas = new List<TrelloListas>();

            foreach (var lista in GetTrelloJson("https://api.trello.com/1/boards/" + IdQuadro + "/lists?key=" + API_KEY + "&token=" + TOKEN + ""))
            {
                trelloListas.Add(new TrelloListas
                {
                    IdLista = lista.id,
                    NomeLista = lista.name,
                    ListaCartoes = new List<TrelloCartoes>(),
                    IdQuadro = lista.idBoard
                });
            }

            return trelloListas;
        }
        public List<TrelloCartoes> ObterCartoes(string IdLista)
        {
            List<TrelloCartoes> trelloCartoes = new List<TrelloCartoes>();

            foreach (var cartao in GetTrelloJson("https://api.trello.com/1/lists/" + IdLista + "/cards?key=" + API_KEY + "&token=" + TOKEN + ""))
            {
                trelloCartoes.Add(new TrelloCartoes
                {
                    IdCartao = cartao.id,
                    NomeCartao = cartao.name,
                    DescricaoCartao = cartao.desc,
                    CorLabel = cartao.labels.Count > 0 ? cartao.labels[0].color : "grey",
                    IdQuadro = cartao.idBoard,
                    IdLista = cartao.idList
                });
            }


            return trelloCartoes;
        }
        public TrelloCartoes ObterCartao(string IdCartao)
        {

            TrelloCartoes cartao = new TrelloCartoes();
            dynamic jsonCartao = GetTrelloJson("https://api.trello.com/1/cards/" + IdCartao + "?key=" + API_KEY + "&token=" + TOKEN + "");
            if (jsonCartao != null)
            {
                cartao = new TrelloCartoes
                {
                    IdCartao = jsonCartao.id,
                    NomeCartao = jsonCartao.name,
                    DescricaoCartao = jsonCartao.desc.ToString().Replace("\n", Environment.NewLine),
                    CorLabel = jsonCartao.labels.Count > 0 ? jsonCartao.labels[0].color : "",
                    IdQuadro = jsonCartao.idBoard,
                    IdLista = jsonCartao.idList
                };
                //Console.WriteLine(jsonCartao.desc);
            }
            return cartao;
        }
        public List<TrelloComentarios> ObterComentarios(string IdCartao)
        {

            List<TrelloComentarios> Comentarios = new List<TrelloComentarios>();
            foreach (var comentario in GetTrelloJson("https://api.trello.com/1/cards/" + IdCartao + "/actions?key=" + API_KEY + "&token=" + TOKEN + ""))
            {
                if (comentario.type == "commentCard")
                {
                    Comentarios.Add(new TrelloComentarios
                    {
                        IdComentario = comentario.id,
                        IdCartao = comentario.data.card.id,
                        Comentario = comentario.data.text.ToString().Replace("\n", Environment.NewLine),
                        DataComentario = comentario.date,
                        Utilizador = comentario.memberCreator.fullName
                    });
                }
            }

            return Comentarios;
        }
        public List<TrelloAnexos> ObterAnexos(string IdCartao)
        {
            List<TrelloAnexos> Anexos = new List<TrelloAnexos>();
            foreach (var anexo in GetTrelloJson("https://api.trello.com/1/cards/" + IdCartao + "/attachments?key=" + API_KEY + "&token=" + TOKEN + ""))
            {
                    Anexos.Add(new TrelloAnexos
                    {
                        id = anexo.id,
                        name = anexo.name
                    });
            }

            return Anexos;
        }
        public List<TrelloLabels> ObterLabels(string IdQuadro)
        {
            List<TrelloLabels> Labels = new List<TrelloLabels>();
            foreach (var label in GetTrelloJson("https://api.trello.com/1/boards/"+IdQuadro+"/labels?key="+API_KEY+"&token="+TOKEN+""))
            {
                Labels.Add(new TrelloLabels
                {
                    IdLabel = label.id,
                    Nome = label.name,
                    Color = label.color
                });
            }

            return Labels;
        }
        public void ApagarLabel(string IdLabel, string IdCartao)
        {
            DeleteTrello("https://api.trello.com/1/cards/" + IdCartao + "/idLabels/" + IdLabel + "?key=" + API_KEY + "&token=" + TOKEN + "");
        }
        public void NovoComentario(string IdCartao, string Comentario)
        {
            if (ObterComentarios(IdCartao).Where(c => c.Comentario == Comentario.Replace("\r\n", string.Empty)).Count() == 0) PostTrelloJson("https://api.trello.com/1/cards/" + IdCartao + "/actions/comments?key=" + API_KEY + "&token=" + TOKEN + "&text=" + Comentario + "", "");
        }
        public void NovoAnexo(string IdCartao, byte[] documento, string NomeDocumento)
        {
            if (ObterAnexos(IdCartao).Where(a => a.name == NomeDocumento).Count() > 0) ApagarAnexo(ObterAnexos(IdCartao).Where(a => a.name == NomeDocumento).First().id, IdCartao); 

                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("id", IdCartao);
                nvc.Add("name", NomeDocumento);
                HttpUploadFile("https://api.trello.com/1/cards/"+IdCartao+"/attachments?key="+API_KEY+"&token="+TOKEN+"",
                     documento, "file", "application/pdf", nvc, NomeDocumento);

        }
        public void NovaLabel(string IdCartao, string Cor)
        {
            foreach (var label in ObterLabels(ObterCartao(IdCartao).IdQuadro))
            {
                ApagarLabel(label.IdLabel, IdCartao);
            }
            var json = JsonConvert.SerializeObject(new { value = ObterLabels(ObterCartao(IdCartao).IdQuadro).Where(l => l.Color == Cor).First().IdLabel });
            PostTrelloJson("https://api.trello.com/1/cards/"+IdCartao+"/idLabels?key="+API_KEY+"&token="+TOKEN+"", json);

        }
        public void ApagarAnexo(string IdAnexo, string IdCartao)
        {
            DeleteTrello("https://api.trello.com/1/cards/" + IdCartao + "/attachments/" + IdAnexo + "?key=" + API_KEY + "&token=" + TOKEN + "");
        }

        public static void HttpUploadFile(string url, byte[] file, string paramName, string contentType, NameValueCollection nvc, string NomeDocumento)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, NomeDocumento, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

                rs.Write(file);

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
            }
            catch (Exception)
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }
        private dynamic GetTrelloJson(string url)
        {
            string urlAddress = url;
            dynamic deserializedProduct = JsonConvert.DeserializeObject("");

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;
                    if (response.CharacterSet == null)
                        readStream = new StreamReader(receiveStream);
                    else
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                    string data = readStream.ReadToEnd();
                    deserializedProduct = JsonConvert.DeserializeObject(data);
                    response.Close();
                    readStream.Close();
                }

            }
            catch (Exception)
            {

                Console.WriteLine("Erro 400 Trello");
            }
            return deserializedProduct;
        }
        private void PostTrelloJson(string url, string json)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";


                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(json);
                }


                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Erro 400 Trello");
            }
        }
        private void DeleteTrello(string url)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "DELETE";

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Erro 400 Trello");
            }
        }

        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }

            return "";
        }

    }

    public class TrelloQuadros
    {
        public string IdQuadro { get; set; }
        public string NomeQuadro { get; set; }
        public DateTime DataUltimaAtividade { get; set; }
    }

    public class TrelloListas
    {
        public string IdLista { get; set; }
        public string NomeLista { get; set; }
        public List<TrelloCartoes> ListaCartoes { get; set; }
        public string IdQuadro { get; set; }

    }
    public class TrelloCartoes
    {
        public string IdCartao { get; set; }
        public string NomeCartao { get; set; }
        public string DescricaoCartao { get; set; }
        public string CorLabel { get; set; }
        public List<FolhaObra> FolhasObra { get; set; }
        public string IdQuadro { get; set; }
        public string IdLista { get; set; }
        public List<TrelloComentarios> Comentarios { get; set; }
    }

    public class TrelloComentarios 
    {
        public string IdComentario { get; set; }
        public string Comentario { get; set; }
        public DateTime DataComentario { get; set; }
        public string Utilizador { get; set; }
        public string IdCartao { get; set; }
    }

    public class TrelloAnexos
    {
        public string id { get; set; }
        public byte[] file { get; set; }
        public DateTime date { get; set; }
        public string name { get; set; }
        public string mimeType { get; set; }
    }
   
    public class TrelloLabels
    {
        public string IdLabel { get; set; }
        public string Nome { get; set; }
        public string Color { get; set; }
    }
}
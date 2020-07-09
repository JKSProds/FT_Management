using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class TrelloConector
    {
        private const string API_KEY = "d2c48cf9456913b82f7890d96892e760";
        private const string TOKEN = "fdc25865a518d1f2cefbafb2a13fcf2ced7a37caa872fe02451d1013e81ab9f3";

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
        public void NovoComentario(string IdCartao, string Comentario)
        {
            if (ObterComentarios(IdCartao).Where(c => c.Comentario == Comentario).Count() == 0) PostTrelloJson("https://api.trello.com/1/cards/" + IdCartao + "/actions/comments?key=" + API_KEY + "&token=" + TOKEN + "&text=" + Comentario + "");
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
        private void PostTrelloJson(string url)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

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

}

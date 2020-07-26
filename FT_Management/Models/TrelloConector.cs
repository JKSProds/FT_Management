using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
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
        private static string API_KEY;
        private static string TOKEN;

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
                cartao.Anexos = ObterAnexos(cartao.IdCartao);
                cartao.Comentarios = ObterComentarios(cartao.IdCartao);
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
                        Comentario = comentario.data.text.ToString(),
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
                        Id = anexo.id,
                        Name = anexo.name,
                        MimeType = anexo.mimeType,
                        Date = anexo.date,
                        IdCartao = IdCartao
                    });
            }

            return Anexos;
        }
        public TrelloAnexos ObterAnexo(string IdAnexo, string IdCartao)
        {
            TrelloAnexos trelloAnexo;
            var anexo = GetTrelloJson("https://api.trello.com/1/cards/" + IdCartao + "/attachments/" + IdAnexo + "?key=" + API_KEY + "&token=" + TOKEN + "");
            
                WebClient myWebClient = new WebClient();
                trelloAnexo = new TrelloAnexos
                {
                    Id = anexo.id,
                    Name = anexo.name,
                    File = myWebClient.DownloadData(anexo.url.ToString()),
                    MimeType = anexo.mimeType,
                    Date = anexo.date,
                    IdCartao = IdCartao
                };
   

            return trelloAnexo;
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
            if (ObterComentarios(IdCartao).Where(c => c.Comentario == Comentario).Count() == 0) PostTrelloJson("https://api.trello.com/1/cards/" + IdCartao + "/actions/comments?key=" + API_KEY + "&token=" + TOKEN + "&text=" + Comentario + "", "");
        
        }
        public void NovoAnexo(TrelloAnexos Anexo)
        {
            if (ObterAnexos(Anexo.Id).Where(a => a.Name == Anexo.Name).Count() > 0) ApagarAnexo(ObterAnexos(Anexo.Id).Where(a => a.Name == Anexo.Name).First().Id, Anexo.Id);

            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("id", Anexo.Id);
            nvc.Add("name", Anexo.Name);
            HttpUploadFile("https://api.trello.com/1/cards/"+Anexo.Id+"/attachments?key="+API_KEY+"&token="+TOKEN+"",
                     Anexo.File, "file", Anexo.MimeType, nvc, Anexo.Name);

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
                using var streamReader = new StreamReader(httpResponse.GetResponseStream());
                var result = streamReader.ReadToEnd();
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
                using var streamReader = new StreamReader(httpResponse.GetResponseStream());
                var result = streamReader.ReadToEnd();
            }
            catch (Exception)
            {
                Console.WriteLine("Erro 400 Trello");
            }
        }

        public static string GetBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                if (Start < 0 || End < 0) return "";
                return strSource[Start..End];
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
        public List<TrelloAnexos> Anexos { get; set; }
        public DateTime DataCriacao { get { return DefDataCriacao(); } }

        public DateTime DefDataCriacao() {
            string FirstDigits = IdCartao.Substring(0,8);
            int decValue = Convert.ToInt32(FirstDigits, 16);

            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(decValue);
        }
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
        public string Id { get; set; }
        public string IdCartao { get; set; }
        public byte[] File { get; set; }
        [Display(Name = "Data")]
        public DateTime Date { get; set; }
        [Display(Name = "Ficheiro")]
        public string Name { get; set; }
        public string MimeType { get; set; }

        public IDictionary<string, string> dict = new Dictionary<string, string>()
                        {
                        { ".323", "text/h323" },
                        { ".3g2", "video/3gpp2" },
                        { ".3gp2", "video/3gpp2" },
                        { ".3gp", "video/3gpp" },
                        { ".3gpp", "video/3gpp" },
                        { ".aac", "audio/aac" },
                        { ".aaf", "application/octet-stream" },
                        { ".aca", "application/octet-stream" },
                        { ".accdb", "application/msaccess" },
                        { ".accde", "application/msaccess" },
                        { ".accdt", "application/msaccess" },
                        { ".acx", "application/internet-property-stream" },
                        { ".adt", "audio/vnd.dlna.adts" },
                        { ".adts", "audio/vnd.dlna.adts" },
                        { ".afm", "application/octet-stream" },
                        { ".ai", "application/postscript" },
                        { ".aif", "audio/x-aiff" },
                        { ".aifc", "audio/aiff" },
                        { ".aiff", "audio/aiff" },
                        { ".appcache", "text/cache-manifest" },
                        { ".application", "application/x-ms-application" },
                        { ".art", "image/x-jg" },
                        { ".asd", "application/octet-stream" },
                        { ".asf", "video/x-ms-asf" },
                        { ".asi", "application/octet-stream" },
                        { ".asm", "text/plain" },
                        { ".asr", "video/x-ms-asf" },
                        { ".asx", "video/x-ms-asf" },
                        { ".atom", "application/atom+xml" },
                        { ".au", "audio/basic" },
                        { ".avi", "video/x-msvideo" },
                        { ".axs", "application/olescript" },
                        { ".bas", "text/plain" },
                        { ".bcpio", "application/x-bcpio" },
                        { ".bin", "application/octet-stream" },
                        { ".bmp", "image/bmp" },
                        { ".c", "text/plain" },
                        { ".cab", "application/vnd.ms-cab-compressed" },
                        { ".calx", "application/vnd.ms-office.calx" },
                        { ".cat", "application/vnd.ms-pki.seccat" },
                        { ".cdf", "application/x-cdf" },
                        { ".chm", "application/octet-stream" },
                        { ".class", "application/x-java-applet" },
                        { ".clp", "application/x-msclip" },
                        { ".cmx", "image/x-cmx" },
                        { ".cnf", "text/plain" },
                        { ".cod", "image/cis-cod" },
                        { ".cpio", "application/x-cpio" },
                        { ".cpp", "text/plain" },
                        { ".crd", "application/x-mscardfile" },
                        { ".crl", "application/pkix-crl" },
                        { ".crt", "application/x-x509-ca-cert" },
                        { ".csh", "application/x-csh" },
                        { ".css", "text/css" },
                        { ".csv", "application/octet-stream" },
                        { ".cur", "application/octet-stream" },
                        { ".dcr", "application/x-director" },
                        { ".deploy", "application/octet-stream" },
                        { ".der", "application/x-x509-ca-cert" },
                        { ".dib", "image/bmp" },
                        { ".dir", "application/x-director" },
                        { ".disco", "text/xml" },
                        { ".dlm", "text/dlm" },
                        { ".doc", "application/msword" },
                        { ".docm", "application/vnd.ms-word.document.macroEnabled.12" },
                        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                        { ".dot", "application/msword" },
                        { ".dotm", "application/vnd.ms-word.template.macroEnabled.12" },
                        { ".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" },
                        { ".dsp", "application/octet-stream" },
                        { ".dtd", "text/xml" },
                        { ".dvi", "application/x-dvi" },
                        { ".dvr-ms", "video/x-ms-dvr" },
                        { ".dwf", "drawing/x-dwf" },
                        { ".dwp", "application/octet-stream" },
                        { ".dxr", "application/x-director" },
                        { ".eml", "message/rfc822" },
                        { ".emz", "application/octet-stream" },
                        { ".eot", "application/vnd.ms-fontobject" },
                        { ".eps", "application/postscript" },
                        { ".etx", "text/x-setext" },
                        { ".evy", "application/envoy" },
                        { ".fdf", "application/vnd.fdf" },
                        { ".fif", "application/fractals" },
                        { ".fla", "application/octet-stream" },
                        { ".flr", "x-world/x-vrml" },
                        { ".flv", "video/x-flv" },
                        { ".gif", "image/gif" },
                        { ".gtar", "application/x-gtar" },
                        { ".gz", "application/x-gzip" },
                        { ".h", "text/plain" },
                        { ".hdf", "application/x-hdf" },
                        { ".hdml", "text/x-hdml" },
                        { ".hhc", "application/x-oleobject" },
                        { ".hhk", "application/octet-stream" },
                        { ".hhp", "application/octet-stream" },
                        { ".hlp", "application/winhlp" },
                        { ".hqx", "application/mac-binhex40" },
                        { ".hta", "application/hta" },
                        { ".htc", "text/x-component" },
                        { ".htm", "text/html" },
                        { ".html", "text/html" },
                        { ".htt", "text/webviewhtml" },
                        { ".hxt", "text/html" },
                        { ".ical", "text/calendar" },
                        { ".icalendar", "text/calendar" },
                        { ".ico", "image/x-icon" },
                        { ".ics", "text/calendar" },
                        { ".ief", "image/ief" },
                        { ".ifb", "text/calendar" },
                        { ".iii", "application/x-iphone" },
                        { ".inf", "application/octet-stream" },
                        { ".ins", "application/x-internet-signup" },
                        { ".isp", "application/x-internet-signup" },
                        { ".IVF", "video/x-ivf" },
                        { ".jar", "application/java-archive" },
                        { ".java", "application/octet-stream" },
                        { ".jck", "application/liquidmotion" },
                        { ".jcz", "application/liquidmotion" },
                        { ".jfif", "image/pjpeg" },
                        { ".jpb", "application/octet-stream" },
                        { ".jpe", "image/jpeg" },
                        { ".jpeg", "image/jpeg" },
                        { ".jpg", "image/jpeg" },
                        { ".js", "application/javascript" },
                        { ".json", "application/json" },
                        { ".jsx", "text/jscript" },
                        { ".latex", "application/x-latex" },
                        { ".lit", "application/x-ms-reader" },
                        { ".lpk", "application/octet-stream" },
                        { ".lsf", "video/x-la-asf" },
                        { ".lsx", "video/x-la-asf" },
                        { ".lzh", "application/octet-stream" },
                        { ".m13", "application/x-msmediaview" },
                        { ".m14", "application/x-msmediaview" },
                        { ".m1v", "video/mpeg" },
                        { ".m2ts", "video/vnd.dlna.mpeg-tts" },
                        { ".m3u", "audio/x-mpegurl" },
                        { ".m4a", "audio/mp4" },
                        { ".m4v", "video/mp4" },
                        { ".man", "application/x-troff-man" },
                        { ".manifest", "application/x-ms-manifest" },
                        { ".map", "text/plain" },
                        { ".markdown", "text/markdown" },
                        { ".md", "text/markdown" },
                        { ".mdb", "application/x-msaccess" },
                        { ".mdp", "application/octet-stream" },
                        { ".me", "application/x-troff-me" },
                        { ".mht", "message/rfc822" },
                        { ".mhtml", "message/rfc822" },
                        { ".mid", "audio/mid" },
                        { ".midi", "audio/mid" },
                        { ".mix", "application/octet-stream" },
                        { ".mmf", "application/x-smaf" },
                        { ".mno", "text/xml" },
                        { ".mny", "application/x-msmoney" },
                        { ".mov", "video/quicktime" },
                        { ".movie", "video/x-sgi-movie" },
                        { ".mp2", "video/mpeg" },
                        { ".mp3", "audio/mpeg" },
                        { ".mp4", "video/mp4" },
                        { ".mp4v", "video/mp4" },
                        { ".mpa", "video/mpeg" },
                        { ".mpe", "video/mpeg" },
                        { ".mpeg", "video/mpeg" },
                        { ".mpg", "video/mpeg" },
                        { ".mpp", "application/vnd.ms-project" },
                        { ".mpv2", "video/mpeg" },
                        { ".ms", "application/x-troff-ms" },
                        { ".msi", "application/octet-stream" },
                        { ".mso", "application/octet-stream" },
                        { ".mvb", "application/x-msmediaview" },
                        { ".mvc", "application/x-miva-compiled" },
                        { ".nc", "application/x-netcdf" },
                        { ".nsc", "video/x-ms-asf" },
                        { ".nws", "message/rfc822" },
                        { ".ocx", "application/octet-stream" },
                        { ".oda", "application/oda" },
                        { ".odc", "text/x-ms-odc" },
                        { ".ods", "application/oleobject" },
                        { ".oga", "audio/ogg" },
                        { ".ogg", "video/ogg" },
                        { ".ogv", "video/ogg" },
                        { ".ogx", "application/ogg" },
                        { ".one", "application/onenote" },
                        { ".onea", "application/onenote" },
                        { ".onetoc", "application/onenote" },
                        { ".onetoc2", "application/onenote" },
                        { ".onetmp", "application/onenote" },
                        { ".onepkg", "application/onenote" },
                        { ".osdx", "application/opensearchdescription+xml" },
                        { ".otf", "font/otf" },
                        { ".p10", "application/pkcs10" },
                        { ".p12", "application/x-pkcs12" },
                        { ".p7b", "application/x-pkcs7-certificates" },
                        { ".p7c", "application/pkcs7-mime" },
                        { ".p7m", "application/pkcs7-mime" },
                        { ".p7r", "application/x-pkcs7-certreqresp" },
                        { ".p7s", "application/pkcs7-signature" },
                        { ".pbm", "image/x-portable-bitmap" },
                        { ".pcx", "application/octet-stream" },
                        { ".pcz", "application/octet-stream" },
                        { ".pdf", "application/pdf" },
                        { ".pfb", "application/octet-stream" },
                        { ".pfm", "application/octet-stream" },
                        { ".pfx", "application/x-pkcs12" },
                        { ".pgm", "image/x-portable-graymap" },
                        { ".pko", "application/vnd.ms-pki.pko" },
                        { ".pma", "application/x-perfmon" },
                        { ".pmc", "application/x-perfmon" },
                        { ".pml", "application/x-perfmon" },
                        { ".pmr", "application/x-perfmon" },
                        { ".pmw", "application/x-perfmon" },
                        { ".png", "image/png" },
                        { ".pnm", "image/x-portable-anymap" },
                        { ".pnz", "image/png" },
                        { ".pot", "application/vnd.ms-powerpoint" },
                        { ".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12" },
                        { ".potx", "application/vnd.openxmlformats-officedocument.presentationml.template" },
                        { ".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12" },
                        { ".ppm", "image/x-portable-pixmap" },
                        { ".pps", "application/vnd.ms-powerpoint" },
                        { ".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12" },
                        { ".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow" },
                        { ".ppt", "application/vnd.ms-powerpoint" },
                        { ".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12" },
                        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                        { ".prf", "application/pics-rules" },
                        { ".prm", "application/octet-stream" },
                        { ".prx", "application/octet-stream" },
                        { ".ps", "application/postscript" },
                        { ".psd", "application/octet-stream" },
                        { ".psm", "application/octet-stream" },
                        { ".psp", "application/octet-stream" },
                        { ".pub", "application/x-mspublisher" },
                        { ".qt", "video/quicktime" },
                        { ".qtl", "application/x-quicktimeplayer" },
                        { ".qxd", "application/octet-stream" },
                        { ".ra", "audio/x-pn-realaudio" },
                        { ".ram", "audio/x-pn-realaudio" },
                        { ".rar", "application/octet-stream" },
                        { ".ras", "image/x-cmu-raster" },
                        { ".rf", "image/vnd.rn-realflash" },
                        { ".rgb", "image/x-rgb" },
                        { ".rm", "application/vnd.rn-realmedia" },
                        { ".rmi", "audio/mid" },
                        { ".roff", "application/x-troff" },
                        { ".rpm", "audio/x-pn-realaudio-plugin" },
                        { ".rtf", "application/rtf" },
                        { ".rtx", "text/richtext" },
                        { ".scd", "application/x-msschedule" },
                        { ".sct", "text/scriptlet" },
                        { ".sea", "application/octet-stream" },
                        { ".setpay", "application/set-payment-initiation" },
                        { ".setreg", "application/set-registration-initiation" },
                        { ".sgml", "text/sgml" },
                        { ".sh", "application/x-sh" },
                        { ".shar", "application/x-shar" },
                        { ".sit", "application/x-stuffit" },
                        { ".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12" },
                        { ".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide" },
                        { ".smd", "audio/x-smd" },
                        { ".smi", "application/octet-stream" },
                        { ".smx", "audio/x-smd" },
                        { ".smz", "audio/x-smd" },
                        { ".snd", "audio/basic" },
                        { ".snp", "application/octet-stream" },
                        { ".spc", "application/x-pkcs7-certificates" },
                        { ".spl", "application/futuresplash" },
                        { ".spx", "audio/ogg" },
                        { ".src", "application/x-wais-source" },
                        { ".ssm", "application/streamingmedia" },
                        { ".sst", "application/vnd.ms-pki.certstore" },
                        { ".stl", "application/vnd.ms-pki.stl" },
                        { ".sv4cpio", "application/x-sv4cpio" },
                        { ".sv4crc", "application/x-sv4crc" },
                        { ".svg", "image/svg+xml" },
                        { ".svgz", "image/svg+xml" },
                        { ".swf", "application/x-shockwave-flash" },
                        { ".t", "application/x-troff" },
                        { ".tar", "application/x-tar" },
                        { ".tcl", "application/x-tcl" },
                        { ".tex", "application/x-tex" },
                        { ".texi", "application/x-texinfo" },
                        { ".texinfo", "application/x-texinfo" },
                        { ".tgz", "application/x-compressed" },
                        { ".thmx", "application/vnd.ms-officetheme" },
                        { ".thn", "application/octet-stream" },
                        { ".tif", "image/tiff" },
                        { ".tiff", "image/tiff" },
                        { ".toc", "application/octet-stream" },
                        { ".tr", "application/x-troff" },
                        { ".trm", "application/x-msterminal" },
                        { ".ts", "video/vnd.dlna.mpeg-tts" },
                        { ".tsv", "text/tab-separated-values" },
                        { ".ttc", "application/x-font-ttf" },
                        { ".ttf", "application/x-font-ttf" },
                        { ".tts", "video/vnd.dlna.mpeg-tts" },
                        { ".txt", "text/plain" },
                        { ".u32", "application/octet-stream" },
                        { ".uls", "text/iuls" },
                        { ".ustar", "application/x-ustar" },
                        { ".vbs", "text/vbscript" },
                        { ".vcf", "text/x-vcard" },
                        { ".vcs", "text/plain" },
                        { ".vdx", "application/vnd.ms-visio.viewer" },
                        { ".vml", "text/xml" },
                        { ".vsd", "application/vnd.visio" },
                        { ".vss", "application/vnd.visio" },
                        { ".vst", "application/vnd.visio" },
                        { ".vsto", "application/x-ms-vsto" },
                        { ".vsw", "application/vnd.visio" },
                        { ".vsx", "application/vnd.visio" },
                        { ".vtx", "application/vnd.visio" },
                        { ".wasm", "application/wasm" },
                        { ".wav", "audio/wav" },
                        { ".wax", "audio/x-ms-wax" },
                        { ".wbmp", "image/vnd.wap.wbmp" },
                        { ".wcm", "application/vnd.ms-works" },
                        { ".wdb", "application/vnd.ms-works" },
                        { ".webm", "video/webm" },
                        { ".webp", "image/webp" },
                        { ".wks", "application/vnd.ms-works" },
                        { ".wm", "video/x-ms-wm" },
                        { ".wma", "audio/x-ms-wma" },
                        { ".wmd", "application/x-ms-wmd" },
                        { ".wmf", "application/x-msmetafile" },
                        { ".wml", "text/vnd.wap.wml" },
                        { ".wmlc", "application/vnd.wap.wmlc" },
                        { ".wmls", "text/vnd.wap.wmlscript" },
                        { ".wmlsc", "application/vnd.wap.wmlscriptc" },
                        { ".wmp", "video/x-ms-wmp" },
                        { ".wmv", "video/x-ms-wmv" },
                        { ".wmx", "video/x-ms-wmx" },
                        { ".wmz", "application/x-ms-wmz" },
                        { ".woff", "application/font-woff" }, // https://www.w3.org/TR/WOFF/#appendix-b
                        { ".woff2", "font/woff2" }, // https://www.w3.org/TR/WOFF2/#IMT
                        { ".wps", "application/vnd.ms-works" },
                        { ".wri", "application/x-mswrite" },
                        { ".wrl", "x-world/x-vrml" },
                        { ".wrz", "x-world/x-vrml" },
                        { ".wsdl", "text/xml" },
                        { ".wtv", "video/x-ms-wtv" },
                        { ".wvx", "video/x-ms-wvx" },
                        { ".x", "application/directx" },
                        { ".xaf", "x-world/x-vrml" },
                        { ".xaml", "application/xaml+xml" },
                        { ".xap", "application/x-silverlight-app" },
                        { ".xbap", "application/x-ms-xbap" },
                        { ".xbm", "image/x-xbitmap" },
                        { ".xdr", "text/plain" },
                        { ".xht", "application/xhtml+xml" },
                        { ".xhtml", "application/xhtml+xml" },
                        { ".xla", "application/vnd.ms-excel" },
                        { ".xlam", "application/vnd.ms-excel.addin.macroEnabled.12" },
                        { ".xlc", "application/vnd.ms-excel" },
                        { ".xlm", "application/vnd.ms-excel" },
                        { ".xls", "application/vnd.ms-excel" },
                        { ".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12" },
                        { ".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12" },
                        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                        { ".xlt", "application/vnd.ms-excel" },
                        { ".xltm", "application/vnd.ms-excel.template.macroEnabled.12" },
                        { ".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template" },
                        { ".xlw", "application/vnd.ms-excel" },
                        { ".xml", "text/xml" },
                        { ".xof", "x-world/x-vrml" },
                        { ".xpm", "image/x-xpixmap" },
                        { ".xps", "application/vnd.ms-xpsdocument" },
                        { ".xsd", "text/xml" },
                        { ".xsf", "text/xml" },
                        { ".xsl", "text/xml" },
                        { ".xslt", "text/xml" },
                        { ".xsn", "application/octet-stream" },
                        { ".xtp", "application/octet-stream" },
                        { ".xwd", "image/x-xwindowdump" },
                        { ".z", "application/x-compress" },
                        { ".zip", "application/x-zip-compressed" },
            };
    }
   
    public class TrelloLabels
    {
        public string IdLabel { get; set; }
        public string Nome { get; set; }
        public string Color { get; set; }
    }


}

using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Custom;
using System.Linq;
using System.Net;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FT_Management.Models
{
    public static class SMSContext
    {
        private static bool SendSMSEnable()
        {
            FT_ManagementContext context = new FT_ManagementContext(ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"], "");
            return context.ObterParam("SendSMS") == "1";
        }

        private static bool EnviarMensagemAsync(string Destino, string Mensagem)
        {
            if (SendSMSEnable())
            {
                FT_ManagementContext context = new FT_ManagementContext(ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"], "");
                switch (context.ObterParam("SMS_Service"))
                {
                    case "Android":
                         _ = EnviarMensagemAndroid(Destino, Mensagem);
                        break;
                    case "Twilio":
                         return EnviarMensagemTwilio(Destino, Mensagem);
                }
            }
            return false;
        }

        private async static Task EnviarMensagemAndroid(string Destino, string Mensagem)
        {
            //string url = "http://192.168.103.195:1688/services/api/messaging/?To=" + Destino + "&Message=" + Mensagem;
            string url = "http://192.168.103.195:1688/services/api/messaging/";
            var client = new HttpClient();

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("To", Destino),
                new KeyValuePair<string, string>("Message", Mensagem)
            };

            var content = new FormUrlEncodedContent(pairs);
            
            var response = await client.PostAsync(url, content);
        }

        private static bool EnviarMensagemTwilio(string Destino, string Mensagem)
        {

                string accountSid = ConfigurationManager.AppSetting["SMS:Sid"];
                string authToken = ConfigurationManager.AppSetting["SMS:Token"];

                try
                {
                    TwilioClient.Init(accountSid, authToken);

                    var message = MessageResource.Create(
                        body: Mensagem,
                        from: new Twilio.Types.PhoneNumber("+15136432435"),
                        to: new Twilio.Types.PhoneNumber(Destino)
                    );

                }
                catch
                {
                    return false;
                }

                return true;
        }

        public static void EnviarMensagemTeste(string Destino)
        {
           EnviarMensagemAndroid(Destino, "Mensagem de Teste!");
        }

        public static void EnviarMensagemCriacaoMarcacaoAsync(Marcacao m)
        {
            foreach (var u in m.LstTecnicos)
            {
                if (u.Telemovel.Length >=9)  EnviarMensagemAsync(u.ObterTelemovelFormatado(false), "Foi criada uma marcação nova para o cliente " + m.Cliente.NomeCliente + ".");
            }
        }
    }
}

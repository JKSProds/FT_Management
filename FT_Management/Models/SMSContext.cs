using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Custom;
using System.Linq;

namespace FT_Management.Models
{
    public static class SMSContext
    {
        private static bool SendSMSEnable()
        {
            FT_ManagementContext context = new FT_ManagementContext(ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"], "");
            return context.ObterParam("SendSMS") == "1";
        }
        private static bool EnviarMensagem(string Destino, string Mensagem)
        {
            if (SendSMSEnable())
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
            return false;
        }

        public static void EnviarMensagemTeste(string Destino)
        {
            EnviarMensagem(Destino, "Mensagem de Teste!");

        }

        public static void EnviarMensagemCriacaoMarcacao(Marcacao m)
        {
            foreach (var u in m.LstTecnicos)
            {
                if (u.Telemovel.Length >=9) EnviarMensagem(u.Telemovel, "Foi criada uma marcação nova para o cliente " + m.Cliente.NomeCliente + " para o(s) seguinte(s) dia(s) " + string.Join("|", m.DatasAdicionaisDistintas.Select(x => x.ToString("dd-MM-yyyy"))) + ".");
            }
        }
        public static void EnviarMensagemAtualizacaoMarcacao(Marcacao m)
        {
            foreach (var u in m.LstTecnicos)
            {
                if (!string.IsNullOrEmpty(u.Telemovel)) EnviarMensagem(u.Telemovel, "Foi atualizada a marcação Nº " + m.IdMarcacao + ", para o cliente " + m.Cliente.NomeCliente + ".");
            }
        }
    }
}

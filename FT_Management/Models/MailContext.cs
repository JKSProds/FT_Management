using System;
using System.Linq;
using System.Net.Mail;
using Custom;

namespace FT_Management.Models
{
    public static class MailContext
    {
        public static void EnviarMail(string EmailDestino, string Assunto, string Mensagem)
        {
            try
            {
                SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSetting["Email:ClienteSMTP"])
                {
                    UseDefaultCredentials = false
                };

                System.Net.NetworkCredential basicAuthenticationInfo = new
                   System.Net.NetworkCredential(ConfigurationManager.AppSetting["Email:EmailOrigem"], ConfigurationManager.AppSetting["Email:SenhaEmailOrigem"]);
                mySmtpClient.Credentials = basicAuthenticationInfo;

                MailAddress from = new MailAddress(ConfigurationManager.AppSetting["Email:EmailOrigem"], ConfigurationManager.AppSetting["Email:NomeOrigem"]);
                MailAddress to = new MailAddress(EmailDestino);
                MailMessage myMail = new System.Net.Mail.MailMessage(from, to);
                myMail.CC.Add(new MailAddress(ConfigurationManager.AppSetting["Email:EmailCC2"]));

                myMail.Subject = Assunto;
                myMail.SubjectEncoding = System.Text.Encoding.UTF8;

                if (int.Parse(DateTime.Now.ToString("HH")) < 13)
                {
                    myMail.Body = "Bom Dia, <br><br>";
                }
                else
                {
                    myMail.Body = "Boa Tarde, <br><br>";
                }

                myMail.Body += Mensagem;
                myMail.BodyEncoding = System.Text.Encoding.UTF8;
                myMail.IsBodyHtml = true;

                //Assinatura
                myMail.Body += "<br><br><i>Atenção este é um email automático, por favor não responda a este email!</i><br><br><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'><strong><span style='font-size:11px;font-family:'Rubik-Regular',serif;color:#0069A5;'><a href='http://www.food-tech.pt/'><span style='color:#0563C1;'>www.food-tech.pt</span></a></span></strong></p><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'>&nbsp;</p><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'><img width='250' src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAPoAAAAzCAYAAAC+CxVBAAAAAXNSR0IArs4c6QAAAERlWElmTU0AKgAAAAgAAYdpAAQAAAABAAAAGgAAAAAAA6ABAAMAAAABAAEAAKACAAQAAAABAAAA+qADAAQAAAABAAAAMwAAAABJcWeZAAAAHGlET1QAAAACAAAAAAAAABoAAAAoAAAAGgAAABkAAAqdPj/LXgAACmlJREFUeAHsnGuMVUUSx/nIV76RGAJRooSoROKTGHGJui4hMI7BBAPOKq7yEJXZEZa3yhtGERDULIiosDBKEFciDwmCgWRdIUQhEDQEZEIA5Tk8wsv2/o63L33PdFefc+65M4ycSU763j7VXdXV9e+qru47rVplf9eMBnbu3KnatbvB+axZ86W6ZoTNBElVA8OHv+yc93vvvSeb91S13cydZUBv5gloRvZlB3rfvo+rcj7NqLsWx7qlAn3evHlltaGwfR49evRP5+HKDnQpVEzjXYtDWzMK3FKBLhlpGjYU7uPgwZ8zoMe107AS0/4eV57rmT4Dujs/YdplBvQEKDEVWI7PCUS6bptkQM+AbsNgKsk4W8dp1l23qE0w8AzoGdBt2MuAngBM13KTDOgZ0DOgX8sITUm2DOgZ0JsF6FVVA9SpU6dKeqJi4MSJE2rDhg3qvffeVVOmTFYTJoxXtbUz1ZIlS9SOHTvU5cuXU820/vrrL2rt2jXqnXfmq8mTJwX83nzzDbVs2X/Url271JUrV1Ljh/yMa+TIEeqll4blypFqxozpatWqz9Tx48cLfFoq0KPOsS87j61F7ctGd+nSJbV161Y1f/68QNcvvviCguekSRPVp59+ourr60vq3+R5/vx5tWnT12ru3DnGvI5QU6dOUStXrlSHDh2KxUvSTTh0P3z4sKqrW56z2QmBPVVXDw8ws2LFCgWOTDkLn20riK4bOPAZe6NC69I+ACaM/Ykn+qr27ds5bwYhz+2336bGjBmt9u3bl1imixcvquXLl6nevXt5+XXteoeaOPH1xMaB0X388UeqW7f7xHF16NBePffcP9RPP/2oMqAnAzqgGj9+rLr11s6irrGjysrH1Pr16xLb0O7duwNw3Xxzxwi8KtXnn6+K5DSiAB3bx1YkrGBPo0ePUidPniweowa1rSwn0Ldt26YeeqiHV1lhuRgIk3r27NnigXjWHFbf++/vFpvfjTd2CLzvhQsXIvNjQnr2/FssXvDB44fHa35v6VdgJWNmnEk8+oIF/1ZRQGfqkc8DBvRXcS7enDlzJnA0EsjCPPT3Xr16qr1794r2I+kGj15XV6ewEd2nr6QNzqMAC6lBuYD+/vsLxVVJkkm/69HjL+rAgQNXB1IYUeMPM2fOiKwg3X+4JAog3G/ce3ENYXqXLl1K5hfmz/cM6Fd1TXQ2ePCgkvQMGKJEiEQMSZySOYedOt2iNm/e5LQfCeg4N7OvqJ/vvLPr1cVMalQOoLN/knjGecdA6usPOpWHWeD94/Qp0TLZ5p76qtn98Yk9YLlAjlxxgU6YuXr1arVo0aJED3vN7du3Rwo9w7qwfZeMmfHF8ehDhw5JZV4Bu7SA4/WTRII2O8Ijf/vt/6z26tONrb8odYMGPf8HP4k4baCzN5L4JXlHiMx+2GZY7MeT9Cm16devn5UX/Mk1SG1LfRcV6CQw8SA5kVJ52rZtq4iK4mxfbPPhM+aoQCdcL1WXZnuSzjZ5qUt7Tsn92BJmPt2Y8sb9HGwbpEYPPthdkYlO8oT3P+xxGKTEj3esehUVFQrlw99Hz3syn+GJOnLkSGDsvvbs7yorK4M9my9xpvtaunRpI354Tv1eKkkYYTz9+z+p8CYSbfidD+icTDz1VP9UwJ3TZ6N+8GxSRBOeg/B3nzFHATrz6tuTE1WRkGJBePvtuYGuw7oMf9+4cWOjOWWew3S274TWRJedO3eKRE9SOa5ubHyj1oHfVlGJ49KRQTYH4wvZUdasWbNUQ0NDUbsffvg+AL7EHwU3NJwuavfaa6+KSsdYMASOSUw5Ca0eeeRhse3dd9/VKIro06eP2IZTA44/wtEHx0E+fnrsPqDX1PyTsQQPMn7xxX8D74FOObqEj36ftMz3Yaos8uc0gM5JiNaHrUQHtkTtli1bRCCy2JsDYZ7QoY2HrsMRoWPThvbs2RNkxjWNrcSZHTt2rIifTzf0A0awazBBVMDWjKNDX4IQx9lkQJe8FwP45pvNRQM3lU7ihW2ETWm6jnBVtyHElFZXQE7STNOHy3PnzgVeXvdtKzmD1+1ICtpodB1eXEr6YJgYmqZ3lRLQ8wsrMgV9oTPbH2fL0JTycNdAjz1O6TNmn0cHfFIOhKMnSR7f1tHM97AwuuaBeuYr7JRM3uPGjRPbf/jh4iJZfbqBp2v+sX1JVnTWJEBn5ZEEwZObSrJ9RqnSJD/99N8LfbBoSPzMRcHGizpfiFhTU1Pg51M0uQIXH13v4ydNNH1wESdXqDZt2gRJLRvIqSO8L3X/jifTcscpfcbsAzpHsq55xatFuRAjHXuaW7KxY8c4eeEomC9p7DibcESAjCR06TuclPPpZsiQwSK/Rx/9q1Ne+DYJ0PEArgkijJFWRlOZ7DVc/bD/17Tsy1x0LBbh8Fm3C5fspVz9mCGsRIc3T4MfcrhWdOTOjz8IG10g1/WEe7kmJT1XfvutoO+w3lzffcbsAzpe0DUfadQDQC07Z9+uPkeN+leBTtPbSuyVY1luen711XprEk638+mGeyCa1lZyi9QlL/VNAnRJCBJTNsFtdd99939xMHqvNGLEK046zl5tfdvqAJZLeWwNdJtnnx3opKuqqirQaXpXyXbAxY96CejBZObAO23aVI1nZ5n3XMiV+JGOpFzj8xmzD+gARtJPqe/yicxAfCl65Laba4xJ63268TlDbmFK428SoHNv3SXEsGFDIyvNtxfWhkIY6+LHuXrUyeAM2dUP9bof/tWRiy6fINOkYknewNUP9RLQ8wuPqq6udgJcv5g9+y1kL+khByAOxvLSZ8x6/ixNgyrJjiS9RX3HPGrebG9c7Uigarq0Skk3plNx8ePuu0te6kWg421J8iR5tHdFMGklls4ww4PixybSYHTmXVJaHOARLrn4dex4U2GyGYOLzre3MsfIzSlXP9RLQOdKZ64vhVzsw6W/7t0fCGihT/JwgmDKHfWzNC+Mzwd030mKpLso70ygS8lcEnVRxxyVTtINiWxfPyUBPa0LMwsXLnAaMHvLqL8Uk5Je+QRToI8ZuV+GuSaWZIhPafo9ns/VD2fumo5fD7no8qDSpGI5Z85sZz/0LwF93bq1yBM8AML1t3jxBwU6TR+3rK2tLYxdHFDopWTMjM8HdH4B6NIziw8ev5RnUe4GoRaZK9YuXsG5tCYUSpKHzCnJYe2EXOSSbloM0Al1XEqj3jyqcimCeilBUlHRuzBJ/CJO4sdeX+LDOxJoXBBx9cO+XPfhuyPAhGtaV+njhxwS0OmXjHKuCB5yEWSG9R8gym9bCjSaNk5JxMDxY65N7D/JmBmfD+jSkRdHtLYbZ7GFzDcgEnPNPXYRJcHKcZ/ug8w3iwenNTis/fv3F+lQ0k2LATpHDdJtJgbiS+7wQxitNFtprrL0hWJtdNTh1bmpJxnB9OnTnO3pw1z9fUlCFijfntbHD54+oHNrLTgzNUJyIh3C0NatW5cE8JyuFFdhOSqV9Ca9k4yZ8fmAfvr0afEXXOhQ4s87QDY195txbjJKx3FS9IisJD0lXtK2j/bh25ySbtIA+u8AAAD//74e2kwAAAmLSURBVO2ca4xNVxTH56OvvkkaISoqoqSiykRKRVVlwlRDosFEqXoUNaamKFPvYjrqMVTKKCLVSTwrqSJRz6SKSI0QnfhQIqjneMUru/e356xj33PP68690zLdN5k5j/1Ya/33XmuvvfbeJ6dp05dU0N/w4R+qnCz9CgsnBtKBfvfu3VRVVVUKvcePH6ulS5eoZs2aBpYn7fz580llCwoKAvNDr0+fd1PKIOqDBw/U3LlzQsu2aNFcXb9+3aX39OlT1bFjh9AygwYNUleuXHHLCKzQmzVrZmhZaZ9du35OKS/1yPXu3btq7NgxqlGjRuTN2l///v3VxYsXI+kLH37XiRM/DZXz9u3bkfWPGDE8sI7mzZupvXv3BNaxZ89uRR7Bk2v79u1VQcEQVVq6KKlNb926pVq2fDkpr1mO+6lTpyg/nrdv36ZatWoZWvbcuXNJfIZh07nzG0l5/bCtrPwxlF6Ol3nzOZuKjmBekE1a3KOwQ4YMVsuXL1Pr16/TCpCb2yVUAMrRsb3CHz9+PLIc/AwbNkytWFGu6c2Y8UWkwkKvpGRGCj2MkVce7zONP27cWLVq1bdq3brv1bRpU1WHDq9FlpN64ii64IBRgcaUKZ+rCRPG1elv8uTPNDZeIyo00r2GdWZk9FMaL42odqUP0Y6nT59WGFEM8tGjv6ni4smhg0XXrrmKQcWkRzsL9kFXjMHgwR+4OFNPUF55j9E36XAfhs0LpegIM2/e3EgQBIy41zZtWgeONIWFhVmnh2Ji7ZHH/DGSpqO0ceUz86Wj6CZvz8t9WGdGzjiKjiwYSxOXbNxv3bo1pU1p52y3KYNLVdWpFFph2Lxwio7FHDDg/aw1EtZ79+5fUkCTjn3v3j3Vq9fbWaNHIzE6SP3e6759+0JHjUw7pFX0WsRRwDieXly8R436OLBNjxw5EumJxqVDPrxHb7/huUEpOgLV1NQo5nvpgOOXF6XbsmWLL2jQkd+1a3+r3r3fyZgeLhpzPKk36Lp69XcZ0/KTl3dW0Z+hznSiU6fXM8Y6L6+Pwht7VnPqHYMJcZmgdon7nqlaau21bxqcoiPWo0eP1Jw5s+s8+uHKhI2sXjDv37+vmG/GbRBvvh493vINFHrpyDOBkbp0DOh4aZvPVtEF4drr5cuXM/IQiV3g9SXX6v904sQJFWf+bbaX3NMX1qxZHUqnQSq6QEnAhCgqLriAEnZlvkSgjiCL1JHO9dix33XgJIyGmYZBqahYozBM6dAhL8FHAotmfUH37dq9qumcOvVHaH6r6KmtwIrHpk0/pOXK4+HF8c681Oh3y5Yt1ZH6oLY03+N1EiiOE8ysd0VnDhL0d/bs2bQ7uBecOM9Y5o0bN+rIMMteKBjLVVjQgQMHqNmzZynmv96oaJy6/fKwTFRRUaEbQehhRLp1e1MREZ0/f546fPiwevLkScbynzlzRi1atFCPPNAgeIhsdLbx4z9RmzdvVngc8IkLGdQWvDeX9Pzket7fVVf/GSpfJu1LWx06dFCviOTn5+sgGlizfEY/Kkgst5aVfa0wppni9PDhQ20ocMXz8/u6tGhfvLIxY0brFY+rV6/GphWGDSsNUTxDK6zvRJW36RYBi4BFwCJgEbAIWAQsAhYBi4BFwCJgEbAIWAQsAhYBi4BFwCLwf0CACPedO3cio86CxYED+xUrHTxTNmqdnOU5Ket3ZSVg//5fVXV1dWg+v7K8g35dl4PNOlmZyUY9Zp323iLw3CDgHKyKpWQ3b94kn14mRYCePXuoxo0b+5Zdu3atatKkic7Pspj3NJkA4KyB6+U5eZfOFfosp6ZTxi8vOzOzUY9f3fadReA/R4D17wQTsRVlx44dio1RMM6+CD9FZ1s0x3g5B8GRUhS+X79+vjTwCshT1/V36LMhKlMg2VTGnoBM67HlLQL1jgA7+1Aojs+yMamoaJLuuGyGYgMRHZlDT/JdAo4Qs9MswZguJ/kZfSlPfkZtc3sp5yjYoIQwQYp+4cJfuk52o3FijtES+n4A4DKzAWflyhU6HR4oN3Pml3qE5TsK8C9lDx48oDfN4CVMnz5dGxpR0AULvkqc83hPYTzIj0ECD9nx6MXh5MmTepMU9DFMGA3yl5eXaw+Eew7J4PUg94YNGzSvly5dcvmBT3ZoCn/2ahGodwQWL15Mh9N/nM3nHDdKTSdm9xodmH3hdOgbN27o7dGmohcXF+sOyzu2l5KfQynUaSibVgaECVJ00oYO1Z1fj+bQC9p9JtMBx7PIcZRWGxkMEbzzB798A0CeR478SLVt20bzJiM6ipkgrZw6c5yjsQpc2B1KWQ7q8L0CysAXdaLEpPHct2+eTnfk1fUx2mNY2M1J/Sg3MopBy+Z3JajX/iwCoQiIopeUlOiOSOaioiLdOXfu/EnvCWebauK13qJMup/rTgdmyycjIh+ZID/blsmf+EUqOuVRDPLyx4lDCo4ePSpl9PMqOgqI0snZeQ5LUQcn3CorK/W98MIUgTQZ0cMUXWIBbP9GiQni1dTUaL7gzeu6i6I7dZJF/5ALQ8iWYDyKxEuFZ+Ak24tFoP4REEU3P/JgKLLulAku9HXhwgW6cxrpmkEOGMmnshjh2M9OGcN4RCo6oyLKSjSdvencM4pydQyPC4ZX0VFalE4ymDKZ95IOj3FGdPKXlZW5AcJaXiYpWRUIUnRDbk2SMxyJG210iD0wtdAJ9p9F4N9CwE8RnHm3G0DjYA+jm8xjcY8T/Llf9eGkGs9LlnyjO7A8Gx0+UtGZk8soy8iJi0+dKCUutIlHOoouIzpHsKnDmSu7tIgrQEci/Cg2z+DCMh60UGxGYOHJGblzmOrAN/XykxHdkFu/J6aAgiNL4kWsbzbogvafRSBbCPgpOp2a0QuXk47fuvUr+lmi3KIMjIoElYiAJ/jRH/fERZYlMqPDRyq6841BPd/l6zKiFPDh1O+KnI6im3N05sXibYhRcQJ6eu5OOvSQBVxEcTlNiXfBXJ00cbvFSGAAkFXyG3K7PDuxDB3HEI/ATbQ3FoH6RkCi7rL8JfSY30rUncCTE1TSyYzsfJWX904ALYfRHEWg0xOQY57KcV8KELWXqDtBKcf1F1JJdebmdtEGhsAZCsW3ESgv3gSZJeqO8vGMB2J+esorkxl15xgreSWIKPNmjBqrBcQG4F2i7ngnLPlhGPLy8pKMDvN9voFIfuTCK+De8SJgzf1JkK+0tFTz7CbYG4uARaDhIICRwFuQgGHDkcxKYhGwCGgEJC7g/QT6P87274Y+vnkKAAAAAElFTkSuQmCC' alt='image'></p><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'><strong><span style='font-size:11px;font-family:'Rubik-Regular',serif;color:#0069A5;'>Subic, Lda</span></strong><strong><span style='font-size:11px;font-family:'Cambria',serif;color:#0069A5;'>&nbsp;</span></strong></p><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'><em><span style='font-size:11px;font-family:'Rubik-Regular',serif;color:#0069A5;'></span></em></p><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'><span style='font-size:11px;font-family:'Rubik-Regular',serif;color:black;'><a href='x-apple-data-detectors%3A//2/1' style='color:var(--linkColor);'>Rua Eng. Sabino Marques, 144</a>, Zona Industrial da Maia Sector II,</span><span style='font-size:11px;font-family:'Cambria',serif;color:black;'>&nbsp;</span></p><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'><span style='font-size:11px;font-family:'Rubik-Regular',serif;color:black;'><a href='tel:4470-605' style='color:var(--linkColor);'>4470-605</a> Maia, Portugal &bull; Tel. <a href='tel:(+351)%20229%20479%20670' style='color:var(--linkColor);'>(+351) 229 479</a></span><span style='font-size:11px;font-family:'Cambria',serif;color:black;'>&nbsp;</span><span style='font-size:11px;font-family:'Rubik-Regular',serif;color:black;'><a href='tel:(+351)%20229%20479%20670' style='color:var(--linkColor);'>670</a></span></p><br><br>Powered by: JKSProds - Software";

                  mySmtpClient.SendMailAsync(myMail);
            }
            catch (SmtpException ex)
            {
                throw new ApplicationException
                  ("SmtpException has occured: " + ex.Message);
            }
        }

        public static bool EnviarEmailMarcacaoTecnico(string EmailDestino, Marcacao m, string Tecnico)
        {
            string Assunto = "Nova Marcação - Cliente - " + m.Cliente.NomeCliente;
            string Mensagem = "Foi criada uma marcação para o cliente: " + m.Cliente.NomeCliente + "<br><br><b>Dados adicionais:</b><br>Técnico: " + Tecnico + "<br>Data: " + m.DataMarcacao.ToShortDateString() + "<br>Cliente: " + m.Cliente.NomeCliente + "<br>Morada: " + m.Cliente.MoradaCliente + "<br>Prioridade: " + m.PrioridadeMarcacao + "<br>Equipamento: " + m.TipoEquipamento;
            EnviarMail(EmailDestino, Assunto, Mensagem);

            return true;
        }


        public static bool EnviarEmailMarcacaoCliente(string EmailDestino, Marcacao m)
        {
            string Assunto = "Nova Marcação - Cliente - " + m.Cliente.NomeCliente;
            string Mensagem = "Foi agendada uma marcação para o dia: " + m.DataMarcacao.ToShortDateString() + "<br><br><b>Dados adicionais:</b><br>Técnico: " + string.Join(" | ", m.LstTecnicos.Select(x => x.NomeCompleto)) + "<br>Data: " + m.DataMarcacao.ToShortDateString() + "<br>Cliente: " + m.Cliente.NomeCliente + "<br>Morada: " + m.Cliente.MoradaCliente + "<br>Prioridade: " + m.PrioridadeMarcacao + "<br>Equipamento: " + m.TipoEquipamento;
            EnviarMail(EmailDestino, Assunto, Mensagem);

            return true;
        }
    }
}

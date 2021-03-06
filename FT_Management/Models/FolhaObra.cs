using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Recibo {
        public int IdRecibo { get; set; }
                [Display(Name = "Mão-de-Obra")]
        public double MaoObra { get; set; }
                [Display(Name = "Material Aplicado")]
        public double MaterialAplicado { get; set; }
                [Display(Name = "Deslocação")]
        public double Deslocacao { get; set; }
        public int IdFolhaObra { get; set; }
       public double TotalRecibo { get {return CalcularValorFinal();} }
        public double SubTotalRecibo { get {return CalcularSubtotal(); } }
        public double IvaRecibo { get {return Math.Round(CalcularIva(CalcularSubtotal(), 23), 2);} }

        private static String[] units = { "Zero", "Um", "Dois", "Três",  
    "Quatro", "Cinco", "Seis", "Sete", "Oito", "Nove", "Dez", "Onze",  
    "Doze", "Treze", "Catorze", "Quinze", "Dezasseis",  
    "Dezassete", "Dezoito", "Dezanove" };  
    private static String[] tens = { "", "", "Vinte", "Trinta", "Quarenta",  
    "Cinquenta", "Sessenta", "Setenta", "Oitenta", "Noventa" };  
      private static String[] cens = { "", "Cento", "Duzentos", "Trezentos", "Quatrocentos",  
    "Quinhentos", "Seiscentos", "Setecentos", "Oitocentos", "Novecentos" };  
    public String ConverterValorPalavras()  
    {  
        try  
        {  
            double amount = CalcularValorFinal();
            Int64 amount_int = (Int64)amount;  
            Int64 amount_dec = (Int64)Math.Round((amount - (double)(amount_int)) * 100);  
            if (amount_dec == 0)  
            {  
                return Convert(amount_int) + " Euros";  
            }  
            else  
            {  
                return Convert(amount_int) + " Euros e " + Convert(amount_dec) + " Cêntimos.";  
            }  
        }  
        catch (Exception)  
        {  
            // TODO: handle exception  
        }  
        return "";  
    }  
  
    public static String Convert(Int64 i)  
    {  
        if (i < 20)  
        {  
            return units[i];  
        }  
        if (i < 100)  
        {  
            return tens[i / 10] + ((i % 10 > 0) ? " e " + Convert(i % 10) : "");  
        }  
        if (i < 1000)  
        {  
            return  cens[i / 100]
                    + ((i % 100 > 0) ? " e " + Convert(i % 100) : "");  
        }  
        if (i < 100000)  
        {           return ((i / 1000) < 2 ? "" : Convert(i / 1000)) + " Mil"  
                    + ((i % 1000 > 0) ? " " + Convert(i % 1000) : "");  
        }  
        return Convert(i / 1000000000) + " Bilioes "  
                + ((i % 1000000000 > 0) ? " " + Convert(i % 1000000000) : "");  
    } 

        private double CalcularValorFinal() {
            double SubTotal = CalcularSubtotal();
            return (SubTotal + CalcularIva(SubTotal, 23));
        }
        private double CalcularSubtotal() {
            return MaoObra + MaterialAplicado + Deslocacao;
        }
        private double CalcularIva(double Valor, int Iva) {
            return (Valor/100)*Iva;
        }
    }
    public class Intervencao
    {
        public int IdIntervencao { get; set; }
        public int IdTecnico { get; set; }
        public int IdFolhaObra { get; set; }
        private string _NomeTecnico;
        [Display(Name = "Técnico")]
        public string NomeTecnico { get {return _NomeTecnico ?? ""; } set {_NomeTecnico = value ;} }
        [Display(Name = "Data do Serviço")]
        public DateTime DataServiço { get; set; }
        [Display(Name = "Hora de Inicio")]
        public DateTime HoraInicio { get; set; }
        [Display(Name = "Hora de Fim")]
        public DateTime HoraFim { get; set; }
    }

    public class Equipamento
    {
        public int IdEquipamento { get; set; }
        private string _DesignacaoEquipamento;
        [Display(Name = "Designação do Equipamento")]
        public string DesignacaoEquipamento { get {return _DesignacaoEquipamento ?? ""; } set {_DesignacaoEquipamento = value ;}  }
        private string _MarcaEquipamento;
        [Display(Name = "Marca")]
        public string MarcaEquipamento { get {return _MarcaEquipamento ?? ""; } set {_MarcaEquipamento = value ;} }
        private string _ModeloEquipamento;
        [Display(Name = "Modelo")]
        public string ModeloEquipamento { get {return _ModeloEquipamento ?? ""; } set {_ModeloEquipamento = value ;}  }
        private string _NumeroSerieEquipamento;
        [Required(ErrorMessage = "Número de Série é Obrigatório")]
        [Display(Name = "Numero de Série")]
        public string NumeroSerieEquipamento { get {return _NumeroSerieEquipamento ?? ""; } set {_NumeroSerieEquipamento = value ;} }
    }

    public class Vendedor
    {
        public int IdVendedor { get; set; }
        private string _NomeVendedor;
        [Required(ErrorMessage = "Nome do Vendedor é Obrigatório")]
        [Display(Name = "Nome do Cliente")]
        public string NomeVendedor { get { return _NomeVendedor ?? ""; } set { _NomeVendedor = value; } }
        public string uid { get; set; }
    }

    public class Cliente
    {
        public int IdCliente { get; set; }
        public int IdLoja { get; set; }
        private string _NomeCliente;
        [Required(ErrorMessage = "Nome do Cliente é Obrigatório")]
        [Display(Name = "Nome do Cliente")]
        public string NomeCliente { get {return _NomeCliente ?? ""; } set {_NomeCliente = value ;} }
        private string _PessoaContactoCliente;
        [Display(Name = "Pessoa de Contacto")]
        public string PessoaContatoCliente { get {return _PessoaContactoCliente ?? ""; } set {_PessoaContactoCliente = value ;} }
        private string _MoradaCliente;
        [Display(Name = "Morada")]
        public string MoradaCliente { get {return _MoradaCliente ?? ""; } set {_MoradaCliente = value ;} }
        private string _EmailCliente;
        [Display(Name = "Email")]
        public string EmailCliente { get {return _EmailCliente ?? ""; } set {_EmailCliente = value ;}  }
        public string TelefoneCliente { get; set; }
        private string _NumeroContribuinteCliente;
        [Display(Name = "Numero de Contribuinte")]
        public string NumeroContribuinteCliente { get {return _NumeroContribuinteCliente ?? ""; } set {_NumeroContribuinteCliente = value ;} }
        public int IdVendedor { get; set; }
        public string TipoCliente { get; set; }
        public bool IsValidContrib()
        {
            string Contrib = NumeroContribuinteCliente;
            bool functionReturnValue = false;
            functionReturnValue = false;
            string[] s = new string[9];
            string Ss = null;
            string C = null;
            int i = 0;
            long checkDigit = 0;

            s[0] = Convert.ToString(Contrib[0]);
            s[1] = Convert.ToString(Contrib[1]);
            s[2] = Convert.ToString(Contrib[2]);
            s[3] = Convert.ToString(Contrib[3]);
            s[4] = Convert.ToString(Contrib[4]);
            s[5] = Convert.ToString(Contrib[5]);
            s[6] = Convert.ToString(Contrib[6]);
            s[7] = Convert.ToString(Contrib[7]);
            s[8] = Convert.ToString(Contrib[8]);

            if (Contrib.Length == 9)
            {
                C = s[0];
                if (s[0] == "1" || s[0] == "2" || s[0] == "5" || s[0] == "6" || s[0] == "9")
                {
                    checkDigit = Convert.ToInt32(C) * 9;
                    for (i = 2; i <= 8; i++)
                    {
                        checkDigit = checkDigit + (Convert.ToInt32(s[i - 1]) * (10 - i));
                    }
                    checkDigit = 11 - (checkDigit % 11);
                    if ((checkDigit >= 10))
                        checkDigit = 0;
                    Ss = s[0] + s[1] + s[2] + s[3] + s[4] + s[5] + s[6] + s[7] + s[8];
                    if ((checkDigit == Convert.ToInt32(s[8])))
                        functionReturnValue = true;
                }
            }
            return functionReturnValue;
        }
    }
    public class FolhaObra
    {
        public int IdFolhaObra { get; set; }
        [Display(Name = "Data")]
        public DateTime DataServico { get; set; }
        private string _ReferenciaServico;
        [Display(Name = "Referência")]
        public string ReferenciaServico { get {return _ReferenciaServico ?? ""; } set {_ReferenciaServico = value ;} }
        [Display(Name = "Estado do Equipamento")]
        public string EstadoEquipamento { get; set; }
        private string _RelatorioServico;
        [DataType(DataType.MultilineText)]
        [Display(Name = "Relatório do Serviço")]
        public string RelatorioServico { get {return _RelatorioServico ?? ""; } set {_RelatorioServico = value ;}  }
        private string _SituacoesPendentes;
        [DataType(DataType.MultilineText)]
        [Display(Name = "Situações Pendentes")]
        public string SituacoesPendentes { get {return _SituacoesPendentes ?? ""; } set {_SituacoesPendentes = value ;} }
        public List<Produto> PecasServico { get; set; }
        public List<Intervencao> IntervencaosServico { get; set; }
        public Equipamento EquipamentoServico { get; set; }
        public Cliente ClienteServico { get; set; }
        public string IdCartao { get; set; }
        private string _ConferidoPor;
        [Display(Name = "Conferido por")]
        public string ConferidoPor { get {return _ConferidoPor ?? ""; } set {_ConferidoPor = value ;} }
        private string _GuiaTransporteAtual; 
        [Display(Name = "Número da tua Guia de Transporte")]
        public string GuiaTransporteAtual { get {return _GuiaTransporteAtual ?? ""; } set {_GuiaTransporteAtual = value ;} }
        [Display(Name = "Assistência Remota?")]
        public bool AssistenciaRemota { get; set; }
        public string RubricaCliente { get; set; }
        public Recibo Recibo { get; set; }
    }

    public class Movimentos
    {
        public int IdFolhaObra { get; set; }
        public int IdTecnico { get; set; }
        public string GuiaTransporte { get; set; }
        public string NomeTecnico { get; set; }
        public string RefProduto { get; set; }
        public string Designacao { get; set; }
        public float Quantidade { get; set; }
        public string NomeCliente { get; set; }
        public DateTime DataMovimento { get; set; }
    }
}

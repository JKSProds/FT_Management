using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public class Equipamento
    {
        public string IdEquipamento { get; set; }
        private string _DesignacaoEquipamento;
        [Display(Name = "Designação do Equipamento")]
        public string DesignacaoEquipamento { get { return _DesignacaoEquipamento ?? ""; } set { _DesignacaoEquipamento = value; } }
        private string _MarcaEquipamento;
        [Display(Name = "Marca")]
        public string MarcaEquipamento { get { return _MarcaEquipamento ?? ""; } set { _MarcaEquipamento = value; } }
        private string _ModeloEquipamento;
        [Display(Name = "Modelo")]
        public string ModeloEquipamento { get { return _ModeloEquipamento ?? ""; } set { _ModeloEquipamento = value; } }
        private string _NumeroSerieEquipamento;
        [Required(ErrorMessage = "Número de Série é Obrigatório")]
        [Display(Name = "Numero de Série")]
        public string NumeroSerieEquipamento { get { return _NumeroSerieEquipamento ?? ""; } set { _NumeroSerieEquipamento = value; } }
        [Display(Name = "Num. do Cliente")]
        public int IdCliente { get; set; }
        [Display(Name = "Num. do Estabelecimento")]
        public int IdLoja { get; set; }
        [Display(Name = "Num. do Fornecedor")]
        public int IdFornecedor { get; set; }
        [Display(Name = "Referência do Equipamento")]
        public string RefProduto { get; set; }
    }
}

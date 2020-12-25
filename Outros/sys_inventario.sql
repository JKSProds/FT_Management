-- MySQL Administrator dump 1.4
--
-- ------------------------------------------------------
-- Server version	5.0.19-nt


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;


--
-- Create schema sys_inventario
--

CREATE DATABASE IF NOT EXISTS sys_inventario;
USE sys_inventario;

--
-- Definition of table `dat_clientes`
--

DROP TABLE IF EXISTS `dat_clientes`;
CREATE TABLE `dat_clientes` (
  `IdCliente` int(10) unsigned NOT NULL auto_increment,
  `NomeCliente` varchar(1024) NOT NULL,
  `PessoaContactoCliente` varchar(45) default NULL,
  `MoradaCliente` varchar(1024) default NULL,
  `EmailCliente` varchar(1024) default NULL,
  `NumeroContribuinteCliente` varchar(45) default NULL,
  PRIMARY KEY  (`IdCliente`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_clientes`
--

/*!40000 ALTER TABLE `dat_clientes` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_clientes` ENABLE KEYS */;


--
-- Definition of table `dat_equipamentos`
--

DROP TABLE IF EXISTS `dat_equipamentos`;
CREATE TABLE `dat_equipamentos` (
  `IdEquipamento` int(10) unsigned NOT NULL auto_increment,
  `DesignacaoEquipamento` varchar(1024) default NULL,
  `MarcaEquipamento` varchar(1024) default NULL,
  `ModeloEquipamento` varchar(1024) default NULL,
  `NumeroSerieEquipamento` varchar(1024) NOT NULL,
  PRIMARY KEY  (`IdEquipamento`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_equipamentos`
--

/*!40000 ALTER TABLE `dat_equipamentos` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_equipamentos` ENABLE KEYS */;


--
-- Definition of table `dat_folhas_obra`
--

DROP TABLE IF EXISTS `dat_folhas_obra`;
CREATE TABLE `dat_folhas_obra` (
  `IdFolhaObra` int(10) unsigned NOT NULL auto_increment,
  `DataServico` datetime NOT NULL,
  `ReferenciaServico` varchar(45) default NULL,
  `EstadoEquipamento` varchar(45) default NULL,
  `RelatorioServico` varchar(1024) default NULL,
  `SituacoesPendentes` varchar(1024) default NULL,
  `IdEquipamento` int(10) unsigned NOT NULL,
  `IdCliente` int(10) unsigned NOT NULL,
  `IdCartaoTrello` varchar(100) default NULL,
  `ConferidoPor` varchar(200) default NULL,
  `GuiaTransporteAtual` varchar(45) default NULL,
  `Remoto` tinyint(1) NOT NULL default '0',
  PRIMARY KEY  (`IdFolhaObra`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_folhas_obra`
--

/*!40000 ALTER TABLE `dat_folhas_obra` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_folhas_obra` ENABLE KEYS */;


--
-- Definition of table `dat_intervencoes_folha_obra`
--

DROP TABLE IF EXISTS `dat_intervencoes_folha_obra`;
CREATE TABLE `dat_intervencoes_folha_obra` (
  `IdIntervencao` int(10) unsigned NOT NULL auto_increment,
  `IdTecnico` int(10) unsigned NOT NULL,
  `IdFolhaObra` int(10) unsigned NOT NULL,
  `NomeTecnico` varchar(1024) NOT NULL,
  `DataServico` datetime NOT NULL,
  `HoraInicio` varchar(45) NOT NULL,
  `HoraFim` varchar(45) NOT NULL,
  PRIMARY KEY  (`IdIntervencao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_intervencoes_folha_obra`
--

/*!40000 ALTER TABLE `dat_intervencoes_folha_obra` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_intervencoes_folha_obra` ENABLE KEYS */;


--
-- Definition of table `dat_logs_stocks`
--

DROP TABLE IF EXISTS `dat_logs_stocks`;
CREATE TABLE `dat_logs_stocks` (
  `id_log` int(10) unsigned NOT NULL auto_increment,
  `id_produto` int(10) unsigned NOT NULL,
  `qtd_anterior` double unsigned zerofill NOT NULL,
  `qtd_seguinte` double unsigned zerofill NOT NULL,
  `user` varchar(100) NOT NULL,
  `data_log` timestamp NOT NULL default CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP,
  PRIMARY KEY  (`id_log`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_logs_stocks`
--

/*!40000 ALTER TABLE `dat_logs_stocks` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_logs_stocks` ENABLE KEYS */;


--
-- Definition of table `dat_produto_intervencao`
--

DROP TABLE IF EXISTS `dat_produto_intervencao`;
CREATE TABLE `dat_produto_intervencao` (
  `RefProduto` varchar(250) NOT NULL,
  `Designacao` varchar(1024) NOT NULL,
  `Quantidade` float unsigned zerofill NOT NULL,
  `IdFolhaObra` int(10) unsigned NOT NULL,
  PRIMARY KEY  (`RefProduto`,`IdFolhaObra`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_produto_intervencao`
--

/*!40000 ALTER TABLE `dat_produto_intervencao` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_produto_intervencao` ENABLE KEYS */;


--
-- Definition of table `dat_produtos`
--

DROP TABLE IF EXISTS `sys_inventario_test`.`dat_produtos`;
CREATE TABLE  `sys_inventario_test`.`dat_produtos` (
  `ref_produto` varchar(254) NOT NULL,
  `designacao_produto` varchar(1024) DEFAULT NULL,
  `stock_fisico` double DEFAULT NULL,
  `stock_phc` double DEFAULT NULL,
  `stock_rec` double DEFAULT NULL,
  `stock_res` double DEFAULT NULL,
  `pos_stock` varchar(45) DEFAULT NULL,
  `armazem_id` int(11) NOT NULL,
  `obs` varchar(1024) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  PRIMARY KEY (`ref_produto`,`armazem_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_produtos`
--

/*!40000 ALTER TABLE `dat_produtos` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_produtos` ENABLE KEYS */;

DROP TABLE IF EXISTS `sys_inventario_test`.`dat_armazem`;
CREATE TABLE  `sys_inventario_test`.`dat_armazem` (
  `armazem_id` int(11) NOT NULL,
  `armazem_nome` varchar(256) NOT NULL DEFAULT ' ',
  PRIMARY KEY (`armazem_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;

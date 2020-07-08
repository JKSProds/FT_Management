-- MySQL dump 10.10
--
-- Host: localhost    Database: sys_inventario
-- ------------------------------------------------------
-- Server version	5.0.19-nt

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `dat_intervencoes_folha_obra`
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
LOCK TABLES `dat_intervencoes_folha_obra` WRITE;
INSERT INTO `dat_intervencoes_folha_obra` (`IdIntervencao`, `IdTecnico`, `IdFolhaObra`, `NomeTecnico`, `DataServico`, `HoraInicio`, `HoraFim`) VALUES (1,0,1,'Jorge Monteiro','2020-07-05 00:00:00','18:00','19:00');
UNLOCK TABLES;
/*!40000 ALTER TABLE `dat_intervencoes_folha_obra` ENABLE KEYS */;

--
-- Table structure for table `dat_clientes`
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
LOCK TABLES `dat_clientes` WRITE;
INSERT INTO `dat_clientes` (`IdCliente`, `NomeCliente`, `PessoaContactoCliente`, `MoradaCliente`, `EmailCliente`, `NumeroContribuinteCliente`) VALUES (2,'PD Vila das Aves','Jorge Monteiro','Casa','','');
UNLOCK TABLES;
/*!40000 ALTER TABLE `dat_clientes` ENABLE KEYS */;

--
-- Table structure for table `dat_equipamentos`
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
LOCK TABLES `dat_equipamentos` WRITE;
INSERT INTO `dat_equipamentos` (`IdEquipamento`, `DesignacaoEquipamento`, `MarcaEquipamento`, `ModeloEquipamento`, `NumeroSerieEquipamento`) VALUES (1,'Balança','Dibal','S545','31612300');
UNLOCK TABLES;
/*!40000 ALTER TABLE `dat_equipamentos` ENABLE KEYS */;

--
-- Table structure for table `dat_folhas_obra`
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
  PRIMARY KEY  (`IdFolhaObra`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_folhas_obra`
--


/*!40000 ALTER TABLE `dat_folhas_obra` DISABLE KEYS */;
LOCK TABLES `dat_folhas_obra` WRITE;
INSERT INTO `dat_folhas_obra` (`IdFolhaObra`, `DataServico`, `ReferenciaServico`, `EstadoEquipamento`, `RelatorioServico`, `SituacoesPendentes`, `IdEquipamento`, `IdCliente`) VALUES (1,'2020-07-20 00:00:00','580202633','Instalação','Testes. OK','Sem situações pendentes!',1,2);
UNLOCK TABLES;
/*!40000 ALTER TABLE `dat_folhas_obra` ENABLE KEYS */;

--
-- Table structure for table `dat_produto_intervencao`
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
LOCK TABLES `dat_produto_intervencao` WRITE;
INSERT INTO `dat_produto_intervencao` (`RefProduto`, `Designacao`, `Quantidade`, `IdFolhaObra`) VALUES ('90500.42.599','Cabeça Térmica \"L-8Xx\" (Bh-Kf2002GR)',000000000001,1),('90500.42.620','Sensor Opto Digital L-8Xx (4505014051)',000000000001,1);
UNLOCK TABLES;
/*!40000 ALTER TABLE `dat_produto_intervencao` ENABLE KEYS */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;


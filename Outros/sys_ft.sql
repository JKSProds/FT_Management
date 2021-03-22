-- MySQL Administrator dump 1.4
--
-- ------------------------------------------------------
-- Server version	5.5.5-10.4.17-MariaDB


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;


--
-- Create schema sys_ft
--

CREATE DATABASE IF NOT EXISTS sys_ft;
USE sys_ft;

--
-- Definition of table `dat_armazem`
--

DROP TABLE IF EXISTS `dat_armazem`;
CREATE TABLE `dat_armazem` (
  `armazem_id` int(11) NOT NULL,
  `armazem_nome` varchar(256) NOT NULL DEFAULT ' ',
  PRIMARY KEY (`armazem_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `dat_armazem`
--

/*!40000 ALTER TABLE `dat_armazem` DISABLE KEYS */;
INSERT INTO `dat_armazem` (`armazem_id`,`armazem_nome`) VALUES 
 (0,'Não Definido'),
 (1,'Aveiro'),
 (2,'Alverca'),
 (3,'Maia'),
 (31,'Luis Fernandes'),
 (32,'Jorge Monteiro'),
 (33,'Helder Pinto'),
 (34,'Zarcofrio'),
 (35,'Pedro Santos'),
 (36,'Nelson Martins'),
 (37,'Praiotel'),
 (38,'Ricardo Oliveira'),
 (39,'Armando Rodrigues'),
 (42,'Paulo Deus'),
 (43,'Diogo Correia'),
 (45,'Daniel Diogo'),
 (46,'Henrique Crispim'),
 (48,'Ricardo Almeida'),
 (99,'Produção & Montagem');
/*!40000 ALTER TABLE `dat_armazem` ENABLE KEYS */;


--
-- Definition of table `dat_clientes`
--

DROP TABLE IF EXISTS `dat_clientes`;
CREATE TABLE `dat_clientes` (
  `IdCliente` int(10) unsigned NOT NULL,
  `IdLoja` int(10) unsigned NOT NULL,
  `NomeCliente` varchar(1024) NOT NULL,
  `PessoaContactoCliente` varchar(45) DEFAULT NULL,
  `MoradaCliente` varchar(1024) DEFAULT NULL,
  `EmailCliente` varchar(1024) DEFAULT NULL,
  `NumeroContribuinteCliente` varchar(45) DEFAULT NULL,
  `Telefone` varchar(45) DEFAULT NULL,
  `IdVendedor` int(10) unsigned NOT NULL,
  `TipoCliente` varchar(45) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdCliente`,`IdLoja`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_clientes`
--

/*!40000 ALTER TABLE `dat_clientes` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_clientes` ENABLE KEYS */;


--
-- Definition of table `dat_controlo_viatura`
--

DROP TABLE IF EXISTS `dat_controlo_viatura`;
CREATE TABLE `dat_controlo_viatura` (
  `id_reg` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `nome_tecnico` varchar(45) NOT NULL,
  `matricula_viatura` varchar(45) NOT NULL,
  `kms_viatura` varchar(45) NOT NULL,
  `data_inicio` datetime NOT NULL,
  `devolvida_viatura` tinyint(1) DEFAULT NULL,
  `notas_viatura` varchar(1024) DEFAULT NULL,
  `data_fim` datetime DEFAULT NULL,
  `kms_finais` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id_reg`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `dat_controlo_viatura`
--

/*!40000 ALTER TABLE `dat_controlo_viatura` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_controlo_viatura` ENABLE KEYS */;


--
-- Definition of table `dat_equipamentos`
--

DROP TABLE IF EXISTS `dat_equipamentos`;
CREATE TABLE `dat_equipamentos` (
  `IdEquipamento` varchar(50) NOT NULL,
  `NumeroSerieEquipamento` varchar(250) NOT NULL,
  `DesignacaoEquipamento` varchar(45) DEFAULT NULL,
  `MarcaEquipamento` varchar(45) DEFAULT NULL,
  `ModeloEquipamento` varchar(45) DEFAULT NULL,
  `IdCliente` int(11) DEFAULT NULL,
  `IdLoja` int(11) DEFAULT NULL,
  `IdFornecedor` int(10) unsigned DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`NumeroSerieEquipamento`) USING BTREE,
  KEY `KEY` (`IdEquipamento`)
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
  `IdFolhaObra` int(10) unsigned NOT NULL,
  `DataServico` datetime NOT NULL,
  `ReferenciaServico` varchar(45) DEFAULT NULL,
  `EstadoEquipamento` varchar(45) DEFAULT NULL,
  `SituacoesPendentes` varchar(1024) DEFAULT NULL,
  `IdEquipamento` varchar(50) NOT NULL,
  `IdCliente` int(10) unsigned NOT NULL,
  `IdLoja` int(10) unsigned NOT NULL,
  `IdCartaoTrello` varchar(100) DEFAULT NULL,
  `ConferidoPor` varchar(250) DEFAULT NULL,
  `GuiaTransporteAtual` varchar(100) DEFAULT NULL,
  `Remoto` tinyint(1) NOT NULL DEFAULT 0,
  `RubricaCliente` blob DEFAULT NULL,
  `EstadoFolhaObra` varchar(45) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdFolhaObra`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_folhas_obra`
--

/*!40000 ALTER TABLE `dat_folhas_obra` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_folhas_obra` ENABLE KEYS */;


--
-- Definition of table `dat_fornecedores`
--

DROP TABLE IF EXISTS `dat_fornecedores`;
CREATE TABLE `dat_fornecedores` (
  `IdFornecedor` int(10) unsigned NOT NULL,
  `NomeFornecedor` varchar(45) DEFAULT NULL,
  `MoradaFornecedor` varchar(120) DEFAULT NULL,
  `ContactoFornecedor` varchar(45) DEFAULT NULL,
  `ReferenciaFornecedor` varchar(45) DEFAULT NULL,
  `EmailFornecedor` varchar(120) DEFAULT NULL,
  `PessoaContactoFornecedor` varchar(45) DEFAULT NULL,
  `Obs` varchar(250) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdFornecedor`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `dat_fornecedores`
--

/*!40000 ALTER TABLE `dat_fornecedores` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_fornecedores` ENABLE KEYS */;


--
-- Definition of table `dat_intervencoes_folha_obra`
--

DROP TABLE IF EXISTS `dat_intervencoes_folha_obra`;
CREATE TABLE `dat_intervencoes_folha_obra` (
  `IdIntervencao` int(10) unsigned NOT NULL,
  `IdTecnico` int(10) unsigned NOT NULL,
  `IdFolhaObra` int(10) unsigned NOT NULL,
  `NomeTecnico` varchar(1024) NOT NULL,
  `RelatorioServico` varchar(1024) DEFAULT NULL,
  `DataServico` datetime NOT NULL,
  `HoraInicio` varchar(45) NOT NULL,
  `HoraFim` varchar(45) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdIntervencao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_intervencoes_folha_obra`
--

/*!40000 ALTER TABLE `dat_intervencoes_folha_obra` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_intervencoes_folha_obra` ENABLE KEYS */;


--
-- Definition of table `dat_logs`
--

DROP TABLE IF EXISTS `dat_logs`;
CREATE TABLE `dat_logs` (
  `id_log` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user` varchar(100) NOT NULL,
  `data_log` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `msg_log` varchar(1024) NOT NULL,
  `tipo_log` int(11) NOT NULL COMMENT '1 - Stocks 2 - Impressoes, 3 - Folhas de Obra, 4 - Logins',
  PRIMARY KEY (`id_log`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_logs`
--

/*!40000 ALTER TABLE `dat_logs` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_logs` ENABLE KEYS */;


--
-- Definition of table `dat_marcacoes`
--

DROP TABLE IF EXISTS `dat_marcacoes`;
CREATE TABLE `dat_marcacoes` (
  `IdMarcacao` int(10) unsigned NOT NULL,
  `DataMarcacao` datetime NOT NULL,
  `IdCliente` int(10) unsigned NOT NULL,
  `IdLoja` int(10) unsigned NOT NULL,
  `TipoMarcacao` tinyint(1) NOT NULL DEFAULT 1,
  `ResumoMarcacao` varchar(1024) DEFAULT NULL,
  `EstadoMarcacao` varchar(45) DEFAULT NULL,
  `PrioridadeMarcacao` varchar(45) DEFAULT NULL,
  `MarcacaoStamp` varchar(45) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdMarcacao`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `dat_marcacoes`
--

/*!40000 ALTER TABLE `dat_marcacoes` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_marcacoes` ENABLE KEYS */;


--
-- Definition of table `dat_marcacoes_tecnico`
--

DROP TABLE IF EXISTS `dat_marcacoes_tecnico`;
CREATE TABLE `dat_marcacoes_tecnico` (
  `IdMarcacaoTecnico` varchar(45) NOT NULL,
  `MarcacaoStamp` varchar(45) NOT NULL,
  `IdTecnico` int(10) unsigned NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdMarcacaoTecnico`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `dat_marcacoes_tecnico`
--

/*!40000 ALTER TABLE `dat_marcacoes_tecnico` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_marcacoes_tecnico` ENABLE KEYS */;


--
-- Definition of table `dat_produto_intervencao`
--

DROP TABLE IF EXISTS `dat_produto_intervencao`;
CREATE TABLE `dat_produto_intervencao` (
  `RefProduto` varchar(250) NOT NULL,
  `Designacao` varchar(1024) NOT NULL,
  `Quantidade` float unsigned zerofill NOT NULL,
  `tipoun` varchar(11) NOT NULL DEFAULT 'UN',
  `IdFolhaObra` int(10) unsigned NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`RefProduto`,`IdFolhaObra`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_produto_intervencao`
--

/*!40000 ALTER TABLE `dat_produto_intervencao` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_produto_intervencao` ENABLE KEYS */;


--
-- Definition of table `dat_produtos`
--

DROP TABLE IF EXISTS `dat_produtos`;
CREATE TABLE `dat_produtos` (
  `ref_produto` varchar(254) NOT NULL,
  `designacao_produto` varchar(1024) DEFAULT NULL,
  `stock_fisico` double DEFAULT NULL,
  `stock_phc` double DEFAULT NULL,
  `stock_rec` double DEFAULT NULL,
  `stock_res` double DEFAULT NULL,
  `armazem_id` int(11) NOT NULL,
  `modificado` tinyint(1) NOT NULL DEFAULT 0,
  `pos_stock` varchar(45) DEFAULT NULL,
  `obs` varchar(1024) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`ref_produto`,`armazem_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_produtos`
--

/*!40000 ALTER TABLE `dat_produtos` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_produtos` ENABLE KEYS */;


--
-- Definition of table `dat_recibos`
--

DROP TABLE IF EXISTS `dat_recibos`;
CREATE TABLE `dat_recibos` (
  `IdRecibo` int(11) unsigned NOT NULL,
  `MaoObra` double NOT NULL DEFAULT 0,
  `Deslocacao` double NOT NULL DEFAULT 0,
  `MaterialAplicado` double NOT NULL DEFAULT 0,
  `IdFolhaObra` int(11) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdRecibo`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `dat_recibos`
--

/*!40000 ALTER TABLE `dat_recibos` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_recibos` ENABLE KEYS */;


--
-- Definition of table `dat_vendedores`
--

DROP TABLE IF EXISTS `dat_vendedores`;
CREATE TABLE `dat_vendedores` (
  `IdVendedor` int(10) unsigned NOT NULL,
  `NomeVendedor` varchar(45) DEFAULT NULL,
  `uid` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`IdVendedor`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `dat_vendedores`
--

/*!40000 ALTER TABLE `dat_vendedores` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_vendedores` ENABLE KEYS */;


--
-- Definition of table `sys_tabelas`
--

DROP TABLE IF EXISTS `sys_tabelas`;
CREATE TABLE `sys_tabelas` (
  `idtabela` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `nometabela` varchar(45) NOT NULL,
  `ultimamodificacao` datetime NOT NULL DEFAULT '1900-01-01 00:00:00',
  PRIMARY KEY (`idtabela`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `sys_tabelas`
--

/*!40000 ALTER TABLE `sys_tabelas` DISABLE KEYS */;
INSERT INTO `sys_tabelas` (`idtabela`,`nometabela`,`ultimamodificacao`) VALUES 
 (1,'sa','1900-01-01 00:00:00'),
 (2,'cl','1900-01-01 00:00:00'),
 (3,'fl','1900-01-01 00:00:00'),
 (4,'ma','1900-01-01 00:00:00'),
 (5,'pa','1900-01-01 00:00:00'),
 (6,'mh','1900-01-01 00:00:00'),
 (7,'bi','1900-01-01 00:00:00'),
 (8,'u_marcacao','1900-01-01 00:00:00'),
 (9,'u_mtecnicos','1900-01-01 00:00:00');
/*!40000 ALTER TABLE `sys_tabelas` ENABLE KEYS */;


--
-- Definition of table `sys_utilizadores`
--

DROP TABLE IF EXISTS `sys_utilizadores`;
CREATE TABLE `sys_utilizadores` (
  `IdUtilizador` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `NomeUtilizador` varchar(45) NOT NULL,
  `Password` varchar(1024) NOT NULL,
  `NomeCompleto` varchar(250) NOT NULL,
  `TipoUtilizador` varchar(45) NOT NULL,
  `EmailUtilizador` varchar(250) DEFAULT NULL,
  `IdCartaoTrello` varchar(100) DEFAULT NULL,
  `timestamp` timestamp NULL DEFAULT NULL,
  `admin` tinyint(1) NOT NULL DEFAULT 0,
  `enable` tinyint(1) NOT NULL DEFAULT 1,
  `IdPHC` int(10) unsigned DEFAULT NULL,
  `IdArmazem` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`IdUtilizador`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8;

--
-- Dumping data for table `sys_utilizadores`
--

/*!40000 ALTER TABLE `sys_utilizadores` DISABLE KEYS */;
INSERT INTO `sys_utilizadores` (`IdUtilizador`,`NomeUtilizador`,`Password`,`NomeCompleto`,`TipoUtilizador`,`EmailUtilizador`,`IdCartaoTrello`,`timestamp`,`admin`,`enable`,`IdPHC`,`IdArmazem`) VALUES 
 (1,'jmonteiro','AQAAAAEAACcQAAAAELeq22xmE9gUCMQun/WymGZPjjucwx/ilg8x8M9V2p8mB2vn6KM0Yiy2DVOzOB31aw==','Jorge Monteiro','1','jmonteiro@food-tech.pt','',NULL,1,1,33,NULL),
 (2,'lfernandes','AQAAAAEAACcQAAAAEGZVKT2u/mis91pvh6f+nYWdf717pNoGPsYozocKDUgkmQuoGzXo3C1cmfbuLYa9ZA==','Luis Fernandes','1','lfernandes@food-tech.pt',NULL,NULL,1,1,20,NULL),
 (3,'nmartins','AQAAAAEAACcQAAAAEFiEkz1uvmCRGLEvI6Bmbk9WEBcWWTcVYR18Bp4mY3wVLPKu+ffzpGd3fsi2HExmxw==','Nelson Martins','1','nmartins@food-tech.pt','59ccb51b751826836cfd4f95',NULL,0,0,40,NULL),
 (4,'ralmeida','AQAAAAEAACcQAAAAEAahhVEsmXwaU/80zNlihzpegFlL3He7ME06rE5SkYw/V4rctBUEF6RyspfjQOlKrg==','Ricardo Almeida','1','ralmeida@food-tech.pt','',NULL,0,0,34,NULL),
 (5,'psantos','AQAAAAEAACcQAAAAEKJAEPtx976uQnH8ogf7juNT5YKqY+JswmSrl7fSh/vpbQi+bpr0RJYfIUKaANjYdQ==','Pedro Santos','1','psantos@food-tech.pt','5cffd63b18581a4e74da24a9',NULL,0,0,42,NULL),
 (6,'arodrigues','AQAAAAEAACcQAAAAEGb+vnSBz+OsooXZmpq/6ZPWkEOYSlNvPWkySdUky8tznkgEFNLxXKtjIj3tLxymyg==','Armando Rodrigues','0','arodrigues@food-tech.pt','59ccb52aa80fdc78530096ea',NULL,0,0,NULL,NULL),
 (7,'roliveira','AQAAAAEAACcQAAAAEA7IIX+6geOrhV3B7jp9TpDoopWYYTDfocrKjcacqUgq3sVwpHySxlTMVvo/php7Tw==','Ricardo Oliveira','1','hpinto@food-tech.pt','59ccb536db119ea33b01400e',NULL,0,0,43,NULL),
 (8,'silvino','AQAAAAEAACcQAAAAEBBfOfMVcgM23Gg6dHJY/zXA+sGNhUpsQ01j3ZjGVNTeEoVk+YsFFbEJ6FXuEUYVBg==','Silvino','0','silvino@food-tech.pt',NULL,NULL,0,0,NULL,NULL),
 (9,'dcorreia','AQAAAAEAACcQAAAAEGzOCegSc8kce1FXkJe/aZULradxJHOHmM8GqOb6wOgkD9Qa5dazX4ke3ahqtOpDSA==','Diogo Correia','1','dcorreia@food-tech.pt','59ccb5722a8d2ef7d1ce17fb',NULL,0,0,7,NULL),
 (10,'pdeus','AQAAAAEAACcQAAAAEARXJ+xEuLbpJu3S5sPkzPEJju1BMgKlf/M69TtGVAey/qqVJcINiU5Lp5yqA9VWNQ==','Paulo Deus','1','pdeus@food-tech.pt','5e4124fb4b50a44e13907669',NULL,0,0,41,NULL),
 (11,'ddiogo','AQAAAAEAACcQAAAAEHBnt8rzwBHS+Km0cWiDFg7zttD7ZtI8yvmX+3EBdTkqEzGFzKVSExZx3dqEFGBfmQ==','Daniel Diogo','1','ddiogo@food-tech.pt','5db07146e80294682409b74e',NULL,0,0,44,NULL),
 (12,'mferreira','AQAAAAEAACcQAAAAEGidEMqvkvZ78Dp9LY8aN8Ee9evNGqOCEKm+96tWDexuXvonazLIamJbz209G/CeDQ==','Mafalda Ferreira','3','mferreira@food-tech.pt',NULL,NULL,1,1,NULL,NULL),
 (13,'cperes','AQAAAAEAACcQAAAAEIdBe7OJkMOy1j3pHhCFpT3RWrqigW/J82K0BZRdfsQZTJkC000XH423UKQ2jQi4Vg==','Cindy Peres','3','cperes@food-tech.pt',NULL,NULL,0,1,NULL,NULL),
 (14,'pecas','AQAAAAEAACcQAAAAEDqkoqMObRWuK8JNAgwbRe3jewdzLpeCqyD3N+tM590Z3d0hTTFHM//Uk/RZM61RLA==','Peças','3','pecas@food-tech.pt',NULL,NULL,0,1,NULL,NULL),
 (15,'hcrispim','AQAAAAEAACcQAAAAEG2jClLgA9a/+Ahq+HXAYSEDaxbEzgOEZuDAJyf2nygBd+lfZNpgkCinEvktwdu1ig==','Henrique Crispim','1','sopesagem@gmail.com','5cf68fd1933da030a49330c6',NULL,0,0,9,NULL),
 (16,'fsoares','AQAAAAEAACcQAAAAEDqkoqMObRWuK8JNAgwbRe3jewdzLpeCqyD3N+tM590Z3d0hTTFHM//Uk/RZM61RLA==','Filipe Soares','2','fsoares@food-tech.pt',NULL,NULL,1,1,500,NULL);
/*!40000 ALTER TABLE `sys_utilizadores` ENABLE KEYS */;


--
-- Definition of table `sys_viaturas`
--

DROP TABLE IF EXISTS `sys_viaturas`;
CREATE TABLE `sys_viaturas` (
  `matricula_viatura` varchar(45) NOT NULL,
  `marca` varchar(45) DEFAULT NULL,
  `modelo` varchar(45) DEFAULT NULL,
  `responsavel` varchar(45) DEFAULT NULL,
  `ultimoKms` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`matricula_viatura`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `sys_viaturas`
--

/*!40000 ALTER TABLE `sys_viaturas` DISABLE KEYS */;
INSERT INTO `sys_viaturas` (`matricula_viatura`,`marca`,`modelo`,`responsavel`,`ultimoKms`) VALUES 
 ('26-89-OF','Volwswgwen','LT3','',''),
 ('AF-02-BT','Opel','','','');
/*!40000 ALTER TABLE `sys_viaturas` ENABLE KEYS */;




/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;

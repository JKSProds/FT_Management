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
-- Definition of table `dat_acessos`
--

DROP TABLE IF EXISTS `dat_acessos`;
CREATE TABLE `dat_acessos` (
  `Id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `IdUtilizador` int(10) unsigned NOT NULL,
  `DataHoraAcesso` datetime NOT NULL,
  `TipoAcesso` varchar(45) NOT NULL,
  `Temperatura` varchar(45) DEFAULT 'N/D',
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=121 DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `dat_acessos`
--



--
-- Definition of table `dat_clientes`
--

DROP TABLE IF EXISTS `dat_clientes`;
CREATE TABLE `dat_clientes` (
  `IdCliente` int(10) unsigned NOT NULL AUTO_INCREMENT,
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
) ENGINE=InnoDB AUTO_INCREMENT=903053 DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_clientes`
--

/*!40000 ALTER TABLE `dat_clientes` DISABLE KEYS */;

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
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=latin1;

--
-- Dumping data for table `dat_controlo_viatura`
--

/*!40000 ALTER TABLE `dat_controlo_viatura` DISABLE KEYS */;
INSERT INTO `dat_controlo_viatura` (`id_reg`,`nome_tecnico`,`matricula_viatura`,`kms_viatura`,`data_inicio`,`devolvida_viatura`,`notas_viatura`,`data_fim`,`kms_finais`) VALUES 
 (1,'','26-89-OF','200000','2021-02-08 09:00:00',0,NULL,'2021-02-08 09:00:00','200000'),
 (2,'Pedro Santos','AF-02-BT','45','2021-02-08 09:00:00',NULL,'Ponte de Lima','2021-02-08 18:30:00','300'),
 (3,'Nelson Martins','AF-02-BT','300','2021-02-08 18:30:00',NULL,'PD Marco de Canaveses ','2021-02-09 18:30:00','422'),
 (4,'Jorge Monteiro','AF-02-BT','3421','2021-04-19 08:50:00',1,'CBD Mercado','2021-04-19 13:11:00','3450'),
 (5,'Jorge Monteiro','AF-02-BT','5300','2021-05-11 16:00:00',1,'Lisboa','2021-05-13 20:00:00','5800'),
 (6,'Luis Fernandes','AF-02-BT','5800','2021-05-12 20:00:00',1,'Lisboa','2021-05-13 17:02:00','6210'),
 (7,'Jorge Monteiro','AF-02-BT','6210','2021-05-13 17:02:00',1,'Lisboa','2021-05-14 08:09:00','6265');
/*!40000 ALTER TABLE `dat_controlo_viatura` ENABLE KEYS */;


--
-- Definition of table `dat_equipamentos`
--

DROP TABLE IF EXISTS `dat_equipamentos`;
CREATE TABLE `dat_equipamentos` (
  `NumeroSerieEquipamento` varchar(250) NOT NULL,
  `DesignacaoEquipamento` varchar(45) DEFAULT NULL,
  `MarcaEquipamento` varchar(45) DEFAULT NULL,
  `ModeloEquipamento` varchar(45) DEFAULT NULL,
  `IdCliente` int(11) DEFAULT NULL,
  `IdLoja` int(11) DEFAULT NULL,
  `IdFornecedor` int(10) unsigned DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `IdEquipamento` varchar(50) NOT NULL,
  PRIMARY KEY (`IdEquipamento`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_equipamentos`
--

/*!40000 ALTER TABLE `dat_equipamentos` DISABLE KEYS */;

--
-- Definition of table `dat_ferias`
--

DROP TABLE IF EXISTS `dat_ferias`;
CREATE TABLE `dat_ferias` (
  `Id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `DataInicio` datetime NOT NULL,
  `DataFim` datetime NOT NULL,
  `Validado` tinyint(1) NOT NULL DEFAULT 0,
  `Obs` varchar(256) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `IdUtilizador` int(10) unsigned NOT NULL,
  `ValidadoPor` int(10) unsigned DEFAULT 0,
  `mail_validacao` tinyint(1) NOT NULL DEFAULT 0,
  `mail_validado` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=55 DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `dat_ferias`
--

/*!40000 ALTER TABLE `dat_ferias` DISABLE KEYS */;
INSERT INTO `dat_ferias` (`Id`,`DataInicio`,`DataFim`,`Validado`,`Obs`,`timestamp`,`IdUtilizador`,`ValidadoPor`,`mail_validacao`,`mail_validado`) VALUES 
 (6,'2021-12-08 00:00:00','2021-12-08 00:00:00',0,'','2021-12-27 17:43:02',3,0,1,0);
/*!40000 ALTER TABLE `dat_ferias` ENABLE KEYS */;


--
-- Definition of table `dat_ferias_ano`
--

DROP TABLE IF EXISTS `dat_ferias_ano`;
CREATE TABLE `dat_ferias_ano` (
  `Id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Ano` varchar(45) NOT NULL,
  `Active` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `dat_ferias_ano`
--

/*!40000 ALTER TABLE `dat_ferias_ano` DISABLE KEYS */;
INSERT INTO `dat_ferias_ano` (`Id`,`Ano`,`Active`) VALUES 
 (1,'2021',0),
 (2,'2022',1);
/*!40000 ALTER TABLE `dat_ferias_ano` ENABLE KEYS */;


--
-- Definition of table `dat_ferias_utilizador`
--

DROP TABLE IF EXISTS `dat_ferias_utilizador`;
CREATE TABLE `dat_ferias_utilizador` (
  `Id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `IdUtilizador` int(10) unsigned NOT NULL,
  `Ano` varchar(45) NOT NULL,
  `DiasDireito` int(10) unsigned NOT NULL DEFAULT 23,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `dat_ferias_utilizador`
--


--
-- Definition of table `dat_folhas_obra`
--

DROP TABLE IF EXISTS `dat_folhas_obra`;
CREATE TABLE `dat_folhas_obra` (
  `IdFolhaObra` int(10) unsigned NOT NULL AUTO_INCREMENT,
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
) ENGINE=InnoDB AUTO_INCREMENT=104570 DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_folhas_obra`
--


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

--
-- Definition of table `dat_intervencoes_folha_obra`
--

DROP TABLE IF EXISTS `dat_intervencoes_folha_obra`;
CREATE TABLE `dat_intervencoes_folha_obra` (
  `IdIntervencao` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `IdTecnico` int(10) unsigned NOT NULL,
  `IdFolhaObra` int(10) unsigned NOT NULL,
  `NomeTecnico` varchar(1024) NOT NULL,
  `RelatorioServico` varchar(1024) DEFAULT NULL,
  `DataServico` datetime NOT NULL,
  `HoraInicio` varchar(45) NOT NULL,
  `HoraFim` varchar(45) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdIntervencao`)
) ENGINE=InnoDB AUTO_INCREMENT=233074 DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_intervencoes_folha_obra`
--


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
) ENGINE=InnoDB AUTO_INCREMENT=6443 DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_logs`
--

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
  `EstadoMarcacao` int(10) unsigned DEFAULT 1,
  `PrioridadeMarcacao` varchar(45) DEFAULT NULL,
  `MarcacaoStamp` varchar(45) DEFAULT NULL,
  `TipoEquipamento` varchar(45) NOT NULL,
  `Oficina` tinyint(1) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `Instalacao` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`IdMarcacao`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `dat_marcacoes`
--


--
-- Definition of table `dat_marcacoes_estado`
--

DROP TABLE IF EXISTS `dat_marcacoes_estado`;
CREATE TABLE `dat_marcacoes_estado` (
  `IdEstado` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `EstadoMarcacaoDesc` varchar(128) DEFAULT NULL,
  PRIMARY KEY (`IdEstado`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `dat_marcacoes_estado`
--

/*!40000 ALTER TABLE `dat_marcacoes_estado` DISABLE KEYS */;
INSERT INTO `dat_marcacoes_estado` (`IdEstado`,`EstadoMarcacaoDesc`) VALUES 
 (1,'Criado'),
 (2,'Em Curso'),
 (3,'Cancelado'),
 (4,'Finalizado'),
 (5,'Agendado'),
 (6,'Orçamentado'),
 (7,'Pedido Peças'),
 (8,'Pedido Orçamento'),
 (9,'Em oficina'),
 (10,'Enc. a Fornecedor'),
 (11,'Transferido'),
 (12,'Em receção'),
 (13,'Enc. de Cliente'),
 (14,'Reagendado'),
 (15,'AT Validada'),
 (16,'Enviado para Alverca'),
 (17,'Pedido Cotação'),
 (18,'Pedido Fornecedor'),
 (19,'Pedido Garantia'),
 (20,'Aguarda Ped. Compra'),
 (21,'Peças na Maia'),
 (22,'Enviado p/ Zarcofrio');
/*!40000 ALTER TABLE `dat_marcacoes_estado` ENABLE KEYS */;


DROP TABLE IF EXISTS `dat_marcacoes_tecnico`;
CREATE TABLE `dat_marcacoes_tecnico` (
  `IdMarcacaoTecnico` varchar(45) NOT NULL,
  `MarcacaoStamp` varchar(45) NOT NULL,
  `IdTecnico` int(10) unsigned NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `NomeTecnico` varchar(50) NOT NULL,
  `Marcado` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`IdMarcacaoTecnico`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `dat_marcacoes_tecnico`
--


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

--
-- Definition of table `dat_propostas`
--

DROP TABLE IF EXISTS `dat_propostas`;
CREATE TABLE `dat_propostas` (
  `IdProposta` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `IdVisita` int(10) unsigned NOT NULL,
  `IdComercial` int(10) unsigned NOT NULL,
  `DataProposta` datetime NOT NULL,
  `EstadoProposta` varchar(45) NOT NULL,
  `ValorProposta` varchar(45) DEFAULT NULL,
  `UrlAnexo` varchar(500) NOT NULL,
  PRIMARY KEY (`IdProposta`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `dat_propostas`
--

/*!40000 ALTER TABLE `dat_propostas` DISABLE KEYS */;
/*!40000 ALTER TABLE `dat_propostas` ENABLE KEYS */;


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


--
-- Definition of table `dat_viaturas`
--

DROP TABLE IF EXISTS `dat_viaturas`;
CREATE TABLE `dat_viaturas` (
  `matricula_viatura` varchar(45) NOT NULL,
  `marca` varchar(45) DEFAULT NULL,
  `modelo` varchar(45) DEFAULT NULL,
  `responsavel` varchar(45) DEFAULT NULL,
  `ultimoKms` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`matricula_viatura`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `dat_viaturas`
--

/*!40000 ALTER TABLE `dat_viaturas` DISABLE KEYS */;
INSERT INTO `dat_viaturas` (`matricula_viatura`,`marca`,`modelo`,`responsavel`,`ultimoKms`) VALUES 
 ('26-89-OF','Volwswgwen','LT3','',''),
 ('AF-02-BT','Opel',NULL,NULL,NULL);
/*!40000 ALTER TABLE `dat_viaturas` ENABLE KEYS */;


--
-- Definition of table `dat_visitas`
--

DROP TABLE IF EXISTS `dat_visitas`;
CREATE TABLE `dat_visitas` (
  `IdVisita` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `DataVisita` datetime NOT NULL,
  `IdCliente` int(10) unsigned NOT NULL,
  `IdLoja` int(10) unsigned NOT NULL,
  `ResumoVisita` varchar(1024) DEFAULT NULL,
  `EstadoVisita` varchar(45) DEFAULT NULL,
  `ObsVisita` varchar(1024) DEFAULT NULL,
  `VisitaStamp` varchar(250) DEFAULT NULL,
  `IdComercial` int(10) unsigned NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdVisita`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=latin1;

--
-- Dumping data for table `dat_visitas`
--

/*!40000 ALTER TABLE `dat_visitas` DISABLE KEYS */;
INSERT INTO `dat_visitas` (`IdVisita`,`DataVisita`,`IdCliente`,`IdLoja`,`ResumoVisita`,`EstadoVisita`,`ObsVisita`,`VisitaStamp`,`IdComercial`,`timestamp`) VALUES 
 (1,'2021-10-07 00:00:00',78,0,'Deslocação ao cliente para verificar a recepção do equipamento da Fessmann e analisar novas necessidades . ','Agendado','',NULL,925,'2021-11-01 10:19:19'),
 (2,'2021-10-06 00:00:00',5987,0,'Reunião com Gabriel Jesus, para analisar projetos de nova fábrica para processamento de carne de peru e unidade para PET food ','Agendado','',NULL,925,'2021-11-01 10:19:22'),
 (3,'2021-10-07 00:00:00',43,0,'Análise FS\r\nTroca de formato tampa','Agendado','testes',NULL,925,'2021-12-27 17:34:10'),
 (9,'2021-11-01 00:00:00',5132,0,'testes','Agendada','',NULL,925,'2021-11-02 09:03:55'),
 (10,'2021-11-04 00:00:00',5075,0,'teste2','Agendado','',NULL,925,'2021-11-02 09:04:26');
/*!40000 ALTER TABLE `dat_visitas` ENABLE KEYS */;


--
-- Definition of table `sys_armazem`
--

DROP TABLE IF EXISTS `sys_armazem`;
CREATE TABLE `sys_armazem` (
  `armazem_id` int(11) NOT NULL,
  `armazem_nome` varchar(256) NOT NULL DEFAULT ' ',
  PRIMARY KEY (`armazem_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `sys_armazem`
--

/*!40000 ALTER TABLE `sys_armazem` DISABLE KEYS */;
INSERT INTO `sys_armazem` (`armazem_id`,`armazem_nome`) VALUES 
 (0,'Não Definido'),
 (1,'Aveiro'),
 (2,'Alverca'),
 (3,'Maia'),
 (4,'Quimiloureiro'),
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
/*!40000 ALTER TABLE `sys_armazem` ENABLE KEYS */;


--
-- Definition of table `sys_tabelas`
--

DROP TABLE IF EXISTS `sys_tabelas`;
CREATE TABLE `sys_tabelas` (
  `idtabela` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `nometabela` varchar(45) NOT NULL,
  `ultimamodificacao` datetime NOT NULL DEFAULT '1900-01-01 00:00:00',
  PRIMARY KEY (`idtabela`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `sys_tabelas`
--

/*!40000 ALTER TABLE `sys_tabelas` DISABLE KEYS */;
INSERT INTO `sys_tabelas` (`idtabela`,`nometabela`,`ultimamodificacao`) VALUES 
 (1,'sa','2021-12-27 00:00:00'),
 (2,'cl','2021-12-27 00:00:00'),
 (3,'fl','2021-12-27 00:00:00'),
 (4,'ma','2021-12-27 00:00:00'),
 (5,'pa','2021-12-27 00:00:00'),
 (6,'mh','2021-12-27 00:00:00'),
 (7,'bi','2021-12-27 00:00:00'),
 (8,'u_marcacao','2021-12-27 00:00:00'),
 (9,'u_mtecnicos','2021-12-27 00:00:00'),
 (10,'u_estados','2021-12-27 00:00:00');
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
  `IdPHC` int(10) unsigned DEFAULT 0,
  `IdArmazem` int(10) unsigned DEFAULT NULL,
  `IniciaisUtilizador` varchar(2) NOT NULL,
  `CorCalendario` varchar(10) NOT NULL,
  PRIMARY KEY (`IdUtilizador`)
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8;

--
-- Dumping data for table `sys_utilizadores`
--

/*!40000 ALTER TABLE `sys_utilizadores` DISABLE KEYS */;
INSERT INTO `sys_utilizadores` (`IdUtilizador`,`NomeUtilizador`,`Password`,`NomeCompleto`,`TipoUtilizador`,`EmailUtilizador`,`IdCartaoTrello`,`timestamp`,`admin`,`enable`,`IdPHC`,`IdArmazem`,`IniciaisUtilizador`,`CorCalendario`) VALUES 
 (1,'jmonteiro','AQAAAAEAACcQAAAAEO352NvY5bM5+pSNCAHnkwBej8DpM1qOrEoKUrn5GqqYEBbGrc106rxgccCT8pZrVA==','Jorge Monteiro','1','jmonteiro@food-tech.pt','',NULL,1,1,33,NULL,'JM','#5DADE2'),
 (2,'lfernandes','AQAAAAEAACcQAAAAEGa5Ya1Onll8wfY+0/pu9SHOohK0xeTF46/LZo7lAPtHDpC80VF80m/yuuMghey3NA==','Luis Fernandes','1','lfernandes@food-tech.pt',NULL,NULL,1,1,20,NULL,'LF','#CB4335'),
 (3,'nmartins','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Nelson Martins','1','nmartins@food-tech.pt','59ccb51b751826836cfd4f95',NULL,0,1,40,NULL,'NM','#48C9B0'),
 (4,'ralmeida','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Ricardo Almeida','1','ralmeida@food-tech.pt','',NULL,0,0,34,NULL,'RA','#A569BD'),
 (5,'psantos','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Pedro Santos','1','psantos@food-tech.pt','5cffd63b18581a4e74da24a9',NULL,0,0,42,NULL,'PS','#F4D03F'),
 (6,'arodrigues','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Armando Rodrigues','1','arodrigues@food-tech.pt','59ccb52aa80fdc78530096ea',NULL,0,0,0,NULL,'AR','#F5B041'),
 (7,'roliveira','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Ricardo Oliveira','1','hpinto@food-tech.pt','59ccb536db119ea33b01400e',NULL,0,0,43,NULL,'RO','#EB984E'),
 (8,'silvino','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Silvino','1','silvino@food-tech.pt',NULL,NULL,0,0,0,NULL,'SI','#ECF0F1'),
 (9,'dcorreia','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Diogo Correia','1','dcorreia@food-tech.pt','59ccb5722a8d2ef7d1ce17fb',NULL,0,0,7,NULL,'DC','#95A5A6'),
 (10,'pdeus','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Paulo Deus','1','pdeus@food-tech.pt','5e4124fb4b50a44e13907669',NULL,0,0,41,NULL,'PD','#2E4053'),
 (11,'ddiogo','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Daniel Diogo','1','ddiogo@food-tech.pt','5db07146e80294682409b74e',NULL,0,0,44,NULL,'DD','#145A32'),
 (12,'mferreira','AQAAAAEAACcQAAAAEKdhkrmdPgvJI+LEmvEHDj1lWi0vNb6ckMQjMdcba7RH9FnA1VXZS/pUivfoZFkj8g==','Mafalda Ferreira','3','mferreira@food-tech.pt',NULL,NULL,1,1,0,NULL,'MF',''),
 (13,'cperes','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Cindy Peres','3','cperes@food-tech.pt',NULL,NULL,0,1,0,NULL,'CP',''),
 (14,'pecas','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','João Santos','3','pecas@food-tech.pt',NULL,NULL,0,1,45,NULL,'JS','#7D6608'),
 (15,'hcrispim','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Henrique Crispim','1','sopesagem@gmail.com','5cf68fd1933da030a49330c6',NULL,0,0,9,NULL,'HC','#17202A'),
 (16,'fsoares','AQAAAAEAACcQAAAAEC9lfm4fJqtSTbUYQF/SxJh4E97rLQrjhUrp1PZYTtXVoJsFLUXQ/bZW8EbazCK69g==','Filipe Soares','2','fsoares@food-tech.pt',NULL,NULL,1,1,925,NULL,'FS',''),
 (17,'estagiario','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Estagiario','1',NULL,NULL,NULL,0,1,32,NULL,'TE','#7D6608'),
 (18,'asousa','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','André Sousa','1','asousa@food-tech.pt',NULL,NULL,0,0,47,NULL,'AS','#7D6608'),
 (19,'asilva','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Avelino Silva','1','assistecnica@zarcofrio.pt',NULL,NULL,0,0,18,NULL,'ZA','#7D6608'),
 (20,'efalcao','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Eduardo Falcão','2','efalcao@food-tech.pt',NULL,NULL,0,1,1006,NULL,'EF',' '),
 (21,'dteixeira','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Davi Teixeira','2','dteixeira@food-tech.pt',NULL,NULL,0,1,540,NULL,'DT',' '),
 (22,'jcalheiros','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','João Calheiros','2','jcalheiros@food-tech.pt',NULL,NULL,0,1,1001,NULL,'JC',' '),
 (23,'lfaria','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Luis Faria','2',NULL,NULL,NULL,0,1,1012,NULL,'LF',' '),
 (24,'acarneiro','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Artur Carneiro','2','acarneiro@food-tech.pt',NULL,NULL,0,1,1010,NULL,'AC',' '),
 (25,'prosinha','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Pedro Rosinha','1','prosinha@food-tech.pt',NULL,NULL,1,1,46,NULL,'PR',' '),
 (26,'acosta','AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==','Ana Costa','3','geral@food-tech.pt',NULL,NULL,1,1,0,NULL,'AC',' ');
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

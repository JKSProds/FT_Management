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
INSERT INTO `dat_fornecedores` (`IdFornecedor`,`NomeFornecedor`,`MoradaFornecedor`,`ContactoFornecedor`,`ReferenciaFornecedor`,`EmailFornecedor`,`PessoaContactoFornecedor`,`Obs`,`timestamp`) VALUES 
 (24,'Sunany Techonology Limited','Room 107, Wego,1 Bantian High-Tech Industrial Park, Longgang Shenzhen, China','+8675584877890','N/D','lila@sunany.com','Lila','','2021-03-22 16:17:45'),
 (25,'MIMAC - FOOD PROCESSING EQUIPMENT','Via dell\'Industria, 22   36013 PIOVENE ROCCHETTE (VI) - ITÁLIA','0039 0445 576250','N/D','info@mimac.com','','','2021-03-22 16:17:45'),
 (26,'Befor','8 Rue des Fondrières Saint Clement 89100 Saint Clemente - France','003386832230','N/D','befor@befor.fr','Isabelle CAMES','','2021-03-22 16:17:45'),
 (27,'IMANPACK PACKAGING & ECO SOLUTIONS S.P.A.','VIA LAGO DI BOLSENA NR 19 36015 SCHIO VI 36015 SCHIO VI  ITALY','+39 0445 578 811','N/D','info@imanpack.it','','','2021-03-22 16:17:45'),
 (28,'LADO PERIFERICO UNIPESSOAL LDA','RUA DO ROUXINOL N 57 4 FTE CORROIOS 2855-205 CORROIOS','211588251','N/D','geral@lado-periferico.pt','Eng. José Martins','Escritório e Fábrica:Rua Quinta do Gato Bravo Nº 1038°38\'44.7\"N 9°10\'13.4\"W2810-069 Almada','2021-03-22 16:17:45'),
 (29,'Carpigiani Horeca S. L. U','C/ Tramuntana, 10 Valencia 46716 Rafelcofer','0034610115344','N/D','info@sencotel.es','Job Puig','','2021-03-22 16:17:45'),
 (30,'Guangzhou Winson Information Technology Co.LT','Room B, Floor 8, Changming Building 72 bend road, Changsha Kowloon, Hong Kong','+8620-82510810','N/D','pansy@winsonchina.com','','Morada da Fábrica:1st Floor, Block C, Yue\'an Industrial Park, No.59 Huang Cun Road,TianheDistrict, Guangzhou, Guangdong, China','2021-03-22 20:27:38'),
 (31,'J.P.SA COUTO, S.A.','Rua da Guarda, 675 MATOSINHOS 4455-466 PERAFITA- MATOSINHOS','22 9993900','N/D','','','Sim, número de IVA válidoAtenção adiantamento','2021-03-22 16:17:45'),
 (32,'WENZHOU HUAQIAO PACKING MACHINE FACTORY','NO.15 GANGFU ROAD, KONGGANG NEW AREA WENZHOU, ZHEJIANG CHINA','','N/D','sales@kunba.biz','Apple Jin','','2021-03-25 22:12:15'),
 (34,'Total Cash Systems CO., LTD','13/F, Hing Lung Commercial Bldg, 68-74 Bonham Strand East, Sheung Wan Hong Kong','','N/D','hiro.usui@tcspos.com','Hiro Usui','','2021-05-18 09:27:19'),
 (43,'RAELMA, SISTEMAS DE ENVASADO Y EMBALAJE, sl','Avda. del Deleite, s/n. - Apartado Correos 150 ARANJUEZ 28300 ARANJUEZ - MADRID -  ESPANHA','00 34 91 8910880','N/D','raelmacomercial@gmail.com','','(M)(NS-15/11 N/FAX=90D)22/5/02=30, 60 e 90dias','2021-03-22 16:17:45'),
 (78,'L.F. S.p.A.','VIA F.PARRI, 111 -TORRE DEL MORO CESANA 47023 CESANA (FORLI)        ITALIA','00390547341111','N/D','','','Para calcular PVP 2.55 SF 09-11-2015...Login: 1004204Password: RIcamBI_2019','2021-03-22 16:17:45'),
 (80,'FABORY PORTUGAL - PARAFUSARIA E MONTAGEM INDU','R RODRIGO SARMENTO DE BEIRES - FOROS DA CATRAPONA N 18 ALDEIA DE PAIO PIRES 2840-068 ALDEIA DE PAIO PIRES','212135900','N/D','vendas.portugal@fabory.com','','','2021-03-22 16:17:45'),
 (88,'GRUPO EPELSA, S.L.','C/ Albasanz, 6 y 8 28037 Madrid 28037 Madrid - ESPANHA','0034936546212','N/D','info@epel-ind.com','','(MJ)LC 90 e 120 dias passou a ser LC a 90,120 e 150dias-----------------------------------------(MJ) Falta lç NC € 9.732.34 Este valor nao e para pagar -----------------------------------------','2021-03-22 16:17:45'),
 (99,'PFM SPA Packaging Machinery SPA','Via Pasubio, 49 TORREBELVICINO 36036 Torrebelvicino(VI) - ITALIA','0039 044 557 0110','N/D','pfm@pfm.it','','(M)','2021-03-22 16:17:45'),
 (100,'Tecnoval Mecanizados CNC, S.L.','C/ Garbi, 7 (Pol. Ind. Pont Del Princep) Vilamalla 17469','0034972527428','N/D','comercial@tecnovalcnc.com','Juanjo','','2021-03-22 16:17:45'),
 (106,'RHENINGHAUS S.r.l.','Strada del Cascinotto 139/39L TORINO 10156 TORINO- ITALIA','0039011 2237514','N/D','info@rheninghaus.com','','A nova morada funciona a partir de 23.07','2021-03-22 16:17:45'),
 (138,'ITALIANA MACCHINE','Via Dell´Artigianato, 26   47030 S. Mauro Pascoli (FO) - ITALIA','0039-0541-930010','N/D','','','1 ENCª = Pº582/02 CH ANT2 ENCª = Pº30336  CH ANT TAMBEM INT. ANABELA','2021-03-22 16:17:45'),
 (141,'C R M, S.P.A.','','0','N/D','','','','2021-03-22 16:17:45'),
 (142,'OVARPACK - Embalagens, S.A.','Zona Industrial de Ovar - Apt.147 Ovar 3880-184 OVAR','256-579170','N/D','ovar.portugal@linpac.com','','puxar adiantamento','2021-03-22 16:17:45'),
 (144,'MINIPACK-TORRE, SPA','Via Provinciale, 54 . 24044 Dalmine (BG) - ITALIA','0039-035-563525','N/D','.','','Valor mín. enc: 100€','2021-03-22 16:17:45'),
 (152,'TECNOPACK - Com. e Assit. Téc. Equip. P/Emb.,','RUA MANUEL VALENTIM DUARTE Nº 10  2665-562  VENDA DO PINHEIRO','21 966 36 19','N/D','geral@tecnopack.pt','','cargas / descargas: RUA MANUEL VALENTIM DUARTE Nº 102665-562 VENDA DO PINHEIRO(Por trás do Armazém da Schweppes)','2021-03-22 16:17:45'),
 (166,'RST - Construtora de Máquinas e Acessórios, S','Zona Industrial de Aveiro Esgueira - Apartado 3136 3801-101 AVEIRO','234 300 020','N/D','geral@rst.pt','','','2021-03-22 16:17:45'),
 (173,'REXEL - Distribuição de Material Eléctrico, S','Av.D.João II,lote 1.12.02,Torre B,Piso 14º Edificio Adamastor-Parque das Nações 1990-077 LISBOA','21 8937224','N/D','ralexandre@rexel.pt','Alexandre( AVEIRO)','desc.2.5% p.pdesc.1.5% 30 diasATENÇÂO DESP.A.CARNEIROJÀ PG.24,42','2021-03-22 16:17:45'),
 (187,'BALANÇAS MARQUES de JOSÉ PIMENTA MARQUES, LDA','Parque Ind. de Celeirós (2ª Fase) APARTADO 2376 4701-905 BRAGA','253-672100','N/D','bm@balancasmarques.pt','','Puxar adiantamento','2021-03-22 16:17:45'),
 (269,'RIVERPACKING S.L.','Avª del Deleite s/n - Nave 1 - Apartado de Correos 247  28300 ARANJUEZ (MADRID) - ESPANHA','0034918910971','N/D','riverpacking@ribernet.es','','(M)Condiçoes de pagamento ?????','2021-03-22 16:17:45'),
 (290,'MEDOC, SA','POLIGONO CANTABRIA I - C/ SOTO GALO 11-13 LOGRONO 26006 LOGRONO      ESPANHA','34 941 247 600','N/D','medoc@medocsa.com','','','2021-03-22 16:17:45'),
 (291,'MOFITEX - Sousa & Fernandes, Lda.','Rua da Cancela Velha, Lote 41 Apartado 3128 4401-801 AVINTES','227860950','N/D','','','Este fornecedor, é também cliente. TODOS OS PAGAMENTOS APARTIR DE FEV/05,SERÃO À ORDEM DO BCP, SA (CESSIONÁRIO DE CRÉDITOS)Av.José Malhoa , Lt.1682,3ºAndar1099-007 LISBOA','2021-03-22 16:17:45'),
 (303,'ALBIPACK - Sist.e Tecnol.de Embalagem, Lda','Arm. nº4 - Alagôa - Apt.3015  3750-301 ÁGUEDA','234648274','N/D','','','ADIANTAMENTO € 3230,00','2021-03-22 16:17:45'),
 (304,'EQUICALCULUM - Com. de Mat. de Escritório, Ld','Rua Calçada de Real, nº10  4715 - 057 BRAGA','253693363','N/D','comercial@equicalculum.pt','','','2021-03-22 16:17:45'),
 (311,'TESTRANA - Balanças e Máquinas, Lda.','Rua Adelina Oliveira Cavadas Dias, Nº. 83 Zona Industrial de Milheirós 4475-301 MAIA','229 606 076/7','N/D','','Sr. Carlos Conçalves','','2021-03-22 16:17:45'),
 (312,'PLÁSTICOS MACAR - Indústria de Plásticos, Lda','Apartado 36  4784-908 AREIAS- STº TIRSO','252830780','N/D','comercial@plasticosmacar.pt','','','2021-03-22 16:17:45'),
 (313,'EUROPESAGEM - Com. Int. de Balanças, Lda.','Rua Srª. do Carmo, N.º 12  4700-613 CELEIRÓS BRG','253287583','N/D','','','','2021-03-22 16:17:45'),
 (323,'ABM COMPANY SRL','Via RHO, 6 LAINATE 20020 LAINATE (MILANO)               ITALIA','0039029373781','N/D','abm@abmcompany.com','','(MJ)17/7/3 Era saque a 60dias, passou para ch a 60diasSr.Fernando=ped.pagtºs21/6/2005 cond pagt coloquei-90 dias dado que nas facturas aparece assim.','2021-03-22 16:17:45'),
 (333,'JOSÉ PINTO - Mobiliário Publicitário','Z.I.Maia I - Sector 2 - Lt.124 -  Arm.5  4470-MOREIRA DA MAIA','229411456','N/D','geral@josecpinto.com','','MM: passou a UnipessoalLda','2021-03-22 16:17:45'),
 (338,'RECHEIO - Cash & Carry, SA','Rua Actor António Silva, nº7   1649-033 LISBOA','234915080 (aveiro )','N/D','','','','2021-03-22 16:17:45'),
 (395,'COMECA - IMPORTAÇÃO E EXPORTAÇÃO, S.A.','Edificio Comeca - Bairro do Campo da Bola Albarraque 2635-022 RIO DE MOURO','219155600','N/D','horeca@comeca.pt (preços e manuais)','','DESCONTO DE 4% A PºPº com 30 dias de resumo de facturasPassword do site em anexo','2021-03-22 16:17:45'),
 (404,'SUDIMP, SL','Poligono Ind. Store C/B Nave 17 - 41008 Sevilla Espanha','0','N/D','','','','2021-03-22 16:17:45'),
 (418,'NILMA, S.P.A.','VIa E. Zacconi, 24A   43122 PARMA - ITÁLIA','0039-0521778828','N/D','export@nilma.it','','(m) 31/3/3-1ºforn - ch anticipado      2º   \"    - 60dias ???Desconto peças 30% + custo embalagem','2021-03-22 16:17:45'),
 (474,'FITEMBAL - Mat.Máq.e Sist.de Embalagem,Lda','Castelo da Maia - Apt.2024  4471-908 MAIA Codex','229825678/9','N/D','fitembal@fitembal.pt','','','2021-03-22 16:17:45'),
 (496,'MIVEG - Werksvertretungen oHG','Gewerbering 4   D-91341 Roettenbach - Alemanha','0049 9195 9411 0','N/D','info@miveg.de','','','2021-03-22 16:17:45'),
 (514,'TERMOFILM - Embalagens Tecnicas, Lda.','Rua de Oliveira, nº 518 . 4770-316 Landim - V.N. FAMALICÃO','252-933769','N/D','geral@termofilm.pt','','ESTE FORNECEDOR TB É CLIENTE. NAO FAZER ENCONTRO DE CONTAS.(14/11/2011)','2021-03-22 16:17:45'),
 (524,'GANDUS Saldatrici S.R.L.','Via Milano, 5 . 20010 CORNAREDO - ITÁLIA','0039 029 31941','N/D','info@gandus.it','','','2021-03-22 16:17:45'),
 (526,'SAMMIC, S.L.','Basarte - Aptdo 77 20720-Azkoitia - (Gipuzkoa) ESPANHA','21 41 503 16','N/D','','','Novo NC a partir de 01.07.2007 :ES-B20869152. (antigo numero B95076592)NOVA MORADA A PARTIRDE 01.08.2007.antiga morada (C/ Atxubiaga, 14-20730 Azpeitia (Gipuzkoa) 17/05 - TRF p/Lisboa; 18/11 Morada Lx: Rua Pereira de Melo, 24A /2795-082 L','2021-03-22 16:17:45'),
 (533,'CRETEL NV','Gentsesteenweg, 77A  9900 EEKLO - BELGIUM','0032 937 69595','N/D','info@cretel.com','Patrick Robert','-1ºFORN 15/5/6=TR30d-','2021-03-22 16:17:45'),
 (536,'ANIMO B.V.','Dr. A.F. Philipsweg 47 9403 AD Assen - P.O.Box 71 9400 AB ASSEN - HOLANDA','0031 592 376376','N/D','svandenberg@animo.nl','','1ºforn 01/5/2006 TR ANT               PUXAR ADIANTAMENTOVAT-NL006921619B01Login MYANIMO:thumer@mirandaeserra.pt_K6Tc?Ww','2021-03-22 16:17:45'),
 (537,'KLOCKNER PENTAPLAST GmbH','Postfach 11 65   D-56401 Montabaur - GERMANY','0049 2602 915-0','N/D','kpinfo@kpfilms.com','','','2021-03-22 16:17:45'),
 (541,'TBS, SRL','Via I° Maggio, 3/11  40057 Quarto Inferiore (BO) - ITALIA','0039-051-6061243','N/D','','','MJ-1ªENC 2/2/4 Pº40051 30diasSAP nº 619','2021-03-22 16:17:45'),
 (543,'BOULANGER SA','8 Rue des Rosiers  53480 VAIGES - FRANCE','0033 243 905092','N/D','arlette.boulanger2@wanadoo.fr','','Próximas encomendas: Pagamento a 60 dias-MRosa (2006-07-21)','2021-03-22 16:17:45'),
 (544,'AB MUNKFORSSAGAR','Fabriksvagen 4 BOX 124 SE 684 23 MUNKFORS - SWEDEN','0046 563 53500','N/D','mfsorder@munkfors.com','anna.hallberg@munkfors.com','MJ 19/9/06 cond pagt passou de ANT para 60dias','2021-03-22 16:17:45'),
 (545,'RISCO S.P.A.','Via Della Statistica, 2 P.O.Box 130 36016 THIENE (VICENZA) - ITÁLIA','0039 0445 385911','N/D','risco@risco.it','','MJ1ºfor 90dias PS','2021-03-22 16:17:45'),
 (553,'HEVEL VACUUM BV','Sluispolderweg, 44 B  1505 HK ZAANDAM - HOLANDA','0031 075 617 7637','N/D','sales@hevel.nl','','MJ1ºforn 07/06/06 TR ANT','2021-03-22 16:17:45'),
 (557,'GRUPOSISTEMAS - Sist. e Soluções Informáticas','Rua Capitães de Abril, 31 A Alfornelos - Colina do Sol 2650-351 AMADORA','21 476 74 20/2','N/D','gruposistemas@gruposistemas.pt','','','2021-03-22 16:17:45'),
 (558,'LABAU TECHNOLOGY CORP.','3F-3, No. 125, Ln. 235, BauChiau Rd. HsinTien 231 Taipei, Taiwan','00886-2-89191371','N/D','wayne@labau.com.tw','Wayne Chang','','2021-03-22 16:17:45'),
 (559,'LAME ITALIA S.R.L.','Via Kennedy, 60 Valla Di Riese Pio X 31030 VALLA´DI RIESE PIO X (TV) - ITÁLIA','0039 0423 746104 -720 383','N/D','brunosalvador@libero.it / info@salvinox.com','','MRosa 29/6/6 - ex Bruno SalvadorFérias 2016: 06/08 - 28/08','2021-03-22 16:17:45'),
 (567,'SARO Gastro-Products GmbH','Postfach 10 06 38 46428 Emmerich ALEMANHA','0049-2822-3014 / 9258-0','N/D','info@saro.de / walter.spangenberg@saro.de','Walter Spangenberg','PUXAR ADIANTAMENTO','2021-03-22 16:17:45'),
 (573,'Marque TDI - Tecnologias de Codificação, S.A.','Zona Industrial da Maia - Sector X Complexo Empresarial Soconorte - Armazém L 4475-249 MAIA','229 866 660','N/D','mail@marquetdi.pt','','','2021-03-22 16:17:45'),
 (574,'INDUSTRIAS GASER, S.A.','CRTA. BESCANO, 15 POL. TORRE MIRONA 17190 SALT (GIRONA) - ESPANHA','0034 972 236572','N/D','admin@gaser.com','','','2021-03-22 16:17:45'),
 (584,'DE KONINGH FOOD EQUIPMENT','6827 BT Arnhem Postbus 5023 6802 EA Arnhem','0031 026 384 9084','N/D','info@dekoningh.nl','Marc Bakermans','','2021-03-22 16:17:45'),
 (586,'Sersounox - Equipamentos P/ Indústria Aliment','AV. PRINCIPAL, N.º 5 CASAIS DE S. MARTINHO 2590-429 Sobral de Monte Agraço','219757618','N/D','sersounox@sapo.pt','','','2021-03-22 16:17:45'),
 (593,'COLDPARTNER - Montagem e Assist.Equipt.Frio,L','Rua Engº.Frederico Ulrich nº 2787 Zona Ind. Maia 4470-605 Moreira - Maia','224 854 800','N/D','fernandofonseca@colpartner.com','Sr. Vieira','EX-EDERIO VIEIRA & VIEIRA - RICOTEL','2021-03-22 16:17:45'),
 (596,'F.FONSECA, S.A.','Estrada de Taboeira 87/89 Esgueira Apartado 3003 3801-997 AVEIRO','234303900','N/D','ffonseca@ffonseca.com','','','2021-03-22 16:17:45'),
 (597,'MODELO CONTINENTE HIPERMERCADOS, S.A.','Edifício Sonae Rua João Mendonça, 505 4464-503 SENHORA DA HORA','229532665','N/D','','','','2021-03-22 16:17:45'),
 (603,'SOVE - Soc. Vedantes e Máquinas, S.A.','Rua do Outeiro, 1302/1324 . 4470-208 MAIA','229 478 500','N/D','sove.maia@sove.pt','','','2021-03-22 16:17:45'),
 (611,'SEWOO TECH CO. LTD.','689-20 Kumjung-Dong, Kunpo-Si Kyungki-Do Coreia','0082-31-4598200','N/D','','','','2021-03-22 16:17:45'),
 (616,'PROTECH SYSTEMS CO., LTD.','Nº 24 LAne 365, Yang Goang Street Nei Hu District, Taipei 114 TAIWAN ROC','00886-2-87511111','N/D','sales@protech.com.tw','','30/4/4-1 FORN Pº40240 TR ANTPUXAR ADIANTAMENTOAR-A aguardar NC USD 670,00(05/01/10)Chang Hwa Commercial Bank','2021-03-22 16:17:45'),
 (619,'S.A.P.  s.r.l.','Via Nobel 7/11/11a - Z.I. 1° Maggio 40064 Ozzano dell Emilia (BO) Italia','0039 051 79 68 72','N/D','business@sapbologna.it','IVANA SCATTOLINI','','2021-03-22 16:17:45'),
 (625,'BERKEL,  S.P.A.','Via F. Olglatti, 12 24143-Milano Itália','0039 0281861','N/D','berkelitalia@berkelinternational.com','','','2021-03-22 16:17:45'),
 (626,'WAH LUEN TYPEWRITER CO.','13/F., Hing Lung Comm, BLDG. 68-74 BONHAM Strand East, Sheung Wan, H.K. HONG KONG','852 25 43 77 36','N/D','wah_luen@ctimail3.com; wlt2231@yahoo.com.hk','','','2021-03-22 16:17:45'),
 (627,'BIRO FRANCE','7 bis, boulevard Robert Thiboust SERRIS 77706 MARNE LA VALLEE CEDEX 4 - FRANCE','0033 01 64 63 35 00','N/D','infos@biro.fr','','','2021-03-22 16:17:45'),
 (628,'ZEBEX INDUSTRIES, INC.','B1-1, No. 207, Section 3, Beisin Road Sindian City, Taipei,231 - Taiwan','+886 2 8913 2598','N/D','mail@zebex.com.tw','','Banco:E Sun Commercial Bank Ltd','2021-03-22 16:17:45'),
 (630,'FLEXOPACK SA','Thessi Tzima, 194 00 KOROPI ATTIKIS GRECIA','0030 210 66 80 000','N/D','FLEXOPACK@FLEXOPACK.GR','','CREDITO € 15.00','2021-03-22 16:17:45'),
 (634,'MBP Srl','Via Fossadone, 85 46043 Castiglione delle Stiviere (Mantova) ITÁLIA','+39-0376/638479','N/D','info@mbp.it','','','2021-03-22 16:17:45'),
 (636,'ITV - INDUSTRIA TECNICA VALENCIANA, SA','POL. IND. LA REVA SECTOR 13 AVDA. HOSTALERS , N.2 46394  RIBA-ROJA  (VALENCIA) - ESPANHA','0034 96 166 75 75','N/D','SALES@ITV.ES','','Pedir dados p/ recolha.','2021-03-22 16:17:45'),
 (637,'NETRONIX, INC.','9F, No. 92, Baojhing Road, Shindian City Taipei, Taiwan, 231. R.O.C Taiwan','886-2-2912-0211 - 19','N/D','-','','','2021-03-22 16:17:45'),
 (639,'GIROPACK - UNIPESSOAL, LDA','RUA JOSÉ ELISIO GONÇALVES CEREJEIRA, 583 VILA NOVA FAMALICÃO 4760-357 CALENDÁRIO','252310994','N/D','','','','2021-03-22 16:17:45'),
 (643,'ELPLAST Co. Ltd','ul. Strazacka 42 44-240 Zory Poland','+48 32 47 30 930','N/D','m.pawelak@elplast.org','','','2021-03-22 16:17:45'),
 (654,'THIELMANN ROTEL  KNOW-HOW AG','Oltnerstrasse 93  CH - 4663 AARBURG - SWITHERLAND','0041 62 791 35 60','N/D','polina.sonntag@juicemaster.ch / office@juicemaster.ch','Polina Sonntag','!!!! Utilizar fornecedoer 1175 para encomendas vindos de Alemanha !!!!','2021-03-22 16:17:45'),
 (655,'SIRMAN SPA','Via dell´industria, 9/11 35010 Pieve di Curtarolo (PD) ITÁLIA','0039-049-9698666','N/D','BARUTTA@SIRMAN.COM; campagnaro@sirman.com','','Para enc. inferiores à 100€ despesas manuseamento de 15€','2021-03-22 16:17:45'),
 (656,'POSLAB TECHONOLOGY CORP.','11F., No 92, Sec. 4, Huanhe E. Rd. Yonghe City - Taipei County 234 TAIWAN R.O.C.','886 2 8942 2501','N/D','SALES@POSLAB.COM.TW','','','2021-03-22 16:17:45'),
 (657,'FIRICH ENTERPRISES CO. LTD','10F, NO.75 SEC.1 HSIN-TAI WU RD, TAIPEI HSIEN, TAIWAN R.O.C.','886 2 26981446','N/D','tiffany.chang@firich.com.tw','','','2021-03-22 16:17:45'),
 (659,'SMC España S.A.','Rua Engº Ferreira Dias, 452 4100-246 PORTO 4100-246 PORTO','226166570','N/D','','','','2021-05-18 14:11:44'),
 (660,'PROVEEDORA HISPANO HOLANDESA, S.A.','C/ DE LES GARRIGUES, 51 POLIGONO MAS BLAU II 08830 - SN BOI DE LLOBREGAT (BARCELONA)','0034 934795800','N/D','PHH@PHH.ES','Sr. Manuel Romero','','2021-03-22 16:17:45'),
 (662,'FIREX SRL - Food Processing Equipment','Z.I. Gresal, 28 32036 Sedico (Belluno) ITALY','+39 0437 852700','N/D','firex@firex.it','','Acessorios 40% SF 2013/05/30CONDIÇÕES PARA CLIENTES: VER INDICAÇOES PEDRO SERRA NO ANEXO.','2021-03-22 16:17:45'),
 (663,'Koneteollisuus Oy','Järvihaantie 5 - P.O.Box 49 01800 Klaukkala - Finland','358 9 878 922 40','N/D','akrenius@koneteollisuus.fi','','Conferir o Nº contrib antes de lançar a compra','2021-03-22 16:17:45'),
 (664,'HOT CLASS SRL','C.DA VALLECUPA 65/B 64010 COLONNELLA TE - ITALIA','39 0861 753523 ou 33','N/D','massimo.granatiero@granatiero.com','','Cliente RST','2021-03-22 16:17:45'),
 (674,'Busch Portugal, Lda','Travessa da Barrosinha, nº 84 -  Fracção B Marco da Raposa / Z.I. EN1 Norte Apartado 3 3750-753  Rasos de Travassô (AGUE','234648070','N/D','pedro.robalo@busch.pt','pedro Robalo','','2021-03-22 16:17:45'),
 (689,'ATB EQUIPMENTS N.V. - SA','Bergensesteenweg, 181 1600- Sint Pieters Leeuw Belgica','0032 2/371.02.20/24 (directo Nicole)','N/D','m.talluto@atb-equipments.be','TALLUTO Michel','','2021-03-22 16:17:45'),
 (702,'PEDRO PORTO - Aparelhos de Pesagem, Lda.','E.N. 10 - Km 136  2696-601 SANTA IRIA DA AZOIA','219566844','N/D','','','E CLIENTESim, número de IVA válido  19/06/2012','2021-03-22 16:17:45'),
 (703,'NTS  INTERNATIONAL CORP.','Honmachi Okamura Bldg. / 1-2-10, Awaza, Nishi-ku Nishi-ku OSAKA, 550-0011 / JAPAN','00816654146529','N/D','ntsjp@pop06.odn.ne.jp','','(m)','2021-03-22 16:17:45'),
 (709,'ETIRÓTULOS LDA','Zona Industrial de Ourém, Nº 74 Casal dos Frades 2435-661 Seiça -Ourém','249544 106','N/D','etirotulos@iol.pt','','','2021-03-22 16:17:45'),
 (713,'Cachapuz - Equipamentos Para Pesagem, Lda','Parque Industrial de Sobreposta Apartado 2012 4701-952 Braga','253 603 480','N/D','','Fernando Agostinho','','2021-03-22 16:17:45'),
 (719,'Ascell Sensor, S. L.','Pol. Industrial Congost - Avda. Congost, 56 - nave 3 . 08760 Martorell – BARCELONA','34 93 776 60 89','N/D','info@ascellsensor.com','','','2021-03-22 16:17:45'),
 (720,'FOSHAN EVERLASTING ENTERPRISE CO., LTD.','31/F Jinghua Bldg, 18 - Jihua Wu Road Foshan Guandong - R. P. China R. P. China','86 (0)20-811 55 138','N/D','lihao@asakifoodmachine.com','','Novo: ANHUAI CO., LTD (893)','2021-03-22 16:17:45'),
 (723,'AVERY BERKEL SAS','36, avenue de l’Europe 95335 DOMONT Cedex FRANCE','33 (0) 825 870 800','N/D','berkel@everyberkel.com','VKoely@averyberkel.com','armazem: NORBERT DENTRESSANGLE / Avery Berkel Batiment I / 3, rue Baranfosse / 60330 LAGNY LE SEC - FranceContact: Nicolas +33 (0) 3 44 60 65 77','2021-03-22 16:17:45'),
 (725,'Comercial de Distribución Silvio 99,S.L.','Poligono Industrial El Pino C/ Pino Mediterráneo,3 41016 Sevilla ESPANHA','34 954 519 888','N/D','admin@eutron.es','','PW site:thumer@mirandaeserra.ptMSERRA2016Deduzir € 25.71 proximo pagamento','2021-03-22 16:17:45'),
 (726,'Fornecedores Diversos Eropa','. . .','.','N/D','.','','','2021-03-22 16:17:45'),
 (727,'Ingram Micro SL','Avda. Maresme (Polígono Almeda) 08940 Cornellá de Llobregat - Barcelona ESPANHA','351-213 976 229','N/D','Eteixeira@ingrammicro.pt','Sr. Luis Martins','Atençaõ ao encomendar pois cobram pedidos minimos (MD)','2021-03-22 16:17:45'),
 (729,'BRAHER INTERNACIONAL,S.A.','Avda. Carlos I, 14-1ºC SAN SEBASTIAN 20011 SAN SEBASTIAN - ESPANHA','0034943465400/0034943470121','N/D','braher@braher.com','','(m)','2021-03-22 16:17:45'),
 (731,'Guangzhou Zonerich Computer Equipments Co. Lt','Floor 4 Office Building - U-Best Industry Zone Nº 17 Xiangshan Road - Luogang GUANGZHOU - CHINE','','N/D','ycb@zonerich.com - Mr. Robbin Yao','','Completar a ficha antes de gravar a compra','2021-03-22 16:17:45'),
 (733,'IKEA PORTUGAL - Móveis e Decoração, Lda.','Av. Óscar Lopes   4450-745 MATOSINHOS','229 980 600','N/D','','','','2021-03-22 16:17:45'),
 (734,'ICB TECNOLOGIE SRL','Via Cavicchione Sopra 102 25011 Calcinato (BS) - ITALY','030 9636002','N/D','stefano.galli@icbtecnologie.com','','','2021-03-22 16:17:45'),
 (735,'HEISEI - System Europe b.v.','Vaart 11 4203cc Gorinchem - Netherlands','0031-(0)183640145','N/D','info@heisei-systems.eu','','','2021-03-22 16:17:45'),
 (738,'UNI-FOOD TECHNIC A/S','Landholmvej 9 9280 Storvorde - Denmark','45 9677 4100','N/D','uft@uni-food.dk','','','2021-03-22 16:17:45'),
 (739,'MS-POS, Lda','Rua Alda Nogueira, nº 14 Monte Abraão 2745-324 Queluz','214304261','N/D','','Pedro','','2021-03-22 16:17:45'),
 (741,'CROBEL - INDÚSTRIA DE ETIQUETAS, LDA','Rua António da Costa Guimarães, nº 3229 Urgeses 4810-491 GUIMARÃES','253 478 340','N/D','geral@crobel.pt','Encomendas','(11/07/2011 MJ abri cliente)','2021-03-22 16:17:45'),
 (742,'Besser Vacuum Ibérica, S.L.','C/Miquel Servet 29-33 Nave 3 Pol.Ind.Bufalvent Manresa 08242 Manresa','0034936424419','N/D','bv.iberica@besservacuum.com','francesca.cancellier@besservam','','2021-04-02 18:40:39'),
 (743,'BESSER VACUUM SRL','Via Casarsa 57 Dignano 33030 Dignano (UD) - Italy','39-0432953097/0432956960','N/D','export@besservacuum.com','francesca.cancellier@besservam','Conferir VAT Nº quando lançarem a 1ª factura25€ despesas de gestão para encomendas inceriores á 250€.','2021-03-22 16:17:45'),
 (744,'MACAP Srl','Via Toniolo 18 Venezia 30030 Maerne di Martellago  Italia','39 041 5030466','N/D','raffaella.caccin@macap.it','','Acessorios 50%+13%  (SF)','2021-03-22 16:17:45'),
 (747,'Mobilscan - Sistemas Informáticos, Lda','Rua Alda Nogueira nº14 Monte Abraão 2745-324 QUELUZ','214304610','N/D','geral@mobilscan.pt','','é cliente nº 1760','2021-03-22 16:17:45'),
 (750,'CARLOS SILVA & DIAS COMPONENTES E SISTEMAS EL','RUA 1 DE MAIO FRAÇÃO F N 201 A ZONA INDUSTRIAL DO ROLIG ESPARGO 4520-115 ESPARGO','229470370','N/D','geral@carlossilvadias.pt','','','2021-03-22 16:17:45'),
 (753,'PLÁSTICOS RETRÁCTILES, S.L.','Polígono Industrial L\'Aguda s/n 25750 TORÀ - LLEIDA - ESPANHA','0034 - 973 473 347','N/D','comercial@plasticosretractiles.com','','','2021-03-22 16:17:45'),
 (757,'SLAYER BLADES, SRL','Via Milano 37 21040 OGGIONA S.STEFANO (VA) - ITALIA ITALIA','0039 0331 739004','N/D','spareparts@rbaricambi.com','','Utente: thumer@mirandaeserra.ptPass: 500842019','2021-03-22 16:17:45'),
 (759,'PACIFIC BUSINESS MACHINE LTD','27M MA TAU WAI ROAD,HUNG HOM, KOWLOON HONG KONG (RPC) REPUBLICA POPULARDA CHINA','852-23302587','N/D','','','','2021-03-22 16:17:45'),
 (761,'Metal Line-Ind. Metalomecanica do Sector Alim','Rua Alto das Mós , nº4 Moitelas - Sapataria 2590-433 Sobral de Monte Agraço','219666076','N/D','metalline@sapo.pt','','','2021-03-22 16:17:45'),
 (765,'APPOSTAR TECHNOLOGY CO. LTD','5F, Nº 88, Minquan Rd., Xindian Dist. New Taipei City 231 Taiwan','886-2-891316168 est. 266','N/D','','','BANCO: E SUN BANK SHINDIAN BRANCHNº 69, MINQUAN RD, SHINDIAN CITYTAIPEI 231TAIWAN','2021-03-22 16:17:45'),
 (766,'ESI - Engenharia, Soluções e Inovação, Lda','Rua dos três caminhos - Parque Industrial de Meães Pavilhao 4 - Meães - Lousado 4760-482 Vila Nova de Famalicão','252-318499','N/D','esigeral@gmail.com','','PT50001800080396656002068(7/3/2019 Ezeq - E CLIENTE fazer ECC)','2021-03-22 16:17:45'),
 (771,'Qingdao Hisense Intelligent Commercial System','YangguangTaiding Tower, No.16A Shandong Road Qingdao,266071 - RPC CHINA','86-532 80901118','N/D','yinwei@hisense.com','','INDUSTRIAL AND COMMERCIAL BANK OF CHINAQINGDAO','2021-03-22 16:17:45'),
 (779,'Combipack - Sistemas e Artigos de Embalagem, ','Zona Industrial Abelheira / Apartado 67 Paços de Brandão 4536-906 Paços de Brandão','227471670','N/D','comercial@combipack.pt','','','2021-03-22 16:17:45'),
 (781,'Facchinetti S. r. l.','Cascina Corte Nuova - Via Case Sparse 14 Fraz. Torrion Quartara 28100 Novara - Italy','0321/455192','N/D','info@facchinettinovara.it','','','2021-03-22 16:17:45'),
 (783,'Casfil - Industria de Plasticos  SA','Rua Ponte da Pinguela, n.º 265  4795-099 Vila das Aves','252 820 100','N/D','casfil@casfil.pt','','','2021-03-22 16:17:45'),
 (786,'Chipcell Unipessoal, Lda','Quinta das Rebelas Rua C nº1G   2830-222 BARREIRO','212 166 410      212 165 157','N/D','info@lookcell.com','','','2021-03-22 16:17:45'),
 (796,'ITALCOR II COMERCIO DE ACESSORIOS INDUSTRIAIS','R CLUBE ATLETICO DE RIO TINTO N S 156 158 RIO TINTO 4435-188 RIO TINTO','224854140','N/D','italcor2comercio@gmail.com','','Empresa associada (Melfinox - 2672)MROSA: PP C/ 3%','2021-03-22 16:17:45'),
 (799,'PARTNER-TECH Europe Gmbh','Fasanenweg 25 D-22145 Hamburg ALEMANHA','004940450635218','N/D','denis.christesen@partner-tech.eu','','ver conta clientePagamentos a partir de 23/03/2012 para Partner:Conta USD: SWIFT: HASPDEHHXXX  IBAN: DE98200505501660137942','2021-03-22 16:17:45'),
 (803,'MARQUES & CARMO, LDA.','Rua da Chapadinha, nº 8 Dornelas 3740-419 Sever do Vouga','256465058 ou 234551191','N/D','geral@marquesecarmo.com; mcarla@netvisao.pt','','','2021-03-22 16:17:45'),
 (805,'ARGON  - COMP. ELECTRICOS E ELECTRONICOS LDA','Rua Noé Pereira, 473   4510-706 Fânzeres','22 466 4200','N/D','geral@argon.pt','','','2021-03-22 16:17:45'),
 (806,'Costa, Leal e Victor – Electrotecnia – Pneumá','Rua Augusto Lessa, 269  4200-100 PORTO','225 508 520','N/D','clv@clv.pt','','','2021-03-22 16:17:45'),
 (807,'Giorik Spa','Via Cal Longa, 45 32030 PADERNO (BL) - Itália','0039-0437-807200','N/D','sales@giorik.com','','','2021-03-22 16:17:45'),
 (816,'LUIS SANCHEZ & FILHOS, LDA','Av. dos Combatentes, 80 Abrunheira 2710-034 SINTRA','21-9255500','N/D','info@luissanchez.net','','','2021-03-22 16:17:45'),
 (820,'Fast and Easy - Autom.e Prog.Informática,Lda','Rua do Salgueiral nº 38  - Estabelecimento nº4 Porto 4200-476 Porto','225094433','N/D','luz.loureiro@fastandeasy.pt.','','E CLIENTE 7089O Miguel Gomes saiu da empresas em 30/09/15','2021-03-22 16:17:45'),
 (829,'SINOCAN INTERNATIONAL TRADE CO LTD','Peking University Founder Shiyan Science Park Bao´an, Shenzhen, China 518108 CHINA','+86-75526745493','N/D','marketing@icdpc.com.cn','','','2021-03-22 16:17:45'),
 (833,'GIROPES S.L.','POL. IND. EMPORDA INTERNACIONAL 17469 VILAMALLA GIRONA - ESPANHA 17469 VILAMALLA GIRONA','+34 972 527 212;','N/D','fgou@giropes.com;lgomez@giropes.com','Sra. Lidia Gómez','','2021-03-22 16:17:45'),
 (835,'Lorapack Srl','Via dei Quartieri, 45 . 36016 - THIENE (VI) - ITALIA','0445-386859','N/D','info@lorapack.com','','Sede operativa: Via Abruzzi, 50 - 36016 THIENE (VI)','2021-03-22 16:17:45'),
 (836,'SAVEMAH HOSTELERIA S.L.','C/ Aviación, 24 (Pol. Industrial) 46940 MANISES (Valencia) ESPANHA','0034 961 526 380','N/D','savemah@savemah.com','Antonio Cordon','ASSOCIADO PAYMA - VER INST. ANEXO(26/11/2012 MM quando fizerem encª ver se e pª pedir seguro)','2021-03-22 16:17:45'),
 (838,'MODULAR Professional S.r.l.','Via Palù, 93 San Vendemiano 31020 San Vendemiano (TV) - ITALY','39 0438 7714','N/D','modular@modular.it','Ivana Corrocher','Claims :MR. SANDRO MORETTI - Service Managerservice@modular.itMS. FEDERICA DE NARDI - Spare parts departmentSpare.parts@modular.it(LOGIN: username: mrosa@mirandaeserra.pt / pass: 500842019)','2021-03-22 16:17:45'),
 (847,'ABO Srl','Via Ronchetti n. 55 21040 Oggiona S. Stefano (Varese) - ITALIA','0039 0331-217169/212698','N/D','abo@abo.it','Armando Franceschina','','2021-03-22 16:17:45'),
 (851,'Quimeracristal, Plásticos e Maquinaria, Lda','Av. Sá Carneiro, 171 V. N. Famalicão 4470-718 Telhado','252087363','N/D','quimeracristal.geral@gmail.com','','','2021-03-22 16:17:45'),
 (860,'AMB Food Tech S.r.l.','Via della Tecnica, 57 (Località Cicogna) S. Lazzaro di Savena 40068 S. Lazzaro di Savena (BO) -  ITALIA','0039 051 6256555/6256802','N/D','info@ambfoodtech.com','Annalisa Guzzini','(m)31/5/4 PECAS 30DIAS20/9/5 60 dias futuramente 90 dias??? aguardamos resposta do fornecedor.Ex 12998Debitam sempre 20€ embalagem/manuseamento nas enc. acessórios','2021-03-22 16:17:45'),
 (863,'Vizelpas - Comércio de Artigos Plásticos, Lda','RUA ILHA DOS AMORES,Nº 335 - APT.265 APARTADO 265 4815-674 VIZELA','253566600','N/D','geral@vizelpas.pt','','(08/1/2015 MJ - e cliente 5556)','2021-03-22 16:17:45'),
 (870,'ATLAS MACCHINE ALIMENTARI S.R.L.','Via L. Pierobon, 15 LIMENA (PD) 35010 LIMENA (PD)  -   ITALIA','00 39 049 7662511/7662520','N/D','info@atlasfe.com','','FALAR COM CELIA ANTES DE DE PAGAR(m) Ped pagt = Sr MarioVer cliente RST...Associada FELMAC (780) trata enc. + facturação/pagamento formadoras hamburguer. Enviar encomenda para a ATLAS - ex F.I.A (3020)','2021-03-22 16:17:45'),
 (873,'EQUIPA ALIMENTAR II -  EQUIP. IND. ALIMENTAR,','AV. DE PORTUGAL, Nº 13 POVOA DA GALEGA 2665-356 MILHARADO','219758610','N/D','','','','2021-03-22 16:17:45'),
 (876,'ETIGUI ETIQUETAS GUIMARAES UNIPESSOAL LDA','Largo de Camões - Pavilhão B 4805-019 Brito - Guimarães 4805-019 Brito - Guimarães','253570733 - 253570734','N/D','etigui@mail.telepac.pt','','IVA NORMAL MENSAL POR OPÇÃO 18/11/13','2021-03-22 16:17:45'),
 (881,'G.E.V. GmbH','Gadastrasse 4 Neuried b. München 85232 Bergkirchen','00 49 (0) 8142 6522-626','N/D','export2@gev-online.com','','Gev_Online Login:36219 Password:RIcamBI_2019Compras Superiores a 400€ UPS EXPRESSCompras Normais UPS standar 18€ até 32kg.Ricambi (UPS)+-(2-4) dias custo*2.55','2021-03-22 16:17:45'),
 (884,'NOAW SRL','VIA COLOMBERA, 27 21048 SOLBIATE ARNO (VA) ITALY','0039 0331 219723','N/D','noaw@noaw.it','','','2021-03-22 16:17:45'),
 (888,'RONOX - Ind. Equip. Aço Inox, Lda.','NEM - Nucleo Empresarial de Mafra Av.Dr. Francisco Sá Carneiro - Pavilhão 21 2460-486 MAFRA','261-816900','N/D','','','','2021-03-22 16:17:45'),
 (896,'Dornow Food Technology GmbH','Willstätterstr 12 D-40549 Düsseldorf - Germany','49-211527060','N/D','office@dornow.de','','','2021-03-22 16:17:45'),
 (905,'Codimarc - Codificação, Marcação e Etiquetage','Apartado 1038   4765 - 902 Delães','252 932 753 / 4 / 5','N/D','geral@codimarc.pt','','PUXAR ADIANTAMENTOInstalaçoes: Pavilhões Rio Ave, B-05 /  Lugar da Cerqueda / 4765 - 701  -  Oliveira S. Mateus','2021-03-22 16:17:45'),
 (907,'Xiamen Hanin Electronic Technology Co. Ltd','Room 305A, Angye Building, Pioneering Park Torch High-Tech Zone - Xiamen - Chine (RPC)','86-592-5885993','N/D','-','','INDUSTRIAL BANK CO., LTDXIAMEN, FUJIAN, CHINA','2021-03-22 16:17:45'),
 (908,'Ambifood, Lda','Rua Dominguez Alvarez, nr. 44, s. 416  4150-801 Porto','229.962.069','N/D','geral@ambifood.com','Artur Melo','','2021-03-22 16:17:45'),
 (909,'EMPERO ENDUSTRIYEL MUTFAK EKIP. PAZARLAMA A.S','Konya Organize Sanayi Bölgesi Büyük Kayack Mah. 9 Sokak N.20 SELÇUKLU/KONYA TURQUIA','+90 332 444 1560','N/D','empero@empero.com.tr/factory@empero.com.tr','','É cliente','2021-03-22 16:17:45'),
 (911,'J. Montenegro, S. A.','Parque Industrial de Guimarães Pavilhão F1 2ª fase - Ponte 4805-602 Guimarães','253 479 400','N/D','guimaraes@jmontenegro.pt','','Puxar adiantamento','2021-03-22 16:17:45'),
 (916,'Roser Construcciones Metalicas SA','Ctra. Girona a Sant Feliu, 369 17244 CASSA DE LA SELVA (GIRONA) Espanha','34 972463434','N/D','','Gloria Segura','','2021-03-22 16:17:45'),
 (917,'Poindus Systems Corp','5F, No. 59, Lane 77, Xing-Ai Road, Neihu District Taipei City 114, Taiwan (RPC)','886-2-7721-4688','N/D','.','','Puxar adiantamento','2021-03-22 16:17:45'),
 (920,'Hacona Packaging- Technology Kft','Újszász u. 45/B. building \"T\" 1165 Budapest Hungary','36 (1)  401-3030','N/D','mail@hacona.com','','','2021-03-22 16:17:45'),
 (921,'I-Mobile Technology Corporation','3F Alley 15 Lane 120 Sec. 1 Neihu Road, Neihu District Taipei, Taiwan','886-2-26562015','N/D','','','','2021-03-22 16:17:45'),
 (923,'REALBOARD, Lda','Rua José Duarte Lexim, Armazém C   2675-393 ODIVELAS','219 387 259','N/D','info@realboard.pt','Duarte Costa','Cliente nº 2618Sim, número de IVA válido','2021-03-22 16:17:45'),
 (927,'S.A.R GROUP S.R.L.','Via Belvedere 24 21020 Galliate Lombardo (VA) Itália','0039 0332 949116','N/D','sar@sarvarese.com','','= ex fornecedor nº  453','2021-03-22 16:17:45'),
 (928,'Detectronic A/S','Roejbaekvej 3 DK-9640 Farso- Denmark','+45 96663060','N/D','sales@detectronic.dk','Mette Frank','(FAZER ECC COM C/6666)','2021-03-22 16:17:45'),
 (929,'Biomicro-Ar Comprimido,Processo e Tecnologias','Rua Manuel Assunção Falcão, 108 - A.24 Avioso 4475-041 Avioso Maia','22 974 2554 / 229 749 106','N/D','geral@biomicro.eu','','','2021-03-22 16:17:45'),
 (938,'Ishida Co., Ltd.','75 Nishikujou Higashihieijouchou, Minami-ku, Kyoto GRAND KYOTO 3F, 601-8438, Japan GRAND KYOTO 3F, 601-8438, Japan','81-75-693-7148','N/D','arai@ishida.co.jp','','','2021-03-22 16:17:45'),
 (940,'Bolloré Division Films Plastiques','Odet Ergué Gabéric  29 556 Quimper Cedex 9 - FRANCE','+33 (0)2 98 66 72 00','N/D','','','','2021-03-22 16:17:45'),
 (941,'AlemPack Comércio de Cons. ind. Alimentar Lda','Zona Industrial da Boa-Fé Rua do Campo Maior, Lote 11 - Aptdo 166 7350-901 ELVAS','+351 268637530','N/D','geral@alempack.pt','','','2021-03-22 16:17:45'),
 (945,'MHS Schneidetechnik GmbH','Im Deboldsacker 6 Abstatt D-74232 Abstatt ALEMANHA','+49 (0 70 62) 91 08 40','N/D','DVandenhove@mhs-schneidetechnik.de','','(é cliente)','2021-03-22 16:17:45'),
 (948,'AVERY WEIGH-TRONIX','Foundry Lane, Smethwick West Midlands, B66 2LP INGLATERRA','(44)(0)121 558 1112','N/D','sales@brecknellscales.co.uk;industrialexport@awtxglobal.com.','','','2021-03-22 16:17:45'),
 (949,'Gebr. Graef GmbH & Co. KG','Donnerfeld 6 Industriegebiet Bergheim D-59757 Arnsberg       ALEMANHA','+49 (0) 29 32 / 97 03 - 484','N/D','silke.oeliden@graef.de','','2% desconto - 60 dias','2021-03-22 16:17:45'),
 (951,'IFOOMA International Food Machines GmbH','Georg-Wössner-Ring 8   72172 Sulz am Neckar      ALEMANHA','+49-7454-9976626','N/D','simone.pfau@ifooma.com','','','2021-03-22 16:17:45'),
 (958,'FOODLOGISTIK - Fleischereimaschinen GmbH','Adolph-Kolping-Strasse 15   D-17034 Neubrandenburg  ALEMANHA','+49 - 395 - 77 99 - 139','N/D','thomas.wuithschick@foodlogistik.de','Thomas Wuithschick','','2021-03-22 16:17:45'),
 (960,'QUALIAÇO - Fab. Móveis em Aço, Unipessoal, Ld','Rua Nova das Cancelas, nº 8 S. Pedro de Fins 4425-531 S. Pedro Fins','224897505/6','N/D','qualiaco.lda@gmail.com','','20-07-2010 mm - pagamentos a favor de Banco Popular Factoring.Ver sempre com o fornecedor...------------------------IVA MENS. VERIFICADO EM 13/01/2011','2021-03-22 16:17:45'),
 (966,'BLUESTAR EUROPE DISTRIBUTION BV','Koninginnegracht, 19 Den Haag 2514 AB Den Haag - The Netherlands','','N/D','ar-eu@eu.bluestarinc.com','','Ex- IDSYS S.L  (1162)','2021-03-22 16:17:45'),
 (970,'MUNDINOX - Fabrico de Equip. Hot., Lda.','Z. Ind. Maia 1 - Sector VII R. Delfim Ferreira, LT 109, nº240 4470-436 MOREIRA DA MAIA','229 441 678','N/D','geral@mundinox.com','','Empresa associada (Melfinox - 2672)MROSA: PP C/ 3%','2021-03-22 16:17:45'),
 (972,'GUANGZHOU ETON ELECTROMECHANICAL CO., LTD','No. 42 , Dabu Road , Xinhua Street Huadu District,Guangzhou, China Republica Popular da China','0086 20-86410351','N/D','sales2@gz-eton.com','','','2021-03-22 16:17:45'),
 (973,'Impeco sp. z o.o. sp. komandytowa','ul. Bacciarellego 54, Wroclaw 51-649 Wroclaw  - POLAND','+48 515 195 625','N/D','a.twardowski@impeco.pl','','','2021-03-22 16:17:45'),
 (975,'TECNICAL INTEGRA','Pol.Ind.LesFerreríes,Camíde Riudellots,t -3 Campllong 17459 Campllong (Girona) - ESPANHA','+34 972 47 88 88','N/D','tecnical@tecnicalintegra.es','','','2021-03-22 16:17:45'),
 (976,'FAMA INDUSTRIE s.r.l.','Via Altobelli, 39 - Zona Artigianale Rimini Sud RIMINI 47900 RIMINI - ITALIA','00390541388222','N/D','fama@famaindustrie.com','','(M)CH  RECEPCAO/TRANSITARIO16/7/02-ch prog. pª 60dias25/2/03-ch  ant prog pª60dias mesmo para valorespequenos(ver pº30112)--- 2004 ch ant prog a 60dias ---23/2/05 TR A 60dias acacio a confirmar????','2021-03-22 16:17:45'),
 (979,'Longshine Technologie GmbH','An der Strusbek 9   DE-22926 Ahrensburg ALEMANHA','+49 (0) 4102-4922-0','N/D','sales@longshine.de','','Tomar atenção aos dados bancários para USD:USD: IBAN:  DE07200800000721137501','2021-03-22 16:17:45'),
 (980,'Indiana Indústria e Com. de Prod. Alimentício','Rua Bom Jesus, 32 Agua Rasa São Paulo/SP - Brasil CEP: 03344-00','+55 11 3636-9090','N/D','indiana@maquindiana@.com.br','','','2021-03-22 16:17:45'),
 (985,'FAM B.V.','Neerveld 2 B-2550 Kontich - Belgium','32-34509220','N/D','francisco.carbonell@fam-iberica.com','Francisco Carbonell','','2021-03-22 16:17:45'),
 (987,'INANISPACK, LDA','Rua de Oliveira, nº 518   4770-316 Landim - V.N. FAMALICÃO','252-933769','N/D','geral@termofilm.pt','','= Termofilme (514)','2021-03-22 16:17:45'),
 (990,'ULTRA-CONTROLO - Projectos Industriais, Lda.','Parque Industrial Quinta Lavi - Armz. 8 Abrunheira 2710-089 SINTRA','219154350','N/D','sales@ultra-controlo.com','','Enquadramento em IVA NORMAL MENSAL','2021-03-22 16:17:45'),
 (991,'STROJPLAST, SEMIC, D.O.O.','MOVERNA VAS 003, MOVERNA VAS 8333 SEMIC Eslovenia','386 735 680 60','N/D','info@strojplast.si','Gorazd Reberšak','','2021-03-22 16:17:45'),
 (993,'Vakona GmbH','Industriestr. 35 D - 49536 Lienen Alemanha','49 5483 722 828-15','N/D','c.verfarth@vakona.de','Christoph Verfarth','','2021-03-22 16:17:45'),
 (994,'GIMENEZ TODO PARA LA INDUSTRIA CARNICA, S.L.','AV LOS CASTILLOS ,1028 POL.IND.SAN JOSÉ DE VALDERAS 28918 LEGANES (MADRID) - ESPANNHA','914880804','N/D','gimenezmaq@gimenezmaq.com','Jacinto Martinez','Fábrica:SUMINISTROS A.LORENZO BARROSO, SAPol. Ind. El Cros | C/ Del Torrent de Madà, s/n Nave 1ES 08310 Argentona (Barcelona)','2021-03-22 16:17:45'),
 (1000,'Fornecedores diversos','. . 0000-000','.','N/D','.','','','2021-03-22 16:17:45'),
 (1015,'HFE VACUUM SYSTEMS','P.O. BOX 2261 Industrial Parc \"De Brand\" 5202 CG s-Hertogenbosch - HOLANDA','0031-73-6271271','N/D','rnagtzaam@hfe.nl  +  rverschuren@hfe.nl','Mr. Arno Ravestein','Eenc. peças para:  info@henkovac.com;service@hencovac.com;','2021-03-22 16:17:45'),
 (1055,'ITALIANPACK S.r.l.','Via Al Bassone, 30 22100 COMO (CO) ITÁLIA','0039-031-888011','N/D','amministrazione@yang.it','','Pagamento é sempre 60/90 dias menos para os moldes que é antecipado (tem de se puxar pagtº antecipado - indicação MM 23/11/2007)AR 15% adiantado','2021-03-22 16:17:45'),
 (1056,'ILPRA Spa','Via Mattei, 21/23 Mortara 2703-600 - Italy','003903842905','N/D','ilpra@ilpra.com','','','2021-05-18 09:27:19'),
 (1061,'UNICORBAL - Sistemas de Pesagem Corte, Lda.','Rua Zona Industrial, nº 493   4525-513 Vila Maior','220 814 174','N/D','geral@unicorbal.com','','','2021-03-22 16:17:45'),
 (1100,'IMAGENS SOLTAS - MEIOS PUBLICITÁRIOS, LDA','Travessa Amadeu Costa Lote 4 Zona Industrial da Maia- Sector I 4475-192 MAIA MAIA','224056602','N/D','geral@mirandaeserra.pt','','Fornecedor de Cartões de Visita','2021-03-22 16:17:45'),
 (1101,'PAPERTECHLDA','VIA ADELINO AMARO DA COSTA LOTE 5 MOREIRA 4470-557 MAIA','','N/D','geral@papaertech.pt','','Fornecedor do PHC','2021-03-22 16:17:45'),
 (1102,'DMNS - DOMINIOS S A','PARQUE MULTIUSOS AREAL GORDO LOTE 3 A FARO 8005-409 FARO','210081081','N/D','geral@dominios.pt','','Fornecedor do PHC','2021-03-22 16:17:45'),
 (1103,'REVTIL- CONSULTORIA E GESTÃO, S.A.','RUA JOÃO DE DEUS Nº 6 - 6º SALA 601 PORTO 4100-456 PORTO','226007284','N/D','revtil@zonmail.pt','Dr. Soares','Serviços de Contabilidade.','2021-03-22 16:17:45'),
 (1104,'RESIDENCIAL PUMA LDA','R DAS GUARDIEIRAS 776 MAIA 4470-593 MAIA','229441760','N/D','residencialpuma@live.com.pt','','','2021-03-22 16:17:45'),
 (1105,'CARNEIRO  FILHOS COMERCIO DE PNEUS LDA','R FERREIRA DE LEMOS 91 SANTO TIRSO 4780-468 SANTO TIRSO','252852179','N/D','geral@carneiroefilhos.pt','','','2021-03-22 16:17:45'),
 (1106,'PC Componentes y Multimedia SLU','Poligono Industrial Neinver - Astintze Kalea, 24 AV Europa, 2-3 Pol.Ind.Las Salinas 3084-000  Alhama de Murcia','','N/D','www.pccomponentes.com','','Compra material informático','2021-03-22 16:17:45'),
 (1107,'FRANCISCO GOMES DE ALMEIDA VIEGAS CALHEIROS','RUA CECÍLIO FRANCO, 25 ASSEICEIRA GRANDE 2665-501 VENDA DO PINHEIRO','','N/D','jcalheiros@food-tech.pt','','','2021-03-22 16:17:45'),
 (1108,'RUI MOREIRA REPARAÇÕES AUTOMOVEIS UNIPES.LDA','RUA DAS AGRAS 709 MAIA 4475-606 MAIA','229823613','N/D','rui-moreira@sapo.pt','','','2021-03-22 16:17:45'),
 (1109,'EMPREENDIMENTOS HOTELEIROS DA QUINTA DO FERRO','R IVONE SILVA N 18 LISBOA 1050-124 LISBOA','217814000','N/D','hotelzurique@viphotels.com','','','2021-03-22 16:17:45'),
 (1110,'XIRATOUR HOTELARIA E TURISMO SA','AVENIDA BARRANCO DE CEGOS N 22 VILA FRANCA DE XIRA 2600-214 VILA FRANCA DE XIRA','263276670','N/D','reservaslph@continentalhotels.eu','','','2021-03-22 16:17:45'),
 (1111,'PAULO CARLOS DANTAS RIBEIRO DE ALMEIDA SOUTO','LARGO DO POLIEDO,ED.SANTO ANTÓNIO ENT.B FR BM VILA REAL 5000-596 VILA REAL','','N/D','paulosoutoadvogados@gmail.','','','2021-03-22 16:17:45'),
 (1112,'JOAQUIM SILVA E SOUSA LDA','RUA DA AMIEIRA N 628 E 648 MATOSINHOS 4465-021 SÃO MAMEDE DE INFESTA','229516258','N/D','jssousa.lda@gmail.com','','','2021-03-22 16:17:45'),
 (1113,'SECWAY LDA','PRAÇA SOUSA CALDAS N 122 VILA NOVA DE GAIA 4400-138 VILA NOVA DE GAIA','224073299','N/D','geral@secway.pt','','','2021-03-22 16:17:45'),
 (1114,'AGEAS PORTUGAL - COMPANHIA DE SEGUROS, S.A.','RUA GONÇALO SAMPAIO 39 PORTO 4150-367 PORTO','226081100','N/D','jcalheiros@food-tech.pt','','','2021-03-22 16:17:45'),
 (1115,'ROBERT MAUSER, LDA','RUA DOS HEROIS DO ULTRAMAR N 366 ZONA INDUSTRIAL DA FRE A ARMAZEM M 2670-747 LOUSA','','N/D','geral@mauser.pt','','','2021-03-22 16:17:45'),
 (1116,'PECOMARK PORTUGAL COMERCIO DE EQUIPAMENTOS DE','RUA JOÃO MANUEL LOPES PINHEIRO LOTE 1 A EIRAS COIMBRA 3020-171 COIMBRA','229476017','N/D','porto@pecomark.pt','','','2021-03-22 16:17:45'),
 (1117,'WURTH PORTUGAL TECNICA MONTAGEM LDA','ESTRADA NACIONAL 249 4 ABRUNHEIRA SINTRA 2710-691 SINTRA','229442458','N/D','info@wurth.pt','','','2021-03-22 16:17:45'),
 (1118,'IBERWEB LDA','AVENIDA DA LIBERDADE 615 1 BRAGA 4710-251 BRAGA','','N/D','geral@iberweb.pt','','','2021-03-22 16:17:45'),
 (1119,'AZITANIA GLOBAL EXPEDITION','GALLE GENOVA 15 PISO 3 DERECHA MADRID 28014 MADRID','34628208944','N/D','aglobalexpedition@gmail.com','','MMSet2011:Sonae Pingo Doce 120 diasStock 90 dias','2021-03-22 16:17:45'),
 (1120,'SOCIEDADE HOTELEIRA OLIATLÂNTICO, S.A.','AVENIDA D JOÃO II 47 LISBOA 1998-028 LISBOA','210020400','N/D','HOTELARTS@VIPHOTELS.COM','','','2021-03-22 16:17:45'),
 (1121,'CODIPOR ASSOC PORTUGUESA IDENTIFICACAO E CODI','ESTRADA DO PAÇO DO LUMIAR CAMPUS DO LUMIAR EDIFICIO K3 LISBOA 1649-038 LISBOA','217520740','N/D','financeira@gs1pt.org','','','2021-03-22 16:17:45'),
 (1122,'Direityavesso, Lda','Rua Prof. Egas Moniz, 223 Agueda 3750-142 AGUEDA','','N/D','geral@direityavesso.pt','','','2021-03-22 16:17:45'),
 (1123,'SPEDYCARGO, TRANSITÁRIOS, S.A.','VIA CENTRAL DE MILHEIROS N 726 MILHEIROS 4475-330 MAIA','229993650','N/D','geral@spedycargo.pt','','','2021-03-22 16:17:45'),
 (1124,'TORRESTIR TRANSITARIOS LDA','PARQUE COMERCIAL N 91 BRAGA 4715-216 BRAGA','253680100','N/D','geral@torrestir.com','','','2021-03-22 16:17:45'),
 (1125,'S.A.M. Kuchler Electronics GmbH','Klatteweg 4-6 9020 Klagenfurt am Wörthersee A-9020 Klagenfurt am Wörthersee - AUSTRIA','43 (0) 463 - 43 5 43','N/D','mail@sam-kuchler.com','','16/10-mudou de contribuinte e designação. Passou a Holding (MM tem conhecimento. MD)','2021-03-22 16:17:45'),
 (1126,'LUSOCARGO TRANSITARIO SA','R JOAQUIM DIAS SALGUEIRO, 167 VILA NOVA DA TELHA 4470-777 MAIA','229990900','N/D','geral@lusocargo.pt','','','2021-03-22 16:17:45'),
 (1127,'MEO - SERVIÇOS DE COMUNICAÇÕES E MULTIMÉDIA S','AV FONTES PEREIRA DE MELO N 40 LISBOA 1069-300 LISBOA','','N/D','geral@meo.pt','','','2021-03-22 16:17:45'),
 (1128,'FIGUEIREDO  CONCEIÇÃO - ACTIVIDADES HOTELEIRA','LG MESTRE ANACLETO MARCOS DA SILVA S/N LOURINHÃ 2530-124 LOURINHÃ','','N/D','info@hotelfigueiredos.com','','','2021-03-22 16:17:45'),
 (1129,'SOPESAGEM - SOLUÇÕES DE PESAGEM E ETIQUETAGEM','RUA DAS FLORES N 10 SEMINO QUARTEIRA 8125-303 QUARTEIRA','289863036','N/D','sopesagem@iol.pt','','','2021-03-22 16:17:45'),
 (1130,'ARMANDO MANUEL DA SILVA','R DE BOLHÃO 747 R/CHÃO OLIVEIRA DO DOURO VILA NOVA GAIA 4430-340 VILA NOVA DE GAIA','','N/D','manoel.rocha.pt@hotmail.com','','','2021-03-22 16:17:45'),
 (1131,'TUEL TECNICOS UNIDOS DE ELECTRONICA LDA','R AREOSA N 238 PORTO 4200-083 PORTO','225405235','N/D','geral@tuel.pt','','','2021-03-22 16:17:45'),
 (1132,'COREPLUS, Lda','Rua Novo Arruamento do Cavaco, nº132 Vermoin 4470-263 Maia','220 937 356','N/D','ivo@coreplus.pt','','(29/9/2014 MJ - é cliente nº 2488)','2021-03-22 16:17:45'),
 (1133,'SOCIEDADE COMERCIAL DE AUTOMOVEIS RENO S A','RUA DA ESTRADA N 387 PAÇOS DE FERREIRA 4590-778 FERREIRA PFR','255860450','N/D','contabilidade@autoreno.pt','','','2021-05-18 09:27:19'),
 (1134,'ZARCOFRIO EQUIPAMENTOS HOTELEIROS LDA','R CORONEL MANUEL FRANÇA DORIA N 15 CAMARA DE LOBOS 9300-044 CAMARA DE LOBOS','291942828','N/D','geral@zarcofrio.pt','','(29/9/2014 MJ - é cliente nº 2488)','2021-03-22 16:17:45'),
 (1135,'ATRADIUS CRÉDITO Y CAUCIÓN, S.A. DE SEGUROS Y','AV DA LIBERDADE 245 3 C LISBOA 1250-143 LISBOA','213190370','N/D','jorge.camilo@creditoycaucion.pt','','(29/9/2014 MJ - é cliente nº 2488)','2021-03-22 16:17:45'),
 (1136,'PETROIBERICA SOC DE PETROLEOS IBERO LATINOS S','ZONA INDUSTRIAL DA PEDRULHA LOTE 12 CASAL COMBA 3050-183 CASAL COMBA','231224270','N/D','geral@petroiberica.pt','','(29/9/2014 MJ - é cliente nº 2488)','2021-03-22 16:17:45'),
 (1137,'M COUTINHO PORTO COMERCIO DE AUTOMOVEIS S A','ESTRADA EXTERIOR DA CIRCUNVALAÇÃO N 3708 RIO TINTO 4435-187 RIO TINTO','229771000','N/D','mcoutinhoporto@mcoutinho.pt','','(29/9/2014 MJ - é cliente nº 2488)','2021-03-22 16:17:45'),
 (1138,'TRANSNAUTICA - GLOBAL LOGISTICS S A','RUA DAS CASAS QUEIMADAS 97 EDIFICIO DESFO VILA NOVA DE GAIA 4415-439 GRIJÓ VNG','229991200','N/D','geral@transnautica.pt','','(29/9/2014 MJ - é cliente nº 2488)','2021-03-22 16:17:45'),
 (1139,'ALVES BANDEIRA & CA S.A.','VALE DE VAZ VILA NOVA DE POIARES 3350-110 VILA NOVA DE POIARES','231244200','N/D','geral@petroiberica.pt','','(29/9/2014 MJ - é cliente nº 2488)','2021-03-22 16:17:45'),
 (1140,'VIA VERDE PORTUGAL - GESTÃO DE SISTEMAS ELECT','QTA DA TORRE DA AGUILHA - EDIF BRISA SÃO DOMINGOS DE RANA 2789-523 SAO DOMINGOS DE RANA','210730300','N/D','geral@viaverde.pt','','(29/9/2014 MJ - é cliente nº 2488)','2021-03-22 16:17:45'),
 (1141,'SALEM TRANSITARIOS INTERNACIONAIS E INSULARES','R DA ESTRADA  ARM 7 CRESTINS 4470-600 MAIA','229438000','N/D','SALEM@SALEM.PT','','','2021-03-22 16:17:45'),
 (1142,'VITOR M SANTOS SERVIÇOS LIMPEZA JARDINAGEM LD','RUA EÇA DE QUEIROS 135 VILA NOVA DA TELHA 4470-766 MAIA','224041586','N/D','.','','','2021-03-22 16:17:45'),
 (1143,'SAPHETY LEVEL TRUSTED SERVIÇES SA','RUA VIRIATO 13, 3º PISO LISBOA 1050-233 LISBOA','210114620','N/D','.','','','2021-03-22 16:17:45'),
 (1144,'Penta Ibérica - Soc.Ibérica de Embalagens,Lda','Zona Industrial Norte - Armazém 5 - Vale Canas  2560-381  TORRES VEDRAS','261 919 075/6/7','N/D','comercial@pentaiberica.pt','','','2021-03-22 16:17:45'),
 (1145,'EQUIPROFI-EQUIP.E PROD.LIMPEZA UNIPESSOAL, LD','RUA CANDIDO DOS REIS 1999 MATOSINHOS 4460-705 MATOSINHOS','220925408','N/D','.','','','2021-03-22 16:17:45'),
 (1146,'Multisac, Embalagens Flexiveis, S. A','Z. Ind. do Monte Cavalo, lote 1ª  3670-273 Vouzela','232-772377','N/D','geral@multisac.pt','','','2021-03-22 16:17:45'),
 (1147,'Gallega de Etiquetado S.L','Areiña 4A Villaza 36380 Gondomar / Pontevedra- España','+34.986.369726','N/D','galetiq@hotmail.com','','','2021-03-22 16:17:45'),
 (1148,'STOPCARMO DE ALVERCA, UNIPESSOAL, LDA','ESTRADA NACIONAL DEZ KILOMETRO 127 BLOCO 4 ARMAZEM 4 ALVERCA DO RIBATEJO 2615-133 ALVERCA DO RIBATEJO','219585596','N/D','.','','','2021-03-22 16:17:45'),
 (1149,'ANTONIO JOSE VIEIRA MAIA','PRAÇA RAINHA D.AMÉLIA 36 PORTO 4000-075 PORTO','225105280','N/D','geral@ajmaia.net','','','2021-03-22 16:17:45'),
 (1150,'MANUEL DOS SANTOS PEREIRA DESPACHANTES OFICIA','ESTR NACIONAL 107 NUMERO 4142 - 1 ANDAR SL 108 FREIXIEIRO 4455-494 PERAFITA','229959486','N/D','santos.pereira@despachante.cdo.pt','','','2021-03-22 16:17:45'),
 (1151,'VERDATIFICIO GLOBAL TRADING LDA','RUA DAS LARANJEIRAS LOTE 44 URBANIZAÇÃO QUINTA DO COVELO 6200-024 COVILHA','963201227','N/D','verdartificio@gmail.com','','','2021-03-22 16:17:45'),
 (1152,'SICOESTE SOCIEDADE INDUSTRIAL DE CUTELARIAS, ','RUA ALTO DE VILA 33 BENEDITA 2475-113 BENEDITA','262929329','N/D','SICO@SICO.PT','','','2021-03-22 16:17:45'),
 (1153,'VITOR MANUEL TEIXEIRA DE SA','R HORIZONTE 888 FONTELEITE 4745-532 SAO ROMAO CORONADO','967285134','N/D','.','','','2021-03-22 16:17:45'),
 (1154,'DHL EXPRESS PORTUGAL, LDA','AV D.JOÃO I PISO 4 LISBOA 1990-085 LISBOA','707102022','N/D','lisccexpress@dhl.com','','','2021-03-22 16:17:45'),
 (1155,'Prominent Chemical S.L','Pol.Ind.La Marquesa- C/Llauradors 96 46260 Alberic 4626-000 Alberic','','N/D','info@prominentchemical.com','','','2021-03-22 16:17:45'),
 (1156,'LEVELMAQ, UNIPESSOAL LDA','AV.SANTA MARTA 105 MOREIRA DE CONEGOS 4815-295 MOREIRA DE CONEGOS','916478970','N/D','levelmaq@gmail.com','','','2021-03-22 16:17:45'),
 (1157,'PT MEDICAL- SEM DESAVENÇAS','AVENIDA JOAO DE DEUS 441 ERMESINDE 4445-474 ERMESINDE','','N/D','.','','','2021-03-22 16:17:45'),
 (1158,'Ta Fang Technology Co., Ltd','9F., No.358, SongHe Street  Nangang Dist Taipei City 11554, Taiwan R.O.C.','+886 2 77137950 ext 10','N/D','jasonlinn@ta-fang.com','','','2021-03-22 16:17:45'),
 (1159,'Metalocachoeira Serralharia Civil Lda','R Moinho Charela 7 Sapataria 2590-430 Sapataria','','N/D','geral@metalocachoeira.pt','','','2021-03-22 16:17:45'),
 (1160,'MAQUIGOMES CO.REP.MAQ.IND.UNIPES. LDA','Rua do Olival 4 Roussada 2665-376 ROUSSADA','','N/D','geral@maquigomes.com','','','2021-03-22 16:17:45'),
 (1161,'BLUEPACK COMERCIO DE EMBALAGENS LDA','URBANIZAÇÃO DO LIDADOR R 11 15 MAIA 4470-703 MAIA','','N/D','.','','','2021-03-22 16:17:45'),
 (1162,'IDSYS S.L','Sanches Pacheco, 73 -1ª Planta Edf.Ávila Madrid 28002 MADRID','','N/D','','','ID.SYS, S.L.Largo Movimento das Forças Armadas, nº 4 - Alfragida2610-123 AMADORA','2021-03-22 16:17:45'),
 (1163,'ANTIPODA – PAULO JORGE AZEVEDO, UNIPESSOAL LD','R. DA ESTAÇÃO VELHA, 2238 3º DIR SRA DA HORA 4460-305 SRA DA HORA','22 9535437','N/D','','','ARMAZEM FABRIL / TRAV. MONTE DE SÃO GENS, 58 -124 / 4460-771 CUSTOIAS','2021-03-22 16:17:45'),
 (1164,'CTT EXPRESSO - SERVIÇOS POSTAIS E LOGISTICA S','AV D JOÃO II 13 LISBOA 1999-001 LISBOA','','N/D','.','','','2021-03-22 16:17:45'),
 (1165,'ITEKPRINT MARKETING URBANO UNIPESSOAL LDA','RUA DO CAVACO 463A MAIA 4470-262 MAIA','','N/D','.','','','2021-03-22 16:17:45'),
 (1166,'HUGO FILIPE RODRIGUES - MENTOL ATELIER COMUNI','R MARIA, Nº 67 - 1º LISBOA 1170-210 LISBOA','962806048','N/D','.','','','2021-03-22 16:17:45'),
 (1170,'ETIROL S.L.','Avda. Herramientas, 36 Pol. Ind. San José de Valderas 0000-000 28918 - Leganés - MADRID','916416770','N/D','ETIROL@ETIROL.com','','','2021-03-22 16:17:45'),
 (1171,'LINEA MASSIMA ACESSORIOS DE MODA LDA','TV S SALVADOR - ARMAZEM 3 N 346 GRIJO 4415-534 GRIJÓ VNG','','N/D','lmassima@gmail.com','','','2021-03-22 16:17:45'),
 (1172,'MBA - MARKETING E BRINDES LDA','RUA VILAR DO SENHOR N 493 MATOSINHOS 4455-213 LAVRA','','N/D','info@nobrinde.com','','','2021-03-22 16:17:45'),
 (1173,'QUEIJOS LAGOS - QUEIJOS E DERIVADOS LDA','RUA DO AMEAL 16 2 ESQ OLIVEIRA DO HOSPITAL 3400-101 OLIVEIRA DO HOSPITAL','238602578','N/D','geral@queijoslagos.com','','','2021-03-22 16:17:45'),
 (1174,'VINOTECA - COMERCIO DE BEBIDAS S A','R DO OUTEIRO - 771 - LOTE 11 - ZONA INDUSTRIAL DA MAIA OR I 4475-150-MAIA','229407373','N/D','geral@vinoteca.pt','','','2021-03-22 16:17:45'),
 (1175,'THIELMANN ROTEL INTERNATIONAL GmbH','Oltnerstrasse 93 AARBURG CH - 4663 AARBURG - SWITHERLAND','0041 62 791 35 60','N/D','office@juicemaster.ch','Polina Sonntag','VER TAMBÉM FORNECEDOR N.º 654Quando lançar a fatura emitir logo nota liquidação com 3% desc a 30 dias e dar á raquel. Pode ser no bcp.','2021-03-22 16:17:45'),
 (1176,'SYNLABHEALTH PORTO, S.A.','RUA SA DA BANDEIRA 790 PORTO 4000-432 PORTO','222087614','N/D','.','','','2021-03-22 16:17:45'),
 (1177,'KEYMAC - Comercio de Equipamentos Ind., Lda','Rua Armando Reis Vieira, 31   2490-546 OURÉM','249094538','N/D','keymac@keymac.pt','Jorhe Telhada','PUXAR ADIANTAMENTO','2021-03-22 16:17:45'),
 (1178,'AVEIROTEL - EQUIPAMENTO HOTELEIRO S.A.','ZONA INDUSTRIAL DE AVEIRO, ESGUEIRA AVEIRO 3800-991 AVEIRO','234300750','N/D','geral@aveirotel.pt','','','2021-03-22 16:17:45'),
 (1179,'Lora Srl','Via dei Quartieri, 45   36016 - THIENE (VI) - ITALIA','0445-386859','N/D','info@lorasrl.com','','Sede operativa: Via Abruzzi, 50 - 36016 THIENE (VI)','2021-03-22 16:17:45'),
 (1180,'EUROCLARIO AUTOMATISMOS ELETRICOS LDA','RUA DO CAULINO 212-216 ALFENA 4445-259 ALFENA','229688460','N/D','.','','','2021-03-22 16:17:45'),
 (1181,'JVCALVES PRODUTOS SIDERURGICOS SA','RUA RIBEIRO CAMBADO 1435 75 VALONGO 4440-695 VALONGO','229407373','N/D','jvcalves@jvcalves.pt','','','2021-03-22 16:17:45'),
 (1182,'JUSTLOG - AGENTES TRANSITARIOS LDA','CENTRO EMPRESARIAL DE VILAR DO PINHEIRO - VIA JOSE REGI 196/202 4485-860 VILAR DO PINHEIRO','229407373','N/D','justlog@justlog.pt','','','2021-03-22 16:17:45'),
 (1183,'SEEPMODE LDA','AV MARECHAL CARMONA N 124 BLOCO 1 R/C A SALA A CASCAIS 2750-312 CASCAIS','214016841','N/D','info@seepmode.com','PATRICIA','','2021-03-22 16:17:45'),
 (1184,'JBM CAMPLLONG S.L','CIM LA SELVA - CTRA.AEROPORT KM1.6 NAU 2.2 C.P.17185 - VILOBI D´ONYAR 0000-000 C.P.17185 - VILOBI D´ONYAR - GIRONA','972245533','N/D','.','','','2021-03-22 16:17:45'),
 (1185,'US - CONSULTORIA DE GESTÃO LDA','RUA O PRIMO BASILIO N 109 2 FRENTE SãO DOMINGOS DE RANA 2785-745 SAO DOMINGOS DE RANA','','N/D','pedro.santos@usefulsolutions.pt','','','2021-03-22 16:17:45'),
 (1186,'MORET PACKAGING, S.L.','Calle Algepsers, 67 y 69 - Parque Empresarial Táctica 46980 Paterna (VALENCIA) - España','+34 96 132 45 17','N/D','moret@moretpackaging.com','','Horario Recogidas: de 8:00 a 14:00 y de 15:00 a 18:00. Viernes hasta las 14:00h.21/3/2017 - alteraçao da designaçao social de troquemor para Moret. Vat Nº é o mesmo (MD)','2021-03-22 16:17:45'),
 (1187,'LACOABREU - PINTURA ELECTROESTATICA UNIPESSOA','RUA DO CAULINO N 200/204 ALFENA 4445-259 ALFENA','224001563','N/D','lacoabreu@sapo.pt','','','2021-03-22 16:17:45'),
 (1188,'P R N INFORMATICA LDA','AVENIDA BOMBEIROS VOLUNTARIOS N 464 REBORDOSA 4585-359 REBORDOSA','224157629','N/D','comercial@@prn.pt','','','2021-03-22 16:17:45'),
 (1189,'GEV Recambios de Hostelería, S.L.U.','Energía, 39-41, PI Famadas 08940 Cornellá - España 0894-000 Cornellá - España','','N/D','.','','','2021-03-22 16:17:45'),
 (1190,'NEBUKRE - COMPRA VENDA E CONSTRUÇÃO DE IMOVEI','R DO OUTEIRO, LOTE 11 MAIA 4475-150 MAIA','229407373','N/D','geral@vinoteca.pt','','','2021-03-22 16:17:45'),
 (1191,'L3W - MATERIAL ELECTRICO LDA','R AMELIA REY COLAÇO N 384 CALENDARIO 4760-293 VILA NOVA DE FAMALICÃO','252316007','N/D','famalicao@l3w.pt','','','2021-03-22 16:17:45'),
 (1192,'STEP PACK, LDA','ZONA DA RODOVIARIA DE COVAS PAVILHÃO 7 URGEZES GUIMARÃES 4810-565 GUIMARAES','252316007','N/D','.','','','2021-03-22 16:17:45'),
 (1193,'FUTURESBOÇO - INDUSTRIA DE SERIGRAFIA LDA','RUA DR EDUARDO SANTOS SILVA N 261 FRACÇÃO BJ PORTO 4200-283 PORTO','252316007','N/D','comercial@futuresboco.pt','','','2021-03-22 16:17:45'),
 (1194,'Alucart S.r.l','Via  R. Morandi 2 20017 Mazzo di Rho (MI) - Italia ITALIA','+39 2 9346171 / 93901702','N/D','direzione@alucart.it','','Sede: Via Bartolini 1 / 20155 MILANO','2021-03-22 16:17:45'),
 (1195,'HOTEL PREMIUM MAIA, LDA','RUA SIMÃO BOLIVAR N 375 MAIA 4470-214 MAIA','','N/D','.','','','2021-03-22 16:17:45'),
 (1196,'CDE CONSULTORES DE DESENVOLVIMENTO EMPRESARIA','RUA ANTONIO PATRICIO N 69 1 DT MATOSINHOS 4460-281 SENHORA DA HORA','','N/D','miguel.soares@cdeconsultores.pt','Miguel Soares','','2021-03-22 16:17:45'),
 (1197,'CENTRAL DE COMPRAS FUSTE LDA','RUA SANTO ANDRE N 198 BRAGA 4710-596 ADAUFE','253 628 570/1','N/D','gesfuste@gesfuste.pt','','','2021-03-22 16:17:45'),
 (1198,'MEDIS - COMPANHIA PORTUGUESA DE SEGUROS DE SA','AVENIDA DR MARIO SOARES TAGUS PARK EDIFICIO 10 PISO 1 PORTO SALVO 2744-002 PORTO SALVO','','N/D','rafaela.silva@agentegeral.ageas.pt','','','2021-03-22 16:17:45'),
 (1199,'SGL-CORPORATE FACILITY SERVICES, S.A.','AVENIDA DOM JOÃO II N 45 EDIFICIO CENTRAL OFFICE 8 A B LISBOA 1990-084 LISBOA','','N/D','geral@sgl.pt','','','2021-03-22 16:17:45'),
 (1200,'METALURGICAS DE SIERRA','Poligono Malpica, Santa Isabel Calle E Parcela 9-10 Naves 28/29 5001-600 ZARAGOZA     ESPANHA','0034976572324','N/D','info@metalurgicadesierras.com','','','2021-03-22 16:17:45'),
 (1201,'ZHEJIANG DINGYE MACHINERY CO.,LTD.','No.77 Jinyang Road, Qidiweilai Industrial Park, Nanbaixiang subdistrict, Ouhai, Wenzhou cit CHINA','13587872024','N/D','.','','','2021-03-22 16:17:45'),
 (1202,'SERITEL, SERIGRAFIA E PUBLICIDADE, UNIPESSOAL','R DAS POMBAS N 33 ARMAZEM C QUINTA DO SIMÃO 3800-325 AVEIRO','234304031','N/D','seritel@seritel.pt','','','2021-04-02 18:40:39'),
 (1203,'PAPELARIA UNIVERSAL LDA','R GAY LUSSAC 11 QUIMIPARQUE BARREIRO 2830-140 BARREIRO','234304031','N/D','cliente@papelariauniversal.pt','','','2021-04-02 18:40:39'),
 (1204,'MAINCO MIRANDA S.L.','69, SUZANA ST. POLG.IND.BAYAS BURGOS 09-200 MIRANDA DE EBRO  ESPANHA','','N/D','mainco@maincomiranda.com','','','2021-04-02 18:40:39'),
 (1205,'ISOLUTIONS- MAQUINAS DE PROCESSAMENTO E EMBAL','RUA DA ZONA INDUSTRIAL ETEL S/N BAIRRO 4765-030 BAIRRO','','N/D','.','','','2021-04-02 18:40:39'),
 (1206,'OCTOPUS CREATIVE STUDIO, I , LDA','RUA DO COMERCIO 16 ODEMIRA 7630-462 ODEMIRA','936318941','N/D','SARA@OCTOPUSCREATIVESTUDIO.COM','','','2021-05-18 09:27:19'),
 (1223,'Fli-Blade Limited','Building C, Knaresborough Technology Park Manse Lane Knaresborough HG5 8LF - United Kingdom','','N/D','','Mark Guffick','NOVO - 1419.controlada pela controlzone. Todos os contactos  são com o Mark Guffick','2021-03-22 16:17:45'),
 (1224,'Zunidata Systems Inc.','5F nº 88 Baojhong Rd, Shindian City Taipei 231 - Taiwan (RPC)','886 2 2912-0211 ext. 121','N/D','alice.sung@zunidata.com','','','2021-03-22 16:17:45'),
 (1226,'Clientron Corp.','3F., No.75. Sec. 1, Sintai 5th Rd Sijhih Dist., New Taipei City 221 Taiwan(R.O.C.)','886-2-2698-7068- ext.610','N/D','service@clientron.com','Marc Lin','','2021-03-22 16:17:45'),
 (1227,'Mais Automação – Distrib. Produtos Autom. Ind','Via Central de Milheiros, nº 720 Maia 4475-330 Maia','224809584/5','N/D','info@mais-automacao.pt','Pedro Natividade','','2021-03-22 16:17:45'),
 (1231,'SILKO','','','N/D','','','Aberto Forncedor só para Marca.','2021-03-22 16:17:45'),
 (1234,'Kerres Anlagensysteme GmbH','Manfred-Von-Ardenne-Allee 11 D-71522 - Backnang','49 (0) 7191-9129-0','N/D','info@kerres-group.de','','','2021-03-22 16:17:45'),
 (1237,'GUANGZHOU CITY GSAN SCIENCE &TECHNOLOGY CO. L','4TH FLOOR HUACHUANG ANIMATION INDUSTRIAL PARK JINSHAN AVENUE, SHIQI,PANYU DISTRICT 511450 - GUANGZHOU (RPC)','0086 020-87506970','N/D','87506970@163.com','CANDY LIN- tlm: +86020-7574498','Beneficiario: HONG KONG GSAN INDUSTRIAL CO.LIMITEDBANCO:HSBC Hong KongQueens Road CentralHong Kong','2021-03-22 16:17:45'),
 (1241,'HORIS  SAS - BONNET INTERNATIONAL','17 RUE DES FRERES LUMIERE Z.I. MITRY COMPANS 77292-MITRY MORY CEDEX - FRANCE','0033-3-84737569','N/D','sabrina.beati@itweurotec.it','VERNIVE PIERRE','\"Yes it is 180 days\"Sabrina - 26/11/18desconto peças/acessórios 40%user: apoioclientes@mirandaeserra.pt pass:miranda2019','2021-03-22 16:17:45'),
 (1244,'Angles Maquinaria de Envase Y Embalaje, S.L.','C. Renall, 11 08205 Sabadell (Barcelona) Espanha','+34 93 720 53 05','N/D','info@anglessl.com','','','2021-03-22 16:17:45'),
 (1245,'Depositembal Lda','Rua Alexandre Magalhães 51 Zona Industrial Maia I, Sector X Lote: 328 4475-251 - Porto','228306512','N/D','geral@depositembal.pt','Andre Barbosa','','2021-03-22 16:17:45'),
 (1246,'ALTRONIX - SISTEMAS ELECTRÓNICOS, LDA','Rua José Moura Coutinho, 214 Estrada Nacional 14 4785-330 Trofa','252 103 200','N/D','info@altronix.pt','Marlene Ferreira','Local de recolha:Rua José Moura Coutinho,Nº 214 - EN14 - 4745-330 Trofa','2021-03-22 16:17:45'),
 (1251,'Lumbeck & Wolter GmbH & Co. KG','Linde 72-74 D-42287 - Wuppertal - Germay','0049 0202 246510?','N/D','vk@lumbeck-wolter.de','Panagiotis Goritsas','','2021-03-22 16:17:45'),
 (1252,'REVIC SPÓLKA Z OGRANICZONA ODPOWIEDZIALNOSCIA','SOKOLSKA 70 41-219 SOSNOWIEC - Polonia','+48603033965','N/D','REVIC@REVIC.PL','Sergio Stanislawski','','2021-03-22 16:17:45'),
 (1258,'Marel Spain & Portugal, S.L.','Polígono Industrial Molí de La Bastida Carrer Pagesía 22-24 08191 Rubí (Barcelona) Spain','936546034','N/D','pedro.costa@marel.com','','','2021-03-22 16:17:45'),
 (1266,'XIAMEN RONGTA TECHNOLOGY CO., LTD','3F/E Building, Gaoqi Industrial Area, GaoqiBeisan Road Dianqian, Huli Xiamen, China','0086-5925553292','N/D','rt55@rongtatech.com','Lee','Pagamento paypal: finance02@rongtatech.com','2021-03-22 16:17:45'),
 (1268,'Danipack, indústria de plásticos, S.A.','Avenida Pacopar, Lote C04 Eco Parque Empresarial 3860-529 Estarreja','+351 234 060 800','N/D','luciano.mortagua@danipack.pt','Carla Ferreira','','2021-03-22 16:17:45'),
 (1274,'Metalurgica dos Arcos - Artur C. Santos','Travessa da Ponte  3730-901 Vale de Cambra','','N/D','','','','2021-03-22 16:17:45'),
 (1277,'NORD Drivesystems PTP, Lda.','Zona Industrial de Oiã, Lote nº 8, Apartado 79 Oia 3770-059 OIA','234 727 090','N/D','info.pt@nord.com / encomendas.pt@nord.com','Eng. Alfredo Jesus','','2021-03-22 16:17:45'),
 (1292,'RCC RIBEIRO COMERCIO CUNHOS CORTANTES LDA','R DO XISTO N 859 TORNO 4620-801 TORNO','229959486','N/D','rcc-portugal@sapo.pt','','','2021-03-22 16:17:45'),
 (1320,'LEÇAFER - Imp.e Com.de Máq. e Ferramentas, SA','Apartado 3125 Travessa da Bateria, 184-212 4451-801 LEÇA DA PALMEIRA','229 998148','N/D','dep.financeiro@lecafer.pt','','','2021-03-22 16:17:45'),
 (1325,'C.E. REICH GMBH','Oberer Wasen 13 D-73630 Remsshalden Alemanha','0049-7151-79899','N/D','cereichgmbh@t-online.de','','','2021-03-22 16:17:45'),
 (1333,'ERVISA. S.A.','Poligono Industrial Malpica - Alfinden, C/.H nº 57 - 50171 La Puerta de Alfinden (Zarago ESPANHA','0034-976-455976','N/D','comercial@ervisa.com','','1ªENC 15/3/5 TR 60dias','2021-03-22 16:17:45'),
 (1341,'SIMIA-Sociedade Industrial de Máquinas p/ Ind','Penas - Edifício Simia, Apartado 1195 MONTIJO 2870-909  Montijo','+351 212 326 970','N/D','info@simia.pt','','','2021-03-22 16:17:45'),
 (1343,'ALBALOGIC UNIPESSOAL LDA','RUA DO REGUINHO 17C ALBERGARIA A VELHA 3850-120 ALBERGARIA A VELHA','234099844','N/D','geral@albalogic.pt','','','2021-03-22 16:17:45'),
 (1344,'Scharfen Maschinenhandel UG (haftungsbeschrän','Friedrich-Ebert-Str. 86 WITTEN D-58454 WITTEN - ALEMANHA','+49 2302 28 277 14','N/D','naomi.fricke@scharfen.com','','ex- 2661','2021-03-22 16:17:45');
INSERT INTO `dat_fornecedores` (`IdFornecedor`,`NomeFornecedor`,`MoradaFornecedor`,`ContactoFornecedor`,`ReferenciaFornecedor`,`EmailFornecedor`,`PessoaContactoFornecedor`,`Obs`,`timestamp`) VALUES 
 (1366,'Extrusiones Vinílicas Aragonesas S. L.','Poligono Industrial Malpica-Alfindén, C/H , N.º57 50171 La Puebla de Alfindén - Zaragoza Espanha','0034 976 455 455','N/D','comercial@araexfilm.com','Adrián Diez','','2021-03-22 16:17:45'),
 (1374,'SICOEL, S.L.','Calle Siglo XX, 76 08032 Barcelona ESPANHA','0034-934363123','N/D','fabricabcn@sicoel.es','','','2021-03-22 16:17:45'),
 (1391,'D S A - DESENVOLVIMENTO DE SISTEMAS DE AUTOMA','RUA DE HENRY THILLO N 105 F ZONA INDUSTRIAL DA MAIA SEC  NORTE 4475-249 MAIA','229471855','N/D','info@dsa.pt','','','2021-03-22 16:17:45'),
 (1417,'MPI - CREATIVE SOLUTIONS UNIPESSOAL LDA','RUA NOVA DA TELHEIRA N 190 SANTO TIRSO 4780-510 SANTO TIRSO','','N/D','ultrasonics@mpi-cs.pt','','','2021-03-22 16:17:45'),
 (1426,'GRUPO PIE PORTUGAL, SA','Rua Artur Aires, nº100  4490  PÓVOA DO VARZIM','252 290600','N/D','grupopie@grupopie.com','','','2021-03-22 16:17:45'),
 (1429,'EUROPNEUMAQ - EQUIPAMENTOS PNEUMATICOS E HIDR','R DA SENHORA MESTRA N 27 E 35 VILA NOVA DE GAIA 4410-035 SERZEDO VNG','227536820','N/D','info@europneumaq.pt','','','2021-03-22 16:17:45'),
 (1430,'PEDRO MANUEL PINHEIRO SOARES, UNIP.LDA','RUA ARMINDO NOGUEIRA COSTA 175 MAIA 4470-248 MAIA','','N/D','pmps.mail@sapo.pt','','','2021-03-22 16:17:45'),
 (1451,'CP2R Desenvolvimento Software, Lda','Rua Calçada de real, 10  4700-239 REAL - BRAGA','253693363 (Equicalculum)','N/D','','','Sim, número de IVA válido  20/06/2012','2021-03-22 16:17:45'),
 (1459,'ELECTRÃO - ASSOCIAÇÃO DE GESTÃO DE RESÍDUOS','RESTELO BUSSINESS CENTER BLOCO 5 4 A AVENIDA ILHA DA MA  35 I 1400-203 LISBOA','214169020','N/D','.','','','2021-03-22 16:17:45'),
 (1460,'SARA CAMPOS MEIRINHOS','RUA DO TOMBAR 4 PICOTE 5225-072 PICOTE','','N/D','.','','','2021-03-22 16:17:45'),
 (1461,'PROX SYSTEMS SPAIN S.L','C/ALBASANZ 14 BIS 2º D MADRID 28037 MADRID             ESPANHA','','N/D','.','','MMSet2011:Sonae Pingo Doce 120 diasStock 90 dias','2021-03-22 16:17:45'),
 (1462,'CRISTRANS TRANSPORTES RODOVIARIOS DE MERCADOR','RUA 16 DE MAIO N 1830 BAIRROS TROFA 4785-520 TROFA','252416061','N/D','cristrans@gmail.com','','','2021-03-22 16:17:45'),
 (1463,'Pingo Doce - Dist. Alimentar, SA','Rua Actor António Silva, nº 7 1649-033 Lisboa 1099-008','22 377 08 88   (Sede217532000)','N/D','docelectronic@mirandaeserra.pt','Mário Gonçalves','','2021-03-22 16:17:45'),
 (1464,'FERNANDO TEIXEIRA E MATOS LDA','RUA DR JOAQUIM MANUEL DA COSTA 1355 GONDOMAR 4420-438 VALBOM GDM','223160888','N/D','freixo@moveistm.com','','','2021-03-22 16:17:45'),
 (1475,'Mercafilo, SL','Pol. Ind. El Oliveral, S13 Nave 26   46396 - Ribarroja / Valencia','+34 963 410 335','N/D','info@mercafilo.es;almacen@mercafilo.es','Pablo Chirivella','Morada p/recolhas:MERCAFILO, SLC/ Ciudad del Aprendiz, nº 17CP: 46017  VALENCIA (España)Tel: 963410335','2021-03-22 16:17:45'),
 (1500,'BERTO´S, S.P.A.','Viale Spagna, 12- Padova   35020 TRIBANO Z.I. - ITALIA','0039-0499588700','N/D','bertos@bertos.com; service@bertos.com','','(MJ) cond.pagt.    =60dias  a partir de 05/6/2=90dias1/4/03-cond.especial 120 e 150d-----------------------------------------Ped.Pagtºs=Sra.Laura/FabioTELE. CONTª 0499588797 --------------------------------08/6/2011-cond a 60 di','2021-03-22 16:17:45'),
 (1517,'MULTIBORRACHA ACESSORIOS DE BORRACHA E PLASTI','RUA IVENS 3B EDIFICIO D MECIA 3 P FUNCHAL 9000-046 FUNCHAL','229688400','N/D','geral@multiborracha.com','','','2021-03-22 16:17:45'),
 (1519,'Partner Tech Iberia - Retail Solutions & Syst','C/ Polo Sur 19 . 28850 Torrejón de Ardoz - Madrid','0034 913120632','N/D','joseantonio.delasheras@partner-tech.eu','José António de Las Heras','','2021-03-22 16:17:45'),
 (1534,'WEGEURO INDUSTRIA ELECTRICA S A','R ENG FREDERICO ULRICH - SECTOR V GUARDEIRAS - MOREIRA 4470-605 MAIA','229477700','N/D','info-pt@weg.net','','','2021-03-22 16:17:45'),
 (1542,'CRM - Meat Machines','Via Leonardo da Vinci, 62 23878 Verderio Superiore (LC) Itália','0039-039-9515456','N/D','','','','2021-03-22 16:17:45'),
 (1563,'Prosistav - Proj. e Sist. de Automação, Lda','Z.I. da Mota, Rua 7 Lote 6A Gafanha da Encarnação 3830-527 ILHAVO','234397210','N/D','prosistav@prosistav.pt','','','2021-03-22 16:17:45'),
 (1581,'GASIN II Unipessoal, Lda','Rua do Progresso, 53 - Perafita Apartado 3051 4451-801 LEÇA DA PALMEIRA','229 997216','N/D','GUERRAN@gasin.com','','','2021-03-22 16:17:45'),
 (1585,'Esteves Alves & Carvalho, Lda','Rua Alexandre Herculano , 80  4750-107 ARCOZELO','','N/D','','','PUXAR ADIANTAMENTO','2021-03-22 16:17:45'),
 (1634,'Migliorini S.r.l.','Via Perugino 32/34   27029 Vigevano (PV) -I taly','+39 038142919','N/D','Info@migliorinisrl.it;ni.bilanzuoli@migliorinisrl.it','Nicholas Bilanzuoli','','2021-03-22 16:17:45'),
 (1635,'Veripack S.r.l.','Via Maestri Del Lavoro - Z.I. Via per Gorla 21040 Cislago - Varese ITÁLIA','0039-0296671267/0296671243','N/D','spareparts@veripack.it','Peças','Para assistencia técnica + peças - a partir de 01/10/2018','2021-03-22 16:17:45'),
 (1637,'V.M. Distribuciones Industriales, S.L.','Isla Alegranza, 2 - Nave 58   28703 San Sebastián de los Reyes','+34 916 513 272','N/D','info@bandasvm.com','Roberto Laje','','2021-03-22 16:17:45'),
 (1639,'Metalúrgica Siemsen Ltda','Rodovia Ivo Silveira, 9525 - Bateas Santa Catarina 88355-202 Brusque - Santa Catarina - Brasil','+55 47 32116000','N/D','marketing@skymsen.com','RAFAEL FANTINI','','2021-03-22 16:17:45'),
 (1640,'G.S. ITALIA SRL','VIA STELVIO 193 Marnate 21050 Marnate (VA) - Italy','+39 0331 389 142','N/D','info@gsitalia.com;cristiano@gsitalia.com','Cristiano Scandroglio','','2021-03-22 16:17:45'),
 (1641,'SACCARDO ARTURO FIGLI, S.R.L.','Via del Lavoro, 15 36016 THIENE (VI) ITÁLIA','0039-0445-380021','N/D','','','','2021-03-22 16:17:45'),
 (1643,'Sälzer Electric GmbH','Matthiasstraße 16   57482 Wenden - Germany','+49(0)2762 614 167','N/D','stefan.halbe@salzer.de','Stefan Halbe','','2021-03-22 16:17:45'),
 (1662,'TECNOVAC S.R.L.','VIA PADRE ELZI N 28/C   24050 GRASSOBBIO BG - Italy','+390 35 534074','N/D','export@tecnovac.com','Comercial','','2021-03-22 16:17:45'),
 (1665,'Sambo Tech Corporation','462-1, Chowonji-ri, Daegot-Myeon, Gimpo-Si, Gyeonggi-Do COREIA','0082-31-9977797','N/D','','','','2021-03-22 16:17:45'),
 (1668,'Zemic Europe B.V.','Leerlooierstraat 8 EN Etten-Leur 4871 EN Etten-Leur - The Netherlands','+31 (0)76 5039480','N/D','info@zemic.nl;Carollyn@zemic.nl','Carollyn Rojas','Bank AddressABN AMRO BANKGustav Mahlerlaan 101082 PP AmsterdamThe NetherlandsAccount name: Zemic Europe B.V.IBAN: NL53ABNA0811130290BIC/Swift: ABNANL2A','2021-03-22 16:17:45'),
 (1669,'Vac-Star AG','Rte de l\'Industrie 7 Switzerland 1786 Sugiez - Switzerland','+41 26 673 93 00','N/D','info@vac-star.com','Michael Meyer','','2021-03-22 16:17:45'),
 (1675,'CEIA, SRL.','Zona Ind.le 54/G 52040 Viciomaggio - Arezzo Itália','0039-0575 4181','N/D','mcocci@ceia-spa.com','','','2021-03-22 16:17:45'),
 (1680,'M.T. BRANDÃO, LDA','Rua de Serralves, 599  4150-708 PORTO','226 167 370','N/D','mtb@mtbrandao.com','','NORMAL MENSAL - 20/10/15','2021-03-22 16:17:45'),
 (1711,'ROFEL INDUSTRIA METALURGICA AGUEDA LDA','BARRO BARRÔ 3750-353 BARRÔ AGD','234604628','N/D','rofel@rofel.pt','Mario Miranda','','2021-03-22 16:17:45'),
 (1720,'GROBA B.V.','Mangaanstraat 21 6031 RT NEDERWEERT - The Netherlands 6031 RT NEDERWEERT - The Netherlands','+31 0475 56 56 56','N/D','info@groba.eu; Bart.van.der.Vleuten@groba.eu','Bart van der Vleuten','','2021-03-22 16:17:45'),
 (1722,'DM PACK SRL','Via dell\'Artigianato, 34 San Vito di Leguzzano 36030 - San Vito di Leguzzano / Italy','+39.0445.602907','N/D','sales3@dmpack.it','Anna Zattara','','2021-03-22 16:17:45'),
 (1727,'DATAVAN INTERNATIONAL CORPORATION','NO. 186, JIAN 1st. RD., JHONGHE DIST, NEW TAIPEI CITY 5 NEW TAIPEI CITY 23553 NEW TAIPEI CITY - TAIWAN R.O.C.','','N/D','michelle_liu@datavan.com.tw','Michelle Liu','','2021-03-22 16:17:45'),
 (1731,'AM2C','Rua Marcel Paul - ZI de Kerdroniou Quimper 2900 Quimper - France','0033 2 98948919','N/D','anne.turgot@provisur.com','Oliver Bazin','','2021-03-22 16:17:45'),
 (1738,'INTERCOM -  Div.Univac Group Srl','Via Longhi 6/8 Fiorenzuola d\'Arda (Pc) 2917 - Fiorenzuola d\'Arda (Pc) - italy','(0039) 0523.247789','N/D','export2@intercom-vacuum.it','Nicole Cimelli','','2021-03-22 16:17:45'),
 (1740,'Shenzhen Gilong Electronic Co., Ltd','5F,Build. B, Kelunte Low Carbon Ind. Park, Gaofeng Com. Subdist. Off. of Dalang, Longhua New Town Guangdong Province Chi','+86-755-29402526','N/D','agnes@szgilong.com','Agnes Sheng','','2021-03-22 16:17:45'),
 (1741,'iMachine(Xiamen) Intelligent Device CO.,Ltd','NO.1502 C1 Building Wanda Plaza NO.3 Jinzhong Road Huli Xiamen 361006 -Xiamen - China','+86 592 6019189','N/D','sales11@imachine-tech.com','Julie Wang','','2021-03-22 16:17:45'),
 (1742,'Klöckner Pentaplast Europe GmbH & Co. KG','Industriestraße 3-5, Heiligenroth 5641-400 Heiligenroth (Alemanha)','+49 2602 915-0','N/D','Jinestal.Pereira@kpfilms.com; kpinfo@kpfilms.com','Jinestal Pereira','','2021-03-22 16:17:45'),
 (1743,'Afilados Y Representaciones S. L.','Pol. Ind. Mutilva, Calle I, 42B Mutilva 31192 Navarra','+34948290387','N/D','export@kingcut.es','Ana Maria Muñoz','','2021-03-22 16:17:45'),
 (1785,'TPH INDUSTRY S.P.A.','VIA ANGELO PEGORARO 26 21013 GALLARATE VA 0000-21013 GALLARATE VA','+390331213315','N/D','Alessandra.bernasconi@tphindustry.com','Alessandra Bernasconi','','2021-03-22 16:17:45'),
 (1786,'RATIONAL','','','N/D','','','so para marca','2021-03-22 16:17:45'),
 (1795,'GESFUSTE - Aquisição Venda e Manut. Equip., L','Parque Industrial de Adaúfe, Lote M1   4710-571 BRAGA','253 628 570/1','N/D','gesfuste@gesfuste.pt','','','2021-03-22 16:17:45'),
 (1850,'RGD, Maquinaria Para Embalaje, S.L.','Parsi, C/ Pino Silvestre, N.º 46 Sevilha 41016 Sevilha (Espanha)','0034 954999450','N/D','rgdmape@rgdmape.com','Simone Rigon','','2021-03-22 16:17:45'),
 (1876,'EBN TECHNOLOGY CORP.','9F, Nº 96, Sec. 1, Sintai 5th Rd. Sijhih City, Taipei Country TAIWAN','0088 6226 969292','N/D','','','','2021-03-22 16:17:45'),
 (1879,'FNC - Fabbrica Nazionale Cilindri, S.r.l.','Via Maestri Del Lavoro - Z.I. Via per Gorla 21040 Cislago - Varese ITÁLIA','0039-02 9667121','N/D','spareparts@veripack.it','','Maria 11/11/05-Pedro Serra TR30dias da data factura-Morada Legal: Milano - Via Dante 4Italia','2021-03-22 16:17:45'),
 (1885,'PLASTAR PAK, SRL','Via Guido Rossa, 22 Concorezzo 20049 Concorezzo (MI) - ITALIA','','N/D','','','11/11/05 P.Serra PAGT ANT com CH prog 30dias08/05/06 €10.761.94 Não ac TR a 30d - fez-se  TR ANT','2021-03-22 16:17:45'),
 (1972,'MECATRONIC, S.A.','Camino San Bernabé, s/n 46600 Alzira (Valencia) ESPANHA','0034-96 2404361','N/D','','','','2021-03-22 16:17:45'),
 (1974,'OBERMUHLE Polymertechnik GmbH','NaBackerstaBe, 6 07381 PoBneck ALEMANHA','0049-364742640','N/D','info@obermuehle.de','','MM - Tânia, lançar o desconto pp 3% na compra e regularizar adiantamento','2021-03-22 16:17:45'),
 (1976,'AVERY BERKEL LIMITED','Foundry Lane, Smethwick - POBox 14127 West Midlands, B67 9df INGLATERRA','0044 1215681218/ 24','N/D','DMoore@averyberkel.com','','PW site:User ID =  thumer@mirandaeserra.ptPassword =  TH511010','2021-03-22 16:17:45'),
 (2015,'GRAM PRECISION','C/Pallars, 65 - 6ª Planta 08018 Barcelona ESPANHA','0034-93-30003332','N/D','','','','2021-03-22 16:17:45'),
 (2139,'ESSEDUE SRL','VIA DELL´ELETTRONICA, Nº 53 Italia 27010 CURA CARPIGNANO (PV) - ITALIA','00390382474396','N/D','ellinor@essedueslicers.com','','','2021-03-22 16:17:45'),
 (2140,'Enterpack Limited','Unit 9, The Homestead- Watling Street- Potterspury Towcester, Northants NN12 6LH','01 327 810011','N/D','jude.wait@enterpack.co.uk - sales@enterpack.co.uk','Jude Wait','','2021-03-22 16:17:45'),
 (2261,'HERMANN SCHARFEN-MASCHINENFABRIK & CO.KG','POSTFACH 2304 RUHRSTR,76-76a WITTEN D-58452 WITTEN               ALEMANHA','0049 230 228 27 70','N/D','mail@scharfen.de','','novo - 1344','2021-03-22 16:17:45'),
 (2717,'ARISTARCO, S.P.A.','Via Del Lavoro, 30 C.P. 163 Castelfranco Veneto 31033 Castelfranco Veneto (TV)  -   ITALIA','00 39 0423 425611','N/D','info@aristarco.it','','(m) DEP.Ped.pagt. da Aristarco =Ana 04/11/05 cond alt pª 120diasAcessorios 50%+10% calculo Pvp tbl /0.75 Login site: aristarco/Password:  service57','2021-03-22 16:17:45'),
 (3012,'JOMIRPECAS COMERCIO E INDUSTRIA AUTO SA','ZONA INDUSTRIAL DE AVEIRO-ESGUEIRA AVEIRO 3801-101 AVEIRO','','N/D','comercial@jomirpecas.pt','','','2021-03-22 16:17:45'),
 (3020,'F.I.A.  Fabbrica Italiana Affettatrici S.P.A','Via L. Pierobon, 17 LIMENA (PD) 35010 LIMENA (PD)  -   ITALIA','00 39 049 7662511/7662520','N/D','exportdep@fiasrl.com','','(m) Ped pagt = Sr MarioVer cliente RST...Associada FELMAC (780) trata enc. + facturação/pagamento formadoras hamburguer. Enviar encomenda para a F.I.A - 12/09/2012Novo - Atlas Macchine Alimentari (870)','2021-03-22 16:17:45'),
 (3210,'DIBAL, S.A.','Poligono Industrial Neinver - Astintze 26 DERIO (BILBAO-VIZCAYA) 48160 DERIO                           ESPANHA','0034944521510','N/D','dibal@dibal.com','','MMSet2011:Sonae Pingo Doce 120 diasStock 90 dias','2021-03-22 16:17:45'),
 (3251,'BRUNO SALVADOR & CA S.N.C.','Via Kennedy, 60 Valla Di Riese Pio X 31030 Valla Di Riese Pio X (TV) - ITALIA','00390423746104','N/D','','','(M)MRosa 29/6/6- Passou a ser LAME ITALIA S.R.L.','2021-03-22 16:17:45'),
 (3848,'MADO GMBH','Maybachstrasse 1 D-72175 DORNHAN - SCHWARZWD ALEMANHA','0049 7455931 - 131','N/D','r.simsir@mado.de;p.guhl@mado.de','','','2021-03-22 16:17:45'),
 (5005,'IGT Microelectronics, S.A.','c/ Alonso Cano, 87 28003 Madrid ESPANHA','0034915333839','N/D','steven.wu@igt.es','','','2021-03-22 16:17:45'),
 (5871,'Magicpack Ibérica M&C, S.L.','Pol. Ind. Ca n´Oller. C/ Catalunya, 37 Barcelona 08130 Santa Perpètua de Mogoba','0034935602450','N/D','info@magicpack.es','Carlos Mantin','','2021-03-22 16:17:45'),
 (6580,'Comi Pak Engineering Srl','Via Lago di Tovel, 6i Z/L.2 Schio 36015 Schio - Italy','+390445575631','N/D','info@comipack.com','Simone Adami','','2021-03-22 16:17:45'),
 (6598,'T.L.M. TORINO LAVORAZIONI MECCANICHE S.R.L.','Via Venezia, 340-P Torino 10088 VOLPIANO (TORINO)','+390119881307','N/D','antonella@tlmpack.com','Antonella','','2021-03-22 16:17:45'),
 (6599,'HAFLIGER FILMS S.P.A.','VIA BRUNO BUOZZI 14 Rozzano 20089 ROZZANO MI','+39282511223','N/D','r.buaiz@hafliger.it','Mauro Albiero','','2021-03-22 16:17:45'),
 (6854,'FELMAC S.R.L.','VIALE DELLE INDUSTRIE 40 35010 CURTAROLO PD 35010 CURTAROLO (PD)','0039 0499620804','N/D','info@felmac.it','','','2021-03-22 16:17:45'),
 (8769,'PPTEX - INDUSTRIA TEXTIL LDA','Rua José Ferreira Paciência 659 SANTO TIRSO 4780-113 Santo Tirso','252098253','N/D','comercial@pptex.pt','João Queiroz','Seguro de Crédito 15.000,00 € - 07-04-2020','2021-03-22 16:17:45'),
 (9995,'Cartão Millennium / José Miguel Barbosa','... Maia 4470-605 MAIA','','N/D','geral@food-tech.pt','Cartão +','','2021-03-22 16:17:45'),
 (9996,'Cartão Millennium / Filipe Fernandes','... Maia 4470-605 MAIA','','N/D','geral@food-tech.pt','Cartão','','2021-03-22 16:17:45'),
 (9997,'Cartão Millennium / Filipe Soares','... Maia 4470-605 MAIA','','N/D','geral@food-tech.pt','Cartão 25716323','','2021-03-22 16:17:45'),
 (9998,'CX Maia','... Maia 4470-605 MAIA','','N/D','geral@food-tech.pt','','','2021-03-22 16:17:45'),
 (9999,'MIRANDA & SERRA SA','ZONA INDUSTRIAL DE AVEIRO-ESGUEIRA AVEIRO 3800-055 AVEIRO','','N/D','geral@mirandaeserra.pt','','','2021-03-22 16:17:45'),
 (12998,'AMB S.r.l.','Via della Tecnica, 57 (Località Cicogna) S. Lazzaro di Savena 40068 S. Lazzaro di Savena (BO) -  ITALIA','0039 051 6256555','N/D','amb@ambsrl.it','Annalisa Guzzini','(m)31/5/4 PECAS 30DIAS20/9/5 60 dias futuramente 90 dias??? aguardamos resposta do fornecedorFornecedor novo:   860','2021-03-22 16:17:45'),
 (903015,'REELSA - REGISTRADORAS ELECTRONICAS SL','POLIGONO P-29. CALLE DESTORNILLADOR - NAVE 30. GEVALO2 COLLADO VILLALBA (MADRID) 28400 COLLADO VILLALBA - MADRID - ESPAN','0034918519711/82/07','N/D','','','(M)','2021-03-22 16:17:45'),
 (903017,'CODI-PACK, MARCAJE Y CODIFICATION S.L.','C/. HUERTA DE ARRIBA, 81 ALGUAZAS - MURCIA 30560 ALGUAZAS (MURCIA)   -  ESPANHA','00 34 968 622 161','N/D','Codipack@larural.es','','(M)','2021-03-22 16:17:45'),
 (903062,'CX LX /  MIRANDA & SERRA, SA','Centro Emp. de Alverca -  APT.255  2616 - 501 ALVERCA DO RIBATEJO','21 9579740','N/D','','','FUNDO FIXO € 150 (Sandra)Reforço 100,00 -28/10/13 provisório','2021-03-22 16:17:45'),
 (903112,'TECNOINOX, SRL','Via Torricelli, n.1 33080 PORCIA (PORDENONE) ITALIA','0039 0434 920110','N/D','montagner.erika@tecnoinox.it','Erika Montaner','Address  to ship TECNOINOX  warehouse Via Malignani,11 ( near via Pieve ,25 -address for GPS - ) Zona Industriale   33080  Porcia (PN)  -  Highway exit- Fontanafredda - h. 08.00 – 11.45   13.30 -16.30','2021-03-22 16:17:45'),
 (903121,'CX PT 2 / MIRANDA & SERRA, SA','','','N/D','','','Apartir de 18/10/17 o caixa passa a ser controlado pelo Ricardo Fonseca, deixa de ser pago por transferência e passa a ser em  numerario...A.Gaspar','2021-03-22 16:17:45');
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
) ENGINE=InnoDB AUTO_INCREMENT=30 DEFAULT CHARSET=utf8;

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
  `TipoEquipamento` varchar(45) NOT NULL,
  `Oficina` tinyint(1) NOT NULL DEFAULT 0,
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
  `NomeTecnico` varchar(45) NOT NULL,
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
 (1,'sa','0000-00-00 00:00:00'),
 (2,'cl','0000-00-00 00:00:00'),
 (3,'fl','0000-00-00 00:00:00'),
 (4,'ma','0000-00-00 00:00:00'),
 (5,'pa','0000-00-00 00:00:00'),
 (6,'mh','0000-00-00 00:00:00'),
 (7,'bi','0000-00-00 00:00:00'),
 (8,'u_marcacao','0000-00-00 00:00:00'),
 (9,'u_mtecnicos','0000-00-00 00:00:00');
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
  `CorCalendario` varchar(45) DEFAULT NULL,
  `IniciaisUtilizador` varchar(10) DEFAULT NULL,
  PRIMARY KEY (`IdUtilizador`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8;

--
-- Dumping data for table `sys_utilizadores`
--

/*!40000 ALTER TABLE `sys_utilizadores` DISABLE KEYS */;
INSERT INTO `sys_utilizadores` (`IdUtilizador`,`NomeUtilizador`,`Password`,`NomeCompleto`,`TipoUtilizador`,`EmailUtilizador`,`IdCartaoTrello`,`timestamp`,`admin`,`enable`,`IdPHC`,`IdArmazem`,`CorCalendario`,`IniciaisUtilizador`) VALUES 
 (1,'jmonteiro','AQAAAAEAACcQAAAAELeq22xmE9gUCMQun/WymGZPjjucwx/ilg8x8M9V2p8mB2vn6KM0Yiy2DVOzOB31aw==','Jorge Monteiro','1','jmonteiro@food-tech.pt','',NULL,1,1,33,NULL,'#5DADE2','JM'),
 (2,'lfernandes','AQAAAAEAACcQAAAAEGZVKT2u/mis91pvh6f+nYWdf717pNoGPsYozocKDUgkmQuoGzXo3C1cmfbuLYa9ZA==','Luis Fernandes','1','lfernandes@food-tech.pt',NULL,NULL,1,1,20,NULL,'#CB4335','LF'),
 (3,'nmartins','AQAAAAEAACcQAAAAEFiEkz1uvmCRGLEvI6Bmbk9WEBcWWTcVYR18Bp4mY3wVLPKu+ffzpGd3fsi2HExmxw==','Nelson Martins','1','nmartins@food-tech.pt','59ccb51b751826836cfd4f95',NULL,0,0,40,NULL,'#48C9B0','NM'),
 (4,'ralmeida','AQAAAAEAACcQAAAAEAahhVEsmXwaU/80zNlihzpegFlL3He7ME06rE5SkYw/V4rctBUEF6RyspfjQOlKrg==','Ricardo Almeida','1','ralmeida@food-tech.pt','',NULL,0,0,34,NULL,'#A569BD','RA'),
 (5,'psantos','AQAAAAEAACcQAAAAEKJAEPtx976uQnH8ogf7juNT5YKqY+JswmSrl7fSh/vpbQi+bpr0RJYfIUKaANjYdQ==','Pedro Santos','1','psantos@food-tech.pt','5cffd63b18581a4e74da24a9',NULL,0,0,42,NULL,'#F4D03F','PS'),
 (6,'arodrigues','AQAAAAEAACcQAAAAEGb+vnSBz+OsooXZmpq/6ZPWkEOYSlNvPWkySdUky8tznkgEFNLxXKtjIj3tLxymyg==','Armando Rodrigues','0','arodrigues@food-tech.pt','59ccb52aa80fdc78530096ea',NULL,0,0,0,NULL,'	\r\n#F5B041','AR'),
 (7,'roliveira','AQAAAAEAACcQAAAAEA7IIX+6geOrhV3B7jp9TpDoopWYYTDfocrKjcacqUgq3sVwpHySxlTMVvo/php7Tw==','Ricardo Oliveira','1','hpinto@food-tech.pt','59ccb536db119ea33b01400e',NULL,0,0,43,NULL,'#EB984E','RO'),
 (8,'silvino','AQAAAAEAACcQAAAAEBBfOfMVcgM23Gg6dHJY/zXA+sGNhUpsQ01j3ZjGVNTeEoVk+YsFFbEJ6FXuEUYVBg==','Silvino','0','silvino@food-tech.pt',NULL,NULL,0,0,0,NULL,'#ECF0F1','SI'),
 (9,'dcorreia','AQAAAAEAACcQAAAAEGzOCegSc8kce1FXkJe/aZULradxJHOHmM8GqOb6wOgkD9Qa5dazX4ke3ahqtOpDSA==','Diogo Correia','1','dcorreia@food-tech.pt','59ccb5722a8d2ef7d1ce17fb',NULL,0,0,7,NULL,'#95A5A6','DC'),
 (10,'pdeus','AQAAAAEAACcQAAAAEARXJ+xEuLbpJu3S5sPkzPEJju1BMgKlf/M69TtGVAey/qqVJcINiU5Lp5yqA9VWNQ==','Paulo Deus','1','pdeus@food-tech.pt','5e4124fb4b50a44e13907669',NULL,0,0,41,NULL,'#2E4053','PD'),
 (11,'ddiogo','AQAAAAEAACcQAAAAEHBnt8rzwBHS+Km0cWiDFg7zttD7ZtI8yvmX+3EBdTkqEzGFzKVSExZx3dqEFGBfmQ==','Daniel Diogo','1','ddiogo@food-tech.pt','5db07146e80294682409b74e',NULL,0,0,44,NULL,'	\r\n#145A32','DD'),
 (12,'mferreira','AQAAAAEAACcQAAAAEGidEMqvkvZ78Dp9LY8aN8Ee9evNGqOCEKm+96tWDexuXvonazLIamJbz209G/CeDQ==','Mafalda Ferreira','3','mferreira@food-tech.pt',NULL,NULL,1,1,0,NULL,' ','MF'),
 (13,'cperes','AQAAAAEAACcQAAAAEIdBe7OJkMOy1j3pHhCFpT3RWrqigW/J82K0BZRdfsQZTJkC000XH423UKQ2jQi4Vg==','Cindy Peres','3','cperes@food-tech.pt',NULL,NULL,0,1,0,NULL,' ','CP'),
 (14,'pecas','AQAAAAEAACcQAAAAEDqkoqMObRWuK8JNAgwbRe3jewdzLpeCqyD3N+tM590Z3d0hTTFHM//Uk/RZM61RLA==','João Santos','3','pecas@food-tech.pt',NULL,NULL,0,1,45,NULL,'#7D6608','JS'),
 (15,'hcrispim','AQAAAAEAACcQAAAAEG2jClLgA9a/+Ahq+HXAYSEDaxbEzgOEZuDAJyf2nygBd+lfZNpgkCinEvktwdu1ig==','Henrique Crispim','1','sopesagem@gmail.com','5cf68fd1933da030a49330c6',NULL,0,0,9,NULL,'#17202A','HC'),
 (16,'fsoares','AQAAAAEAACcQAAAAEDqkoqMObRWuK8JNAgwbRe3jewdzLpeCqyD3N+tM590Z3d0hTTFHM//Uk/RZM61RLA==','Filipe Soares','2','fsoares@food-tech.pt',NULL,NULL,1,1,500,NULL,' ','FS');
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

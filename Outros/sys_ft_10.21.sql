-- phpMyAdmin SQL Dump
-- version 5.1.0
-- https://www.phpmyadmin.net/
--
-- Host: db
-- Generation Time: Oct 04, 2021 at 12:11 PM
-- Server version: 5.5.62
-- PHP Version: 7.4.16

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `sys_ft`
--

-- --------------------------------------------------------

--
-- Table structure for table `dat_armazem`
--

CREATE TABLE `dat_armazem` (
  `armazem_id` int(11) NOT NULL,
  `armazem_nome` varchar(256) NOT NULL DEFAULT ' '
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `dat_armazem`
--

INSERT INTO `dat_armazem` (`armazem_id`, `armazem_nome`) VALUES
(0, 'Não Definido'),
(1, 'Aveiro'),
(2, 'Alverca'),
(3, 'Maia'),
(4, 'Quimiloureiro'),
(31, 'Luis Fernandes'),
(32, 'Jorge Monteiro'),
(33, 'Helder Pinto'),
(34, 'Zarcofrio'),
(35, 'Pedro Santos'),
(36, 'Nelson Martins'),
(37, 'Praiotel'),
(38, 'Ricardo Oliveira'),
(39, 'Armando Rodrigues'),
(42, 'Paulo Deus'),
(43, 'Diogo Correia'),
(45, 'Daniel Diogo'),
(46, 'Henrique Crispim'),
(48, 'Ricardo Almeida'),
(99, 'Produção & Montagem');

-- --------------------------------------------------------

--
-- Table structure for table `dat_clientes`
--

CREATE TABLE `dat_clientes` (
  `IdCliente` int(10) UNSIGNED NOT NULL,
  `IdLoja` int(10) UNSIGNED NOT NULL,
  `NomeCliente` varchar(1024) NOT NULL,
  `PessoaContactoCliente` varchar(45) DEFAULT NULL,
  `MoradaCliente` varchar(1024) DEFAULT NULL,
  `EmailCliente` varchar(1024) DEFAULT NULL,
  `NumeroContribuinteCliente` varchar(45) DEFAULT NULL,
  `Telefone` varchar(45) DEFAULT NULL,
  `IdVendedor` int(10) UNSIGNED NOT NULL,
  `TipoCliente` varchar(45) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `dat_controlo_viatura`
--

CREATE TABLE `dat_controlo_viatura` (
  `id_reg` int(10) UNSIGNED NOT NULL,
  `nome_tecnico` varchar(45) NOT NULL,
  `matricula_viatura` varchar(45) NOT NULL,
  `kms_viatura` varchar(45) NOT NULL,
  `data_inicio` datetime NOT NULL,
  `devolvida_viatura` tinyint(1) DEFAULT NULL,
  `notas_viatura` varchar(1024) DEFAULT NULL,
  `data_fim` datetime DEFAULT NULL,
  `kms_finais` varchar(45) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `dat_controlo_viatura`
--

INSERT INTO `dat_controlo_viatura` (`id_reg`, `nome_tecnico`, `matricula_viatura`, `kms_viatura`, `data_inicio`, `devolvida_viatura`, `notas_viatura`, `data_fim`, `kms_finais`) VALUES
(1, '', '26-89-OF', '200000', '2021-02-08 09:00:00', 0, NULL, '2021-02-08 09:00:00', '200000'),
(2, 'Pedro Santos', 'AF-02-BT', '45', '2021-02-08 09:00:00', NULL, 'Ponte de Lima', '2021-02-08 18:30:00', '300'),
(3, 'Nelson Martins', 'AF-02-BT', '300', '2021-02-08 18:30:00', NULL, 'PD Marco de Canaveses ', '2021-02-09 18:30:00', '422'),
(4, 'Jorge Monteiro', 'AF-02-BT', '3421', '2021-04-19 08:50:00', 1, 'CBD Mercado', '2021-04-19 13:11:00', '3450'),
(5, 'Jorge Monteiro', 'AF-02-BT', '5300', '2021-05-11 16:00:00', 1, 'Lisboa', '2021-05-13 20:00:00', '5800'),
(6, 'Luis Fernandes', 'AF-02-BT', '5800', '2021-05-12 20:00:00', 1, 'Lisboa', '2021-05-13 17:02:00', '6210'),
(7, 'Jorge Monteiro', 'AF-02-BT', '6210', '2021-05-13 17:02:00', 1, 'Lisboa', '2021-05-14 08:09:00', '6265');

-- --------------------------------------------------------

--
-- Table structure for table `dat_equipamentos`
--

CREATE TABLE `dat_equipamentos` (
  `NumeroSerieEquipamento` varchar(250) NOT NULL,
  `DesignacaoEquipamento` varchar(45) DEFAULT NULL,
  `MarcaEquipamento` varchar(45) DEFAULT NULL,
  `ModeloEquipamento` varchar(45) DEFAULT NULL,
  `IdCliente` int(11) DEFAULT NULL,
  `IdLoja` int(11) DEFAULT NULL,
  `IdFornecedor` int(10) UNSIGNED DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `IdEquipamento` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `dat_folhas_obra`
--

CREATE TABLE `dat_folhas_obra` (
  `IdFolhaObra` int(10) UNSIGNED NOT NULL,
  `DataServico` datetime NOT NULL,
  `ReferenciaServico` varchar(45) DEFAULT NULL,
  `EstadoEquipamento` varchar(45) DEFAULT NULL,
  `SituacoesPendentes` varchar(1024) DEFAULT NULL,
  `IdEquipamento` varchar(50) NOT NULL,
  `IdCliente` int(10) UNSIGNED NOT NULL,
  `IdLoja` int(10) UNSIGNED NOT NULL,
  `IdCartaoTrello` varchar(100) DEFAULT NULL,
  `ConferidoPor` varchar(250) DEFAULT NULL,
  `GuiaTransporteAtual` varchar(100) DEFAULT NULL,
  `Remoto` tinyint(1) NOT NULL DEFAULT '0',
  `RubricaCliente` blob,
  `EstadoFolhaObra` varchar(45) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `dat_fornecedores`
--

CREATE TABLE `dat_fornecedores` (
  `IdFornecedor` int(10) UNSIGNED NOT NULL,
  `NomeFornecedor` varchar(45) DEFAULT NULL,
  `MoradaFornecedor` varchar(120) DEFAULT NULL,
  `ContactoFornecedor` varchar(45) DEFAULT NULL,
  `ReferenciaFornecedor` varchar(45) DEFAULT NULL,
  `EmailFornecedor` varchar(120) DEFAULT NULL,
  `PessoaContactoFornecedor` varchar(45) DEFAULT NULL,
  `Obs` varchar(250) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Table structure for table `dat_intervencoes_folha_obra`
--

CREATE TABLE `dat_intervencoes_folha_obra` (
  `IdIntervencao` int(10) UNSIGNED NOT NULL,
  `IdTecnico` int(10) UNSIGNED NOT NULL,
  `IdFolhaObra` int(10) UNSIGNED NOT NULL,
  `NomeTecnico` varchar(1024) NOT NULL,
  `RelatorioServico` varchar(1024) DEFAULT NULL,
  `DataServico` datetime NOT NULL,
  `HoraInicio` varchar(45) NOT NULL,
  `HoraFim` varchar(45) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `dat_logs`
--

CREATE TABLE `dat_logs` (
  `id_log` int(10) UNSIGNED NOT NULL,
  `user` varchar(100) NOT NULL,
  `data_log` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `msg_log` varchar(1024) NOT NULL,
  `tipo_log` int(11) NOT NULL COMMENT '1 - Stocks 2 - Impressoes, 3 - Folhas de Obra, 4 - Logins'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `dat_marcacoes`
--

CREATE TABLE `dat_marcacoes` (
  `IdMarcacao` int(10) UNSIGNED NOT NULL,
  `DataMarcacao` datetime NOT NULL,
  `IdCliente` int(10) UNSIGNED NOT NULL,
  `IdLoja` int(10) UNSIGNED NOT NULL,
  `TipoMarcacao` tinyint(1) NOT NULL DEFAULT '1',
  `ResumoMarcacao` varchar(1024) DEFAULT NULL,
  `EstadoMarcacao` varchar(45) DEFAULT NULL,
  `PrioridadeMarcacao` varchar(45) DEFAULT NULL,
  `MarcacaoStamp` varchar(45) DEFAULT NULL,
  `TipoEquipamento` varchar(45) NOT NULL,
  `Oficina` tinyint(1) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Table structure for table `dat_marcacoes_tecnico`
--

CREATE TABLE `dat_marcacoes_tecnico` (
  `IdMarcacaoTecnico` varchar(45) NOT NULL,
  `MarcacaoStamp` varchar(45) NOT NULL,
  `IdTecnico` int(10) UNSIGNED NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `NomeTecnico` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Table structure for table `dat_produtos`
--

CREATE TABLE `dat_produtos` (
  `ref_produto` varchar(254) NOT NULL,
  `designacao_produto` varchar(1024) DEFAULT NULL,
  `stock_fisico` double DEFAULT NULL,
  `stock_phc` double DEFAULT NULL,
  `stock_rec` double DEFAULT NULL,
  `stock_res` double DEFAULT NULL,
  `armazem_id` int(11) NOT NULL,
  `modificado` tinyint(1) NOT NULL DEFAULT '0',
  `pos_stock` varchar(45) DEFAULT NULL,
  `obs` varchar(1024) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `dat_produto_intervencao`
--

CREATE TABLE `dat_produto_intervencao` (
  `RefProduto` varchar(250) NOT NULL,
  `Designacao` varchar(1024) NOT NULL,
  `Quantidade` float UNSIGNED ZEROFILL NOT NULL,
  `tipoun` varchar(11) NOT NULL DEFAULT 'UN',
  `IdFolhaObra` int(10) UNSIGNED NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `dat_recibos`
--

CREATE TABLE `dat_recibos` (
  `IdRecibo` int(11) UNSIGNED NOT NULL,
  `MaoObra` double NOT NULL DEFAULT '0',
  `Deslocacao` double NOT NULL DEFAULT '0',
  `MaterialAplicado` double NOT NULL DEFAULT '0',
  `IdFolhaObra` int(11) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `dat_vendedores`
--

CREATE TABLE `dat_vendedores` (
  `IdVendedor` int(10) UNSIGNED NOT NULL,
  `NomeVendedor` varchar(45) DEFAULT NULL,
  `uid` varchar(45) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Table structure for table `dat_viaturas`
--

CREATE TABLE `dat_viaturas` (
  `matricula_viatura` varchar(45) NOT NULL,
  `marca` varchar(45) DEFAULT NULL,
  `modelo` varchar(45) DEFAULT NULL,
  `responsavel` varchar(45) DEFAULT NULL,
  `ultimoKms` varchar(45) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `dat_viaturas`
--

INSERT INTO `dat_viaturas` (`matricula_viatura`, `marca`, `modelo`, `responsavel`, `ultimoKms`) VALUES
('26-89-OF', 'Volwswgwen', 'LT3', '', ''),
('AF-02-BT', 'Opel', NULL, NULL, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `sys_tabelas`
--

CREATE TABLE `sys_tabelas` (
  `idtabela` int(10) UNSIGNED NOT NULL,
  `nometabela` varchar(45) NOT NULL,
  `ultimamodificacao` datetime NOT NULL DEFAULT '1900-01-01 00:00:00'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Dumping data for table `sys_tabelas`
--

INSERT INTO `sys_tabelas` (`idtabela`, `nometabela`, `ultimamodificacao`) VALUES
(1, 'sa', '2021-10-04 00:00:00'),
(2, 'cl', '2021-10-01 00:00:00'),
(3, 'fl', '2021-09-30 00:00:00'),
(4, 'ma', '2021-10-01 00:00:00'),
(5, 'pa', '2021-10-01 00:00:00'),
(6, 'mh', '2021-10-01 00:00:00'),
(7, 'bi', '2021-10-01 00:00:00'),
(8, 'u_marcacao', '2021-10-01 00:00:00'),
(9, 'u_mtecnicos', '2021-10-01 00:00:00');

-- --------------------------------------------------------

--
-- Table structure for table `sys_utilizadores`
--

CREATE TABLE `sys_utilizadores` (
  `IdUtilizador` int(10) UNSIGNED NOT NULL,
  `NomeUtilizador` varchar(45) NOT NULL,
  `Password` varchar(1024) NOT NULL,
  `NomeCompleto` varchar(250) NOT NULL,
  `TipoUtilizador` varchar(45) NOT NULL,
  `EmailUtilizador` varchar(250) DEFAULT NULL,
  `IdCartaoTrello` varchar(100) DEFAULT NULL,
  `timestamp` timestamp NULL DEFAULT NULL,
  `admin` tinyint(1) NOT NULL DEFAULT '0',
  `enable` tinyint(1) NOT NULL DEFAULT '1',
  `IdPHC` int(10) UNSIGNED DEFAULT '0',
  `IdArmazem` int(10) UNSIGNED DEFAULT NULL,
  `IniciaisUtilizador` varchar(2) NOT NULL,
  `CorCalendario` varchar(10) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `sys_utilizadores`
--

INSERT INTO `sys_utilizadores` (`IdUtilizador`, `NomeUtilizador`, `Password`, `NomeCompleto`, `TipoUtilizador`, `EmailUtilizador`, `IdCartaoTrello`, `timestamp`, `admin`, `enable`, `IdPHC`, `IdArmazem`, `IniciaisUtilizador`, `CorCalendario`) VALUES
(1, 'jmonteiro', 'AQAAAAEAACcQAAAAELeq22xmE9gUCMQun/WymGZPjjucwx/ilg8x8M9V2p8mB2vn6KM0Yiy2DVOzOB31aw==', 'Jorge Monteiro', '1', 'jmonteiro@food-tech.pt', '', NULL, 1, 1, 33, NULL, 'JM', '#5DADE2'),
(2, 'lfernandes', 'AQAAAAEAACcQAAAAEGZVKT2u/mis91pvh6f+nYWdf717pNoGPsYozocKDUgkmQuoGzXo3C1cmfbuLYa9ZA==', 'Luis Fernandes', '1', 'lfernandes@food-tech.pt', NULL, NULL, 1, 1, 20, NULL, 'LF', '#CB4335'),
(3, 'nmartins', 'AQAAAAEAACcQAAAAEFiEkz1uvmCRGLEvI6Bmbk9WEBcWWTcVYR18Bp4mY3wVLPKu+ffzpGd3fsi2HExmxw==', 'Nelson Martins', '1', 'nmartins@food-tech.pt', '59ccb51b751826836cfd4f95', NULL, 0, 0, 40, NULL, 'NM', '#48C9B0'),
(4, 'ralmeida', 'AQAAAAEAACcQAAAAEAahhVEsmXwaU/80zNlihzpegFlL3He7ME06rE5SkYw/V4rctBUEF6RyspfjQOlKrg==', 'Ricardo Almeida', '1', 'ralmeida@food-tech.pt', '', NULL, 0, 0, 34, NULL, 'RA', '#A569BD'),
(5, 'psantos', 'AQAAAAEAACcQAAAAEKJAEPtx976uQnH8ogf7juNT5YKqY+JswmSrl7fSh/vpbQi+bpr0RJYfIUKaANjYdQ==', 'Pedro Santos', '1', 'psantos@food-tech.pt', '5cffd63b18581a4e74da24a9', NULL, 0, 0, 42, NULL, 'PS', '#F4D03F'),
(6, 'arodrigues', 'AQAAAAEAACcQAAAAEGb+vnSBz+OsooXZmpq/6ZPWkEOYSlNvPWkySdUky8tznkgEFNLxXKtjIj3tLxymyg==', 'Armando Rodrigues', '1', 'arodrigues@food-tech.pt', '59ccb52aa80fdc78530096ea', NULL, 0, 0, 0, NULL, 'AR', '#F5B041'),
(7, 'roliveira', 'AQAAAAEAACcQAAAAEA7IIX+6geOrhV3B7jp9TpDoopWYYTDfocrKjcacqUgq3sVwpHySxlTMVvo/php7Tw==', 'Ricardo Oliveira', '1', 'hpinto@food-tech.pt', '59ccb536db119ea33b01400e', NULL, 0, 0, 43, NULL, 'RO', '#EB984E'),
(8, 'silvino', 'AQAAAAEAACcQAAAAEBBfOfMVcgM23Gg6dHJY/zXA+sGNhUpsQ01j3ZjGVNTeEoVk+YsFFbEJ6FXuEUYVBg==', 'Silvino', '1', 'silvino@food-tech.pt', NULL, NULL, 0, 0, 0, NULL, 'SI', '#ECF0F1'),
(9, 'dcorreia', 'AQAAAAEAACcQAAAAEGzOCegSc8kce1FXkJe/aZULradxJHOHmM8GqOb6wOgkD9Qa5dazX4ke3ahqtOpDSA==', 'Diogo Correia', '1', 'dcorreia@food-tech.pt', '59ccb5722a8d2ef7d1ce17fb', NULL, 0, 0, 7, NULL, 'DC', '#95A5A6'),
(10, 'pdeus', 'AQAAAAEAACcQAAAAEARXJ+xEuLbpJu3S5sPkzPEJju1BMgKlf/M69TtGVAey/qqVJcINiU5Lp5yqA9VWNQ==', 'Paulo Deus', '1', 'pdeus@food-tech.pt', '5e4124fb4b50a44e13907669', NULL, 0, 0, 41, NULL, 'PD', '#2E4053'),
(11, 'ddiogo', 'AQAAAAEAACcQAAAAEHBnt8rzwBHS+Km0cWiDFg7zttD7ZtI8yvmX+3EBdTkqEzGFzKVSExZx3dqEFGBfmQ==', 'Daniel Diogo', '1', 'ddiogo@food-tech.pt', '5db07146e80294682409b74e', NULL, 0, 0, 44, NULL, 'DD', '#145A32'),
(12, 'mferreira', 'AQAAAAEAACcQAAAAEGidEMqvkvZ78Dp9LY8aN8Ee9evNGqOCEKm+96tWDexuXvonazLIamJbz209G/CeDQ==', 'Mafalda Ferreira', '3', 'mferreira@food-tech.pt', NULL, NULL, 1, 1, 0, NULL, 'MF', ''),
(13, 'cperes', 'AQAAAAEAACcQAAAAEIdBe7OJkMOy1j3pHhCFpT3RWrqigW/J82K0BZRdfsQZTJkC000XH423UKQ2jQi4Vg==', 'Cindy Peres', '3', 'cperes@food-tech.pt', NULL, NULL, 0, 1, 0, NULL, 'CP', ''),
(14, 'pecas', 'AQAAAAEAACcQAAAAEDqkoqMObRWuK8JNAgwbRe3jewdzLpeCqyD3N+tM590Z3d0hTTFHM//Uk/RZM61RLA==', 'João Santos', '3', 'pecas@food-tech.pt', NULL, NULL, 0, 1, 45, NULL, 'JS', '#7D6608'),
(15, 'hcrispim', 'AQAAAAEAACcQAAAAEG2jClLgA9a/+Ahq+HXAYSEDaxbEzgOEZuDAJyf2nygBd+lfZNpgkCinEvktwdu1ig==', 'Henrique Crispim', '1', 'sopesagem@gmail.com', '5cf68fd1933da030a49330c6', NULL, 0, 0, 9, NULL, 'HC', '#17202A'),
(16, 'fsoares', 'AQAAAAEAACcQAAAAELeq22xmE9gUCMQun/WymGZPjjucwx/ilg8x8M9V2p8mB2vn6KM0Yiy2DVOzOB31aw==', 'Filipe Soares', '2', 'fsoares@food-tech.pt', NULL, NULL, 1, 1, 500, NULL, 'FS', ''),
(17, 'estagiario', '', 'Estagiario', '1', NULL, NULL, NULL, 0, 1, 32, NULL, 'TE', '#7D6608'),
(18, 'asousa', '', 'André Sousa', '1', 'asousa@food-tech.pt', NULL, NULL, 0, 1, 47, NULL, 'AS', '#7D6608'),
(19, 'asilva', '', 'Avelino Silva', '1', 'assistecnica@zarcofrio.pt', NULL, NULL, 0, 1, 18, NULL, 'ZA', '#7D6608');

-- --------------------------------------------------------

--
-- Table structure for table `sys_viaturas`
--

CREATE TABLE `sys_viaturas` (
  `matricula_viatura` varchar(45) NOT NULL,
  `marca` varchar(45) DEFAULT NULL,
  `modelo` varchar(45) DEFAULT NULL,
  `responsavel` varchar(45) DEFAULT NULL,
  `ultimoKms` varchar(45) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `sys_viaturas`
--

INSERT INTO `sys_viaturas` (`matricula_viatura`, `marca`, `modelo`, `responsavel`, `ultimoKms`) VALUES
('26-89-OF', 'Volwswgwen', 'LT3', '', ''),
('AF-02-BT', 'Opel', '', '', '');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `dat_armazem`
--
ALTER TABLE `dat_armazem`
  ADD PRIMARY KEY (`armazem_id`);

--
-- Indexes for table `dat_clientes`
--
ALTER TABLE `dat_clientes`
  ADD PRIMARY KEY (`IdCliente`,`IdLoja`) USING BTREE;

--
-- Indexes for table `dat_controlo_viatura`
--
ALTER TABLE `dat_controlo_viatura`
  ADD PRIMARY KEY (`id_reg`);

--
-- Indexes for table `dat_equipamentos`
--
ALTER TABLE `dat_equipamentos`
  ADD PRIMARY KEY (`IdEquipamento`);

--
-- Indexes for table `dat_folhas_obra`
--
ALTER TABLE `dat_folhas_obra`
  ADD PRIMARY KEY (`IdFolhaObra`);

--
-- Indexes for table `dat_fornecedores`
--
ALTER TABLE `dat_fornecedores`
  ADD PRIMARY KEY (`IdFornecedor`);

--
-- Indexes for table `dat_intervencoes_folha_obra`
--
ALTER TABLE `dat_intervencoes_folha_obra`
  ADD PRIMARY KEY (`IdIntervencao`);

--
-- Indexes for table `dat_logs`
--
ALTER TABLE `dat_logs`
  ADD PRIMARY KEY (`id_log`);

--
-- Indexes for table `dat_marcacoes`
--
ALTER TABLE `dat_marcacoes`
  ADD PRIMARY KEY (`IdMarcacao`) USING BTREE;

--
-- Indexes for table `dat_marcacoes_tecnico`
--
ALTER TABLE `dat_marcacoes_tecnico`
  ADD PRIMARY KEY (`IdMarcacaoTecnico`);

--
-- Indexes for table `dat_produtos`
--
ALTER TABLE `dat_produtos`
  ADD PRIMARY KEY (`ref_produto`,`armazem_id`);

--
-- Indexes for table `dat_produto_intervencao`
--
ALTER TABLE `dat_produto_intervencao`
  ADD PRIMARY KEY (`RefProduto`,`IdFolhaObra`);

--
-- Indexes for table `dat_recibos`
--
ALTER TABLE `dat_recibos`
  ADD PRIMARY KEY (`IdRecibo`);

--
-- Indexes for table `dat_vendedores`
--
ALTER TABLE `dat_vendedores`
  ADD PRIMARY KEY (`IdVendedor`);

--
-- Indexes for table `dat_viaturas`
--
ALTER TABLE `dat_viaturas`
  ADD PRIMARY KEY (`matricula_viatura`);

--
-- Indexes for table `sys_tabelas`
--
ALTER TABLE `sys_tabelas`
  ADD PRIMARY KEY (`idtabela`);

--
-- Indexes for table `sys_utilizadores`
--
ALTER TABLE `sys_utilizadores`
  ADD PRIMARY KEY (`IdUtilizador`);

--
-- Indexes for table `sys_viaturas`
--
ALTER TABLE `sys_viaturas`
  ADD PRIMARY KEY (`matricula_viatura`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `dat_clientes`
--
ALTER TABLE `dat_clientes`
  MODIFY `IdCliente` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `dat_controlo_viatura`
--
ALTER TABLE `dat_controlo_viatura`
  MODIFY `id_reg` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT for table `dat_folhas_obra`
--
ALTER TABLE `dat_folhas_obra`
  MODIFY `IdFolhaObra` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `dat_intervencoes_folha_obra`
--
ALTER TABLE `dat_intervencoes_folha_obra`
  MODIFY `IdIntervencao` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `dat_logs`
--
ALTER TABLE `dat_logs`
  MODIFY `id_log` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `sys_tabelas`
--
ALTER TABLE `sys_tabelas`
  MODIFY `idtabela` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT for table `sys_utilizadores`
--
ALTER TABLE `sys_utilizadores`
  MODIFY `IdUtilizador` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=20;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

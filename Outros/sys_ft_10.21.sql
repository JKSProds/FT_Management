-- phpMyAdmin SQL Dump
-- version 5.1.0
-- https://www.phpmyadmin.net/
--
-- Host: db
-- Tempo de geração: 01-Nov-2021 às 09:57
-- Versão do servidor: 5.5.62
-- versão do PHP: 7.4.16

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Banco de dados: `sys_ft`
--

-- --------------------------------------------------------

--
-- Estrutura da tabela `dat_armazem`
--

CREATE TABLE `dat_armazem` (
  `armazem_id` int(11) NOT NULL,
  `armazem_nome` varchar(256) NOT NULL DEFAULT ' '
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Extraindo dados da tabela `dat_armazem`
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
-- Estrutura da tabela `dat_clientes`
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
-- Estrutura da tabela `dat_controlo_viatura`
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

-- --------------------------------------------------------

--
-- Estrutura da tabela `dat_equipamentos`
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
-- Estrutura da tabela `dat_folhas_obra`
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
-- Estrutura da tabela `dat_fornecedores`
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
-- Estrutura da tabela `dat_intervencoes_folha_obra`
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
-- Estrutura da tabela `dat_logs`
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
-- Estrutura da tabela `dat_marcacoes`
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
-- Estrutura da tabela `dat_marcacoes_tecnico`
--

CREATE TABLE `dat_marcacoes_tecnico` (
  `IdMarcacaoTecnico` varchar(45) NOT NULL,
  `MarcacaoStamp` varchar(45) NOT NULL,
  `IdTecnico` int(10) UNSIGNED NOT NULL,
  `Marcado` tinyint(1) NOT NULL DEFAULT '0',
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `NomeTecnico` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Estrutura da tabela `dat_produtos`
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
-- Estrutura da tabela `dat_produto_intervencao`
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
-- Estrutura da tabela `dat_propostas`
--

CREATE TABLE `dat_propostas` (
  `IdProposta` int(10) UNSIGNED NOT NULL,
  `IdVisita` int(10) UNSIGNED NOT NULL,
  `IdComercial` int(10) UNSIGNED NOT NULL,
  `DataProposta` datetime NOT NULL,
  `EstadoProposta` varchar(45) NOT NULL,
  `ValorProposta` varchar(45) DEFAULT NULL,
  `UrlAnexo` varchar(500) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estrutura da tabela `dat_recibos`
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
-- Estrutura da tabela `dat_vendedores`
--

CREATE TABLE `dat_vendedores` (
  `IdVendedor` int(10) UNSIGNED NOT NULL,
  `NomeVendedor` varchar(45) DEFAULT NULL,
  `uid` varchar(45) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Estrutura da tabela `dat_visitas`
--

CREATE TABLE `dat_visitas` (
  `IdVisita` int(10) UNSIGNED NOT NULL,
  `DataVisita` datetime NOT NULL,
  `IdCliente` int(10) UNSIGNED NOT NULL,
  `IdLoja` int(10) UNSIGNED NOT NULL,
  `ResumoVisita` varchar(1024) DEFAULT NULL,
  `EstadoVisita` varchar(45) DEFAULT NULL,
  `ObsVisita` varchar(1024) DEFAULT NULL,
  `VisitaStamp` varchar(250) DEFAULT NULL,
  `IdComercial` int(10) UNSIGNED NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estrutura da tabela `sys_tabelas`
--

CREATE TABLE `sys_tabelas` (
  `idtabela` int(10) UNSIGNED NOT NULL,
  `nometabela` varchar(45) NOT NULL,
  `ultimamodificacao` datetime NOT NULL DEFAULT '1900-01-01 00:00:00'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Extraindo dados da tabela `sys_tabelas`
--

INSERT INTO `sys_tabelas` (`idtabela`, `nometabela`, `ultimamodificacao`) VALUES
(1, 'sa', '2021-10-31 00:00:00'),
(2, 'cl', '2021-11-01 00:00:00'),
(3, 'fl', '2021-10-31 00:00:00'),
(4, 'ma', '2021-11-01 00:00:00'),
(5, 'pa', '2021-11-01 00:00:00'),
(6, 'mh', '2021-11-01 00:00:00'),
(7, 'bi', '2021-11-01 00:00:00'),
(8, 'u_marcacao', '2021-11-01 00:00:00'),
(9, 'u_mtecnicos', '2021-11-01 00:00:00');

-- --------------------------------------------------------

--
-- Estrutura da tabela `sys_utilizadores`
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
-- Extraindo dados da tabela `sys_utilizadores`
--

INSERT INTO `sys_utilizadores` (`IdUtilizador`, `NomeUtilizador`, `Password`, `NomeCompleto`, `TipoUtilizador`, `EmailUtilizador`, `IdCartaoTrello`, `timestamp`, `admin`, `enable`, `IdPHC`, `IdArmazem`, `IniciaisUtilizador`, `CorCalendario`) VALUES
(1, 'jmonteiro', 'AQAAAAEAACcQAAAAEO352NvY5bM5+pSNCAHnkwBej8DpM1qOrEoKUrn5GqqYEBbGrc106rxgccCT8pZrVA==', 'Jorge Monteiro', '1', 'jmonteiro@food-tech.pt', '', NULL, 1, 1, 33, NULL, 'JM', '#5DADE2'),
(2, 'lfernandes', 'AQAAAAEAACcQAAAAEGa5Ya1Onll8wfY+0/pu9SHOohK0xeTF46/LZo7lAPtHDpC80VF80m/yuuMghey3NA==', 'Luis Fernandes', '1', 'lfernandes@food-tech.pt', NULL, NULL, 1, 1, 20, NULL, 'LF', '#CB4335'),
(3, 'nmartins', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Nelson Martins', '1', 'nmartins@food-tech.pt', '59ccb51b751826836cfd4f95', NULL, 0, 0, 40, NULL, 'NM', '#48C9B0'),
(4, 'ralmeida', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Ricardo Almeida', '1', 'ralmeida@food-tech.pt', '', NULL, 0, 0, 34, NULL, 'RA', '#A569BD'),
(5, 'psantos', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Pedro Santos', '1', 'psantos@food-tech.pt', '5cffd63b18581a4e74da24a9', NULL, 0, 0, 42, NULL, 'PS', '#F4D03F'),
(6, 'arodrigues', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Armando Rodrigues', '1', 'arodrigues@food-tech.pt', '59ccb52aa80fdc78530096ea', NULL, 0, 0, 0, NULL, 'AR', '#F5B041'),
(7, 'roliveira', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Ricardo Oliveira', '1', 'hpinto@food-tech.pt', '59ccb536db119ea33b01400e', NULL, 0, 0, 43, NULL, 'RO', '#EB984E'),
(8, 'silvino', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Silvino', '1', 'silvino@food-tech.pt', NULL, NULL, 0, 0, 0, NULL, 'SI', '#ECF0F1'),
(9, 'dcorreia', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Diogo Correia', '1', 'dcorreia@food-tech.pt', '59ccb5722a8d2ef7d1ce17fb', NULL, 0, 0, 7, NULL, 'DC', '#95A5A6'),
(10, 'pdeus', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Paulo Deus', '1', 'pdeus@food-tech.pt', '5e4124fb4b50a44e13907669', NULL, 0, 0, 41, NULL, 'PD', '#2E4053'),
(11, 'ddiogo', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Daniel Diogo', '1', 'ddiogo@food-tech.pt', '5db07146e80294682409b74e', NULL, 0, 0, 44, NULL, 'DD', '#145A32'),
(12, 'mferreira', 'AQAAAAEAACcQAAAAEKdhkrmdPgvJI+LEmvEHDj1lWi0vNb6ckMQjMdcba7RH9FnA1VXZS/pUivfoZFkj8g==', 'Mafalda Ferreira', '3', 'mferreira@food-tech.pt', NULL, NULL, 1, 1, 0, NULL, 'MF', ''),
(13, 'cperes', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Cindy Peres', '3', 'cperes@food-tech.pt', NULL, NULL, 0, 1, 0, NULL, 'CP', ''),
(14, 'pecas', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'João Santos', '3', 'pecas@food-tech.pt', NULL, NULL, 0, 1, 45, NULL, 'JS', '#7D6608'),
(15, 'hcrispim', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Henrique Crispim', '1', 'sopesagem@gmail.com', '5cf68fd1933da030a49330c6', NULL, 0, 0, 9, NULL, 'HC', '#17202A'),
(16, 'fsoares', 'AQAAAAEAACcQAAAAEC9lfm4fJqtSTbUYQF/SxJh4E97rLQrjhUrp1PZYTtXVoJsFLUXQ/bZW8EbazCK69g==', 'Filipe Soares', '2', 'fsoares@food-tech.pt', NULL, NULL, 1, 1, 925, NULL, 'FS', ''),
(17, 'estagiario', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Estagiário', '1', NULL, NULL, NULL, 0, 0, 32, NULL, 'TE', '#7D6608'),
(18, 'asousa', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'André Sousa', '1', 'asousa@food-tech.pt', NULL, NULL, 0, 0, 47, NULL, 'AS', '#7D6608'),
(19, 'asilva', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Avelino Silva', '1', 'assistecnica@zarcofrio.pt', NULL, NULL, 0, 0, 18, NULL, 'ZA', '#7D6608'),
(20, 'efalcao', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Eduardo Falcão', '2', 'efalcao@food-tech.pt', NULL, NULL, 0, 0, 1006, NULL, 'EF', ' '),
(21, 'dteixeira', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Davi Teixeira', '2', 'dteixeira@food-tech.pt', NULL, NULL, 0, 0, 540, NULL, 'DT', ' '),
(22, 'jcalheiros', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'João Calheiros', '2', 'jcalheiros@food-tech.pt', NULL, NULL, 0, 0, 1001, NULL, 'JC', ' '),
(23, 'lfaria', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Luis Faria', '2', NULL, NULL, NULL, 0, 0, 1012, NULL, 'LF', ' '),
(24, 'acarneiro', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Artur Carneiro', '2', 'acarneiro@food-tech.pt', NULL, NULL, 0, 0, 1010, NULL, 'AC', ' '),
(25, 'prosinha', 'AQAAAAEAACcQAAAAELkRkxamkzp897Dihc32hZMMoTW9cw5Qbq2N2ucOVH1aKn51oV6MPM1O145VANR9aw==', 'Pedro Rosinha', '1', 'prosinha@food-tech.pt', NULL, NULL, 0, 0, 46, NULL, 'PR', ' ');

-- --------------------------------------------------------

--
-- Estrutura da tabela `sys_viaturas`
--

CREATE TABLE `sys_viaturas` (
  `matricula_viatura` varchar(45) NOT NULL,
  `marca` varchar(45) DEFAULT NULL,
  `modelo` varchar(45) DEFAULT NULL,
  `responsavel` varchar(45) DEFAULT NULL,
  `ultimoKms` varchar(45) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Extraindo dados da tabela `sys_viaturas`
--

INSERT INTO `sys_viaturas` (`matricula_viatura`, `marca`, `modelo`, `responsavel`, `ultimoKms`) VALUES
('26-89-OF', 'Volwswgwen', 'LT3', '', ''),
('AF-02-BT', 'Opel', '', '', '');

--
-- Índices para tabelas despejadas
--

--
-- Índices para tabela `dat_armazem`
--
ALTER TABLE `dat_armazem`
  ADD PRIMARY KEY (`armazem_id`);

--
-- Índices para tabela `dat_clientes`
--
ALTER TABLE `dat_clientes`
  ADD PRIMARY KEY (`IdCliente`,`IdLoja`) USING BTREE;

--
-- Índices para tabela `dat_controlo_viatura`
--
ALTER TABLE `dat_controlo_viatura`
  ADD PRIMARY KEY (`id_reg`);

--
-- Índices para tabela `dat_equipamentos`
--
ALTER TABLE `dat_equipamentos`
  ADD PRIMARY KEY (`IdEquipamento`);

--
-- Índices para tabela `dat_folhas_obra`
--
ALTER TABLE `dat_folhas_obra`
  ADD PRIMARY KEY (`IdFolhaObra`);

--
-- Índices para tabela `dat_fornecedores`
--
ALTER TABLE `dat_fornecedores`
  ADD PRIMARY KEY (`IdFornecedor`);

--
-- Índices para tabela `dat_intervencoes_folha_obra`
--
ALTER TABLE `dat_intervencoes_folha_obra`
  ADD PRIMARY KEY (`IdIntervencao`);

--
-- Índices para tabela `dat_logs`
--
ALTER TABLE `dat_logs`
  ADD PRIMARY KEY (`id_log`);

--
-- Índices para tabela `dat_marcacoes`
--
ALTER TABLE `dat_marcacoes`
  ADD PRIMARY KEY (`IdMarcacao`) USING BTREE;

--
-- Índices para tabela `dat_marcacoes_tecnico`
--
ALTER TABLE `dat_marcacoes_tecnico`
  ADD PRIMARY KEY (`IdMarcacaoTecnico`);

--
-- Índices para tabela `dat_produtos`
--
ALTER TABLE `dat_produtos`
  ADD PRIMARY KEY (`ref_produto`,`armazem_id`);

--
-- Índices para tabela `dat_produto_intervencao`
--
ALTER TABLE `dat_produto_intervencao`
  ADD PRIMARY KEY (`RefProduto`,`IdFolhaObra`);

--
-- Índices para tabela `dat_propostas`
--
ALTER TABLE `dat_propostas`
  ADD PRIMARY KEY (`IdProposta`);

--
-- Índices para tabela `dat_recibos`
--
ALTER TABLE `dat_recibos`
  ADD PRIMARY KEY (`IdRecibo`);

--
-- Índices para tabela `dat_vendedores`
--
ALTER TABLE `dat_vendedores`
  ADD PRIMARY KEY (`IdVendedor`);

--
-- Índices para tabela `dat_visitas`
--
ALTER TABLE `dat_visitas`
  ADD PRIMARY KEY (`IdVisita`);

--
-- Índices para tabela `sys_tabelas`
--
ALTER TABLE `sys_tabelas`
  ADD PRIMARY KEY (`idtabela`);

--
-- Índices para tabela `sys_utilizadores`
--
ALTER TABLE `sys_utilizadores`
  ADD PRIMARY KEY (`IdUtilizador`);

--
-- Índices para tabela `sys_viaturas`
--
ALTER TABLE `sys_viaturas`
  ADD PRIMARY KEY (`matricula_viatura`);

--
-- AUTO_INCREMENT de tabelas despejadas
--

--
-- AUTO_INCREMENT de tabela `dat_clientes`
--
ALTER TABLE `dat_clientes`
  MODIFY `IdCliente` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de tabela `dat_controlo_viatura`
--
ALTER TABLE `dat_controlo_viatura`
  MODIFY `id_reg` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de tabela `dat_folhas_obra`
--
ALTER TABLE `dat_folhas_obra`
  MODIFY `IdFolhaObra` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de tabela `dat_intervencoes_folha_obra`
--
ALTER TABLE `dat_intervencoes_folha_obra`
  MODIFY `IdIntervencao` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de tabela `dat_logs`
--
ALTER TABLE `dat_logs`
  MODIFY `id_log` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de tabela `dat_propostas`
--
ALTER TABLE `dat_propostas`
  MODIFY `IdProposta` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de tabela `dat_visitas`
--
ALTER TABLE `dat_visitas`
  MODIFY `IdVisita` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de tabela `sys_tabelas`
--
ALTER TABLE `sys_tabelas`
  MODIFY `idtabela` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT de tabela `sys_utilizadores`
--
ALTER TABLE `sys_utilizadores`
  MODIFY `IdUtilizador` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=26;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

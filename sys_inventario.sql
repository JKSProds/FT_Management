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
-- Table structure for table `dat_logs_stocks`
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
LOCK TABLES `dat_logs_stocks` WRITE;
UNLOCK TABLES;
/*!40000 ALTER TABLE `dat_logs_stocks` ENABLE KEYS */;

--
-- Table structure for table `dat_produtos`
--

DROP TABLE IF EXISTS `dat_produtos`;
CREATE TABLE `dat_produtos` (
  `id_produto` int(10) unsigned NOT NULL auto_increment,
  `ref_produto` varchar(100) NOT NULL,
  `designacao_produto` varchar(100) default NULL,
  `stock_fisico` double unsigned zerofill default NULL,
  `stock_phc` double unsigned zerofill default NULL,
  `pos_stock` varchar(45) default NULL,
  `obs` varchar(1024) default NULL,
  PRIMARY KEY  (`id_produto`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `dat_produtos`
--


/*!40000 ALTER TABLE `dat_produtos` DISABLE KEYS */;
LOCK TABLES `dat_produtos` WRITE;
UNLOCK TABLES;
/*!40000 ALTER TABLE `dat_produtos` ENABLE KEYS */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;


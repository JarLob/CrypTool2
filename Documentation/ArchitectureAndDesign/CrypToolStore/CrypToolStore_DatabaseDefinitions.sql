-- phpMyAdmin SQL Dump
-- version 4.5.4.1deb2ubuntu2.1
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Erstellungszeit: 01. Nov 2018 um 14:36
-- Server-Version: 5.7.24-0ubuntu0.16.04.1
-- PHP-Version: 7.0.32-0ubuntu0.16.04.1

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Datenbank: `CrypToolStore`
--
CREATE DATABASE IF NOT EXISTS `CrypToolStore` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `CrypToolStore`;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `developers`
--

CREATE TABLE `developers` (
  `username` varchar(100) NOT NULL,
  `firstname` varchar(100) NOT NULL,
  `lastname` varchar(100) NOT NULL,
  `password` varchar(100) NOT NULL,
  `passwordsalt` varchar(100) NOT NULL,
  `passworditerations` int(11) NOT NULL,
  `email` varchar(100) NOT NULL,
  `isadmin` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `plugins`
--

CREATE TABLE `plugins` (
  `id` int(11) NOT NULL,
  `username` varchar(100) NOT NULL,
  `name` varchar(100) NOT NULL,
  `shortdescription` text NOT NULL,
  `longdescription` text NOT NULL,
  `authornames` text NOT NULL,
  `authoremails` text NOT NULL,
  `authorinstitutes` text NOT NULL,
  `icon` blob
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `resources`
--

CREATE TABLE `resources` (
  `id` int(11) NOT NULL,
  `username` varchar(100) NOT NULL,
  `name` varchar(100) NOT NULL,
  `description` text NOT NULL,
  `activeversion` int(11) NOT NULL,
  `publish` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `resourcesdata`
--

CREATE TABLE `resourcesdata` (
  `resourceid` int(11) NOT NULL,
  `version` int(11) NOT NULL,
  `data` varchar(100) DEFAULT NULL,
  `uploaddate` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `sources`
--

CREATE TABLE `sources` (
  `pluginid` int(11) NOT NULL,
  `pluginversion` int(11) NOT NULL,
  `buildversion` int(11) NOT NULL DEFAULT '0',
  `zipfilename` varchar(100) DEFAULT NULL,
  `buildstate` varchar(100) DEFAULT NULL,
  `buildlog` text NOT NULL,
  `assemblyfilename` varchar(100) DEFAULT NULL,
  `uploaddate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `builddate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `publishstate` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Indizes der exportierten Tabellen
--

--
-- Indizes für die Tabelle `developers`
--
ALTER TABLE `developers`
  ADD PRIMARY KEY (`username`);

--
-- Indizes für die Tabelle `plugins`
--
ALTER TABLE `plugins`
  ADD PRIMARY KEY (`id`),
  ADD KEY `username` (`username`);

--
-- Indizes für die Tabelle `resources`
--
ALTER TABLE `resources`
  ADD PRIMARY KEY (`id`),
  ADD KEY `username` (`username`);

--
-- Indizes für die Tabelle `resourcesdata`
--
ALTER TABLE `resourcesdata`
  ADD PRIMARY KEY (`resourceid`,`version`);

--
-- Indizes für die Tabelle `sources`
--
ALTER TABLE `sources`
  ADD PRIMARY KEY (`pluginid`,`pluginversion`),
  ADD KEY `pluginversion` (`pluginversion`);

--
-- AUTO_INCREMENT für exportierte Tabellen
--

--
-- AUTO_INCREMENT für Tabelle `plugins`
--
ALTER TABLE `plugins`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT für Tabelle `resources`
--
ALTER TABLE `resources`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Constraints der exportierten Tabellen
--

--
-- Constraints der Tabelle `plugins`
--
ALTER TABLE `plugins`
  ADD CONSTRAINT `plugins_ibfk_1` FOREIGN KEY (`username`) REFERENCES `developers` (`username`);

--
-- Constraints der Tabelle `resources`
--
ALTER TABLE `resources`
  ADD CONSTRAINT `resources_ibfk_1` FOREIGN KEY (`username`) REFERENCES `developers` (`username`);

--
-- Constraints der Tabelle `resourcesdata`
--
ALTER TABLE `resourcesdata`
  ADD CONSTRAINT `resourcesdata_ibfk_1` FOREIGN KEY (`resourceid`) REFERENCES `resources` (`id`);

--
-- Constraints der Tabelle `sources`
--
ALTER TABLE `sources`
  ADD CONSTRAINT `sources_ibfk_1` FOREIGN KEY (`pluginid`) REFERENCES `plugins` (`id`);

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
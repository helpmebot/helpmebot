-- MySQL Administrator dump 1.4
--
-- ------------------------------------------------------
-- Server version	5.1.30-community

-- ****************************************************************************
-- *   This file is part of Helpmebot.                                        *
-- *                                                                          *
-- *   Helpmebot is free software: you can redistribute it and/or modify      *
-- *   it under the terms of the GNU General Public License as published by   *
-- *   the Free Software Foundation, either version 3 of the License, or      *
-- *   (at your option) any later version.                                    *
-- *                                                                          *
-- *   Helpmebot is distributed in the hope that it will be useful,           *
-- *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
-- *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
-- *   GNU General Public License for more details.                           *
-- *                                                                          *
-- *   You should have received a copy of the GNU General Public License      *
-- *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
-- ****************************************************************************

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;


--
-- Create schema u_stwalkerster_hmb6
--

CREATE DATABASE IF NOT EXISTS u_stwalkerster_hmb6;
USE u_stwalkerster_hmb6;

--
-- Definition of table `configuration`
--

DROP TABLE IF EXISTS `configuration`;
CREATE TABLE `configuration` (
  `configuration_id` int(11) NOT NULL AUTO_INCREMENT,
  `configuration_description` varchar(1024) DEFAULT NULL,
  `configuration_name` varchar(128) NOT NULL,
  `configuration_value` varchar(4096) NOT NULL,
  `configuration_lastchangedby` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`configuration_id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=latin1;

--
-- Dumping data for table `configuration`
--

/*!40000 ALTER TABLE `configuration` DISABLE KEYS */;
INSERT INTO `configuration` (`configuration_id`,`configuration_description`,`configuration_name`,`configuration_value`,`configuration_lastchangedby`) VALUES 
 (1,'IRC Server','ircServerHost','wolfe.freenode.net',1),
 (2,'IRC Server port','ircServerPort','6667',1),
 (3,'IRC Nickname','ircNickname','Helpmebot6',1),
 (4,'IRC Real name','ircRealname','Helpmebot 6',1),
 (5,'IRC Username','ircUsername','helpmebot',1),
 (6,'IRC Password','ircPassword','PASSWORD',1),
 (7,'Recieve Wallops from IRC','ircRecieveWallops','false',1),
 (8,'Be invisible on WHO and WHOIS','ircInvisible','true',1),
 (9,'Debugging IRC Channel','channelDebug','##stwalkerster',1),
 (10,'Nubio API URI','faqApiUri','http://stable.toolserver.org/nubio/api.php',1),
 (11,'Command Trigger','commandTrigger','!',1),
 (12,'Main IRC Channel','channelMain','#wikipedia-en-help',1);
/*!40000 ALTER TABLE `configuration` ENABLE KEYS */;


--
-- Definition of table `message`
--

DROP TABLE IF EXISTS `message`;
CREATE TABLE `message` (
  `message_id` int(11) NOT NULL AUTO_INCREMENT,
  `message_name` varchar(45) NOT NULL,
  `message_description` varchar(1024) NOT NULL,
  `message_text` varchar(512) NOT NULL,
  `message_lastchangedby` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`message_id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=latin1;

--
-- Dumping data for table `message`
--

/*!40000 ALTER TABLE `message` DISABLE KEYS */;
INSERT INTO `message` (`message_id`,`message_name`,`message_description`,`message_text`,`message_lastchangedby`) VALUES 
 (1,'cmdSayHi1','Said when the bot says hi!','Hi there {0}!',1),
 (2,'cmdSayHi1','Said when the bot says hi!','Hey {0}',1),
 (3,'cmdSayHi1','Said when the bot says hi!','Hello {0}',1),
 (4,'fetchFaqTextNotFound','When an FAQ fetch goes awry...','Could not find FAQ entry.',1);
/*!40000 ALTER TABLE `message` ENABLE KEYS */;


--
-- Definition of table `user`
--

DROP TABLE IF EXISTS `user`;
CREATE TABLE `user` (
  `user_id` int(11) NOT NULL AUTO_INCREMENT,
  `user_nickname` varchar(16) NOT NULL DEFAULT '%',
  `user_username` varchar(12) NOT NULL DEFAULT '%',
  `user_hostname` varchar(63) NOT NULL DEFAULT '%',
  `user_accesslevel` enum('Superuser','Ignored','Semi-ignored','Advanced','Normal') NOT NULL DEFAULT 'Normal',
  PRIMARY KEY (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=latin1;

--
-- Dumping data for table `user`
--

/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` (`user_id`,`user_nickname`,`user_username`,`user_hostname`,`user_accesslevel`) VALUES 
 (1,'Default','BOT','localhost','Superuser'),
 (2,'stwalker%','%','127.0.0.1','Superuser');
/*!40000 ALTER TABLE `user` ENABLE KEYS */;




/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;

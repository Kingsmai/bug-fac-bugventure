/*
 Navicat Premium Data Transfer

 Source Server         : localhost
 Source Server Type    : MySQL
 Source Server Version : 50731
 Source Host           : localhost:3306
 Source Schema         : bugventure

 Target Server Type    : MySQL
 Target Server Version : 50731
 File Encoding         : 65001

 Date: 13/02/2021 14:01:17
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for inventory
-- ----------------------------
DROP TABLE IF EXISTS `inventory`;
CREATE TABLE `inventory`  (
  `InventoryItemID` int(11) NOT NULL,
  `Quantity` int(11) NOT NULL
) ENGINE = InnoDB CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for quest
-- ----------------------------
DROP TABLE IF EXISTS `quest`;
CREATE TABLE `quest`  (
  `QuestID` int(11) NOT NULL,
  `IsCompleted` tinyint(1) NOT NULL
) ENGINE = InnoDB CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for saved_game
-- ----------------------------
DROP TABLE IF EXISTS `saved_game`;
CREATE TABLE `saved_game`  (
  `CurrentHitPoints` int(11) NOT NULL,
  `MaximumHitPoints` int(11) NOT NULL,
  `Gold` int(11) NOT NULL,
  `ExperiencePoints` int(11) NOT NULL,
  `CurrentLocationID` int(11) NOT NULL
) ENGINE = InnoDB CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;

# Bug-Venture

纯UI学习向游戏，学习资源：[(scottlilly.com)](https://scottlilly.com/)

![image-20210213140259186](C:\SourceCode\bug-fac-bugventure\gameScreenshot.png)

## 2021-02-13 数据库更新

MySQL 版本：5.5+

参数：

```c#
string server = "localhost";
string uid = "root";
string pwd = "123456";
string database = "bugventure";
```



直接执行Sqls文件夹里的bugventure.sql（代码如下）便可自动创建数据库

```mysql
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
```


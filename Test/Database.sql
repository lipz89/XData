

IF EXISTS (SELECT * FROM sys.objects WHERE name='Dictionary2' AND type='U')
	DROP TABLE [dbo].[Dictionary2]
GO


CREATE TABLE [dbo].[Dictionary2](
	[ID] [UNIQUEIDENTIFIER] NOT NULL PRIMARY KEY,
	[Name] [VARCHAR](64) NULL,
	[Code] [VARCHAR](64) NULL,
	[IsSys] [BIT] NOT NULL,
	[Memo] [VARCHAR](256) NULL,
	[Status] [TINYINT] NULL
) 

GO


IF EXISTS (SELECT * FROM sys.objects WHERE name='DictionaryDetail2' AND type='U')
	DROP TABLE [dbo].[DictionaryDetail2]
GO

CREATE TABLE [dbo].[DictionaryDetail2](
	[ID] [uniqueidentifier] NOT NULL PRIMARY KEY,
	[DictionaryID] [uniqueidentifier] NOT NULL,
	[Name] [varchar](64) NULL,
	[Code] [varchar](64) NULL,
	[Memo] [varchar](256) NULL,
	[ParentID] [uniqueidentifier] NULL,
	[IndexID] [int] NOT NULL
) ON [PRIMARY]

GO


IF EXISTS (SELECT * FROM sys.objects WHERE name='Menu' AND type='U')
	DROP TABLE [dbo].[Menu]
GO


CREATE TABLE [dbo].[Menu](
	[ID] [uniqueidentifier] NOT NULL PRIMARY KEY,
	[Name] [varchar](64) NULL,
	[Code] [varchar](64) NULL,
	[Controller] [varchar](64) NULL,
	[Action] [varchar](64) NULL,
	[MenuLevel] [int] NOT NULL,
	[IndexID] [int] NOT NULL,
	[Memo] [varchar](256) NULL,
	[IsDeleted] [tinyint] NOT NULL
)

GO



IF EXISTS (SELECT * FROM sys.objects WHERE name='Test' AND type='U')
	DROP TABLE [dbo].Test
GO


CREATE TABLE Test(
	ID int ,
	[Index] int,
	Name varchar(20)
)

GO




IF EXISTS (SELECT * FROM sys.objects WHERE name='Parent' AND type='U')
	DROP TABLE [dbo].Parent
GO


CREATE TABLE [dbo].Parent(
	[ID] int NOT NULL PRIMARY KEY,
	[Name] [varchar](64) NULL,
	[Code] [varchar](64) NULL
)

GO


IF EXISTS (SELECT * FROM sys.objects WHERE name='Child' AND type='U')
	DROP TABLE [dbo].Child
GO


CREATE TABLE [dbo].Child(
	[ID] int NOT NULL PRIMARY KEY,
	ParentID INT NULL,
	[Name] [varchar](64) NULL,
	[Code] [varchar](64) NULL
)

GO



INSERT INTO dbo.Parent ( ID, Name, Code )
SELECT 1,'类型','Type'
UNION SELECT 2,'性别','Sex'
UNION SELECT 3,'目录','Catalog'

GO

INSERT INTO dbo.Child        ( ID, ParentID, Name, Code )
SELECT 1,1,'材料','cailiao'
UNION SELECT 2,1,'消耗品','xiaohaopin'
UNION SELECT 3,1,'装备','zhuangbei'
UNION SELECT 4,1,'器械','qixie'
UNION SELECT 5,1,'秘籍','miji'

GO

INSERT INTO dbo.Child        ( ID, ParentID, Name, Code )
SELECT 6,2,'男','nan'
UNION SELECT 7,2,'女','nv'

GO


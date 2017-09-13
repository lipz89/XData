

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


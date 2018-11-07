

IF EXISTS (SELECT * FROM sys.objects WHERE name='Users' AND type='U')
	DROP TABLE [dbo].[Users]
GO


CREATE TABLE [dbo].[Users](
	[ID] [UNIQUEIDENTIFIER] NOT NULL PRIMARY KEY,
	[UserName] [VARCHAR](64) NULL,
	[Password] [VARCHAR](256) NULL,
	[RealName] [VARCHAR](64) NULL,
	[Code] [VARCHAR](64) NULL,
	[Phone] [VARCHAR](64) NULL,
	[Email] [VARCHAR](64) NULL,
	[IsSys] [BIT] NOT NULL,
	[Memo] [VARCHAR](256) NULL,
	[Status] [TINYINT] NULL
) ON [PRIMARY]

GO


IF EXISTS (SELECT * FROM sys.objects WHERE name='Roles' AND type='U')
	DROP TABLE [dbo].[Roles]
GO

CREATE TABLE [dbo].[Roles](
	[ID] [uniqueidentifier] NOT NULL PRIMARY KEY,
	[Name] [varchar](64) NULL,
	[Code] [varchar](64) NULL,
	[Memo] [varchar](256) NULL
) ON [PRIMARY]

GO


IF EXISTS (SELECT * FROM sys.objects WHERE name='UserRoles' AND type='U')
	DROP TABLE [dbo].[UserRoles]
GO

CREATE TABLE [dbo].[UserRoles](
	[ID] [uniqueidentifier] NOT NULL PRIMARY KEY,
	[UserID] [uniqueidentifier] NOT NULL,
	[RoleID] [uniqueidentifier] NOT NULL
) ON [PRIMARY]

GO


IF EXISTS (SELECT * FROM sys.objects WHERE name='Menus' AND type='U')
	DROP TABLE [dbo].[Menus]
GO


CREATE TABLE [dbo].[Menus](
	[ID] [uniqueidentifier] NOT NULL PRIMARY KEY,
	[Name] [varchar](64) NULL,
	[Code] [varchar](64) NULL,
	[Url] [varchar](64) NULL,
	[MenuLevel] [int] NOT NULL,
	[ParentID] [uniqueidentifier] NULL,
	[IndexID] [int] NOT NULL,
	[Memo] [varchar](256) NULL
)ON [PRIMARY]

GO


IF EXISTS (SELECT * FROM sys.objects WHERE name='Actions' AND type='U')
	DROP TABLE [dbo].[Actions]
GO


CREATE TABLE [dbo].[Actions](
	[ID] [uniqueidentifier] NOT NULL PRIMARY KEY,
	[Name] [varchar](64) NULL,
	[Code] [varchar](64) NULL,
	[MenuID] [uniqueidentifier] NULL,
	[IndexID] [int] NOT NULL,
	[Memo] [varchar](256) NULL
)ON [PRIMARY]

GO



IF EXISTS (SELECT * FROM sys.objects WHERE name='RoleActions' AND type='U')
	DROP TABLE [dbo].[RoleActions]
GO

CREATE TABLE [dbo].[RoleActions](
	[ID] [uniqueidentifier] NOT NULL PRIMARY KEY,
	[RoleID] [uniqueidentifier] NOT NULL,
	[ActionID] [uniqueidentifier] NOT NULL
) ON [PRIMARY]

GO


IF EXISTS (SELECT * FROM sys.objects WHERE name='SomeValues' AND type='U')
	DROP TABLE [dbo].[SomeValues]
GO




CREATE TABLE [dbo].[SomeValues](
	[ID] [int] NOT NULL,
	[ValueBit] [bit] NULL,
	[ValueInt] [int] NULL,
	[ValueInt2] [int] NULL,
	[ValueFloat] [float] NULL,
	[ValueDecimal] [decimal](18, 4) NULL,
	[ValueDate] [date] NULL,
	[ValueDatetime] [datetime] NULL,
	[ValueDatetime2] [datetime2](7) NULL,
	[ValueDatetimeOffset] [DATETIMEOFFSET] NULL,
	[ValueReal] [real] NULL,
	[ValueNumeric] [numeric](18, 4) NULL,
	ValueBigInt BIGINT NULL,
	ValueTinyint TINYINT NULL,
	ValueVarchar VARCHAR(50) NULL,
	ValueNVarchar NVARCHAR(50) NULL,
	ValueText TEXT NULL,
	ValueChar CHAR(1) NULL,
	ValueNChar NCHAR(1) NULL
) ON [PRIMARY]

GO







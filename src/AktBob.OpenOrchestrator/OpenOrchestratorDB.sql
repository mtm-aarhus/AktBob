USE [OpenOrchestrator]
GO
CREATE TABLE [dbo].[Queues](
	[id] [uniqueidentifier] NOT NULL,
	[queue_name] [varchar](100) NOT NULL,
	[status] [varchar](11) NOT NULL,
	[data] [varchar](2000) NULL,
	[reference] [varchar](100) NULL,
	[created_date] [datetime] NOT NULL,
	[start_date] [datetime] NULL,
	[end_date] [datetime] NULL,
	[message] [varchar](max) NULL,
	[created_by] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
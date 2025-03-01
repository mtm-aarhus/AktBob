CREATE TABLE [dbo].[Tickets] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [DeskproId]  INT           NOT NULL,
    [CaseNumber] NVARCHAR (50) NULL,
    [CaseUrl] NVARCHAR(1024) NULL,
    [SharepointFolderName] NVARCHAR(2048) NULL,    
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


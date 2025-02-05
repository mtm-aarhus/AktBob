CREATE TABLE [dbo].[Cases] (
    [Id]             INT              IDENTITY (1, 1) NOT NULL,
    [TicketId]       INT              NOT NULL,
    [PodioItemId]    BIGINT           NOT NULL,
    [CaseNumber]     NVARCHAR (50)    NOT NULL,
    [FilArkivCaseId] UNIQUEIDENTIFIER NULL,
    [SharepointFolderName] NVARCHAR(2048) NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Cases_Tickets] FOREIGN KEY ([TicketId]) REFERENCES [dbo].[Tickets] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Cases_TicketId]
    ON [dbo].[Cases]([TicketId] ASC);


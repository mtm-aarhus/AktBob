CREATE TABLE [dbo].[Messages] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [TicketId]     INT NOT NULL,
    [DeskproId]    INT NOT NULL,
    [GODocumentId] INT NULL,
    [Deleted] BIT NOT NULL DEFAULT 0, 
    [QueuedForJournalizationAt] DATETIME2 NULL, 
    [Hash] NCHAR(64) NULL, 
    [MessageNumber] INT NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Messages_Ticket] FOREIGN KEY ([TicketId]) REFERENCES [dbo].[Tickets] ([Id])
);


GO

CREATE INDEX [IX_Messages_DeskproId] ON [dbo].[Messages] (DeskproId)

CREATE TABLE [dbo].[OS2FormsSubmission]
(
	[Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[DeskproTicketId] INT NOT NULL,
	[SubmissionId] UNIQUEIDENTIFIER NOT NULL,
	[DescriptionFieldValue] NVARCHAR(MAX) NULL
)

GO

CREATE INDEX [IX_OS2FormsSubmission_DeskproTicketId] ON [dbo].[OS2FormsSubmission] ([DeskproTicketId])

GO

CREATE INDEX [IX_OS2FormsSubmission_SubmissionId] ON [dbo].[OS2FormsSubmission] ([SubmissionId])

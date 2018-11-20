CREATE TABLE [dbo].[Items] (
    [RecordId]   INT           IDENTITY (1, 1) NOT NULL,
    [ItemCode]   NVARCHAR (50) NULL,
    [ItemName]   NVARCHAR (50) NULL,
    [ItemPrice]  INT           NULL,
    [ItemImage]  NTEXT         NULL,
    [ItemCount]  INT           NULL,
    [CreatedBy]  NVARCHAR (50) NULL,
    [ModifiedBy] NVARCHAR (50) NULL,
    [CreatedOn]  DATETIME      NULL,
    [ModifiedOn] DATETIME      NULL,
    PRIMARY KEY CLUSTERED ([RecordId] ASC)
);


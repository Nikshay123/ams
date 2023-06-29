/****** Object:  Trigger [dbo].[MaintainInventoryHistory]    Script Date: 2/17/2023 10:04:22 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TRIGGER [dbo].[Samples] 
   ON  [dbo].[Samples]
   AFTER INSERT,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Trigger body - NO-OP right now

END
GO

ALTER TABLE [dbo].[Samples] ENABLE TRIGGER [Sample]
GO



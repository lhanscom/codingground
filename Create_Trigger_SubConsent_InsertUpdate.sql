USE [LW_PQRS]
GO

/****** Object:  Trigger [dbo].[trg_SubConsent_InsertUpdate]    Script Date: 1/6/2015 12:53:55 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








CREATE TRIGGER [dbo].[trg_SubConsent_InsertUpdate] ON [dbo].[SubConsent]
FOR INSERT, UPDATE
AS  
BEGIN

	-- Testing this section to see if it properly deletes existing entires for the same practice with GPRO
	-- If the wrong queue records are being deleted then this might have to be reomoved.
	DELETE FROM MingleAnalyticsWeb.[dbo].[ContentUpdateQueue] 
	WHERE RecordIdentifier IN (SELECT ID FROM INSERTED WHERE ProviderID is null)

    INSERT INTO MingleAnalyticsWeb.[dbo].[ContentUpdateQueue]
         (
		 [TableAltered],
		 [ActionPerformed],
		 [RecordIdentifier],
		 [SubmissionMethod],
		 [ClientId],
		 [PracticeId],
		 [ProviderId],
		 [SubYear],
		 [GProRegID],
		 [Submitting],
		 [HasConsent],
		 [ConsentBy],
		 [Submitted],
		 [LastGeneratedDate],
		 [MedicareBatchId],
		 [DateTimeAddedToQueue]
		 ) 
    SELECT 
		'SubConsent', 
		'UPDATE', 
		inserted.[ID], 
		inserted.[Method],
		inserted.[ClientId], 
		inserted.[PracticeID], 
		inserted.[ProviderId], 
		inserted.[SubYear],
		inserted.[GProRegID],
		inserted.[Submit], 
		inserted.[HasConsent], 
		inserted.[ConsentBy], 
		inserted.[Submitted], 
		inserted.[LastGeneratedDate], 
		inserted.[MedicareBatchId], 
		GETDATE()
    FROM inserted
	/*----------------------------------Check for potential need to update step 7 status if consent changed-----------------------------------------*/

	--TEST DATA

--select * from [dbo].[SubConsent] where  id=10820   --GPRO example
--select * from dbo.sel_measure where clientid=4573
--select * from Analytics.cal.Step_Status where clientid=4573 and practiceid=4561 and subyear=2014
--update [dbo].[SubConsent] set hasconsent='Y' where id=10820

--select * from [dbo].[SubConsent] where clientid=1443  id=1443   --IM example
--select * from dbo.sel_measure where clientid=1443
--select * from Analytics.cal.Step_Status where clientid=1443 and practiceid=1684 and subyear=2014
--update [dbo].[SubConsent] set hasconsent='Y' where id=10824


	DECLARE	@ClientID int
	DECLARE @PracticeID int
	DECLARE @SubYear nchar(4)
	DECLARE @PassedStep nchar(1)
	DECLARE @Step8_Value int
	DECLARE cur CURSOR LOCAL READ_ONLY FAST_FORWARD FOR
    SELECT
        i.clientid,i.practiceid,i.subyear
    FROM
        INSERTED I
        JOIN
        DELETED D  ON D.ID = I.ID AND D.HasConsent <> I.HasConsent											 ---if doing an update and HasConsent value has changed
	OPEN cur

    FETCH NEXT FROM cur INTO @ClientID,@PracticeID, @SubYear
	WHILE @@FETCH_STATUS = 0						 
	BEGIN					
		EXEC dbo.sp_StepStatus_Step8_Check @ClientID, @PracticeID, @SubYear, @PassedStep OUTPUT;		--call proc to check permission step status

		IF @PassedStep = 'Y'
			SET @Step8_Value = 1;
		ELSE
			SET @Step8_Value = 0;

		DECLARE @step8_curValue int
		select @step8_curValue = step8
		from [LW_PQRS].[cal].[StepStatus] 
		WHERE subyear = Convert(int, @SubYear) AND ClientID = @ClientID AND PracticeID = @PracticeID;
		/* 2014/12/20 BM Should be updating step 8 status not the override table*/
		/* 2014/12/24 BM Only update, calc step status if different*/
		if @step8_curValue!=@Step8_Value
		BEGIN
			UPDATE [LW_PQRS].[cal].[StepStatus] 
			SET
				Step8 = @Step8_Value,
				LastUpdated = GETDATE()
			WHERE subyear = Convert(int, @SubYear) AND ClientID = @ClientID AND PracticeID = @PracticeID;

			EXEC cal.CalculateStepStatus @ClientID, @PracticeID, @SubYear;		--call proc to recalc StepStatus
		
		END
/*		
		-- If a record for the Client/Practice/SubYear exists update it, otherwise insert new record
		UPDATE LW_PQRS.cal.StepStatusOverride SET StepValue = @Step8_Value WHERE StepNumber = 8 AND subyear = @SubYear AND ClientID = @ClientID AND PracticeID = @PracticeID;
		IF @@ROWCOUNT=0
			INSERT INTO LW_PQRS.cal.StepStatusOverride (ClientId, PracticeId, SubYear, StepNumber, StepValue) VALUES (@ClientID, @PracticeId, @SubYear, 8, @Step8_Value)
*/
		FETCH NEXT FROM cur INTO @ClientID,@PracticeID, @SubYear
	END
	CLOSE cur
    DEALLOCATE cur
	
		
		
END
GO



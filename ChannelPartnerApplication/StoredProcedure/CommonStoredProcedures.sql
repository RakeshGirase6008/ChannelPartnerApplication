CREATE PROCEDURE [dbo].[Classbook_CalculateCommision]
	@OrderId INT,
	@ChannelPartnerId INT,
	@OurAmount DECIMAL,
	@From VARCHAR(50)
AS
BEGIN

	DECLARE @ResidualPercentageAchieve DECIMAL=4
	DECLARE @ResidualPercentageNotAchieve DECIMAL(10,2)=2.5
	DECLARE @DefaultPercentage INT=10
	DECLARE @LevelId INT=0

	DECLARE @LevelDifferntPercentage INT=0
	DECLARE @TotalSpendLevelDifferntPercentage INT=0
	DECLARE @PartnerAmount DECIMAL=0
	DECLARE @ParentId INT=-1
	DECLARE @CurrentCount INT
	DECLARE @LoopCount INT=0
	DECLARE @GenerationCount INT=1
	DECLARE @PreviousChannelPartnerPercentage INT=0
	DECLARE @ResidualLevelId INT=0
	
	DECLARE @CalculativePercentage INT=0
	DECLARE @StartResidual BIT=0
	DECLARE @TargetToReach INT=0

	DECLARE @StartResidualChannerlPartnerId INT=0

	WHILE (@ParentId!=0)
	BEGIN
		SELECT
				@ParentId=CPM.ParentId ,
				@LevelId=CPM.LevelId,
				@LevelDifferntPercentage=PC.[Percentage],
				@CurrentCount=CurrentCount 
			FROM ChannelPartnerMapping CPM
			INNER JOIN PromotionalCycle PC ON CPM.LevelId =PC.LevelId
			WHERE ChannelPartnerId=@ChannelPartnerId
		
		SELECT @TargetToReach=ISNULL(AchievementCount,-1) FROM PromotionalCycle WHERE LevelId=(@LevelId+1)
		
		--Start Level Difference
		IF @TotalSpendLevelDifferntPercentage!=25
		BEGIN

			IF @LoopCount=0
			BEGIN
				
				SET @PartnerAmount=(@OurAmount*@LevelDifferntPercentage)/100;
				INSERT INTO CommissionHistory VALUES(@OrderId,@From,@ChannelPartnerId,@LevelId,@PartnerAmount,'Direct','LevelDiff')
				SET @TotalSpendLevelDifferntPercentage=@TotalSpendLevelDifferntPercentage+@LevelDifferntPercentage;
			END
			ELSE
			BEGIN
				IF @LevelDifferntPercentage > @PreviousChannelPartnerPercentage
				BEGIN

					SET @CalculativePercentage=@LevelDifferntPercentage-@PreviousChannelPartnerPercentage;
					SET @PartnerAmount=(@OurAmount*@CalculativePercentage)/100;
					INSERT INTO CommissionHistory VALUES(@OrderId,@From,@ChannelPartnerId,@LevelId,@PartnerAmount,'InDirect','LevelDiff')
					SET @TotalSpendLevelDifferntPercentage=@TotalSpendLevelDifferntPercentage+@CalculativePercentage;

				END
			END
			SET @PreviousChannelPartnerPercentage=@LevelDifferntPercentage	
			SET @LoopCount=@LoopCount+1
		END
		--End Level Difference


		--Start Residual
		IF @TotalSpendLevelDifferntPercentage=25 AND @GenerationCount < 4
		BEGIN
			IF @StartResidual=1
			BEGIN 
				IF @LevelId >= @ResidualLevelId+@GenerationCount
				BEGIN
					SET @CalculativePercentage=@ResidualPercentageAchieve
				END
				ELSE
				BEGIN
					SET @CalculativePercentage=@ResidualPercentageNotAchieve
				END
				SET @PartnerAmount=(@OurAmount*@CalculativePercentage)/100;
				INSERT INTO CommissionHistory VALUES(@OrderId,@From,@ChannelPartnerId,@LevelId,@PartnerAmount,'InDirect','Residual')
				SET @GenerationCount=@GenerationCount+1
			END
			ELSE
			BEGIN
				SET @StartResidual=1
				SET @ResidualLevelId=@LevelId
			END
		END
		--End Residual


		-- Start Increase Count & Level Up
		UPDATE ChannelPartnerMapping
		SET CurrentCount=CurrentCount+1,
		TotalCount=TotalCount+1
		WHERE ChannelPartnerId=@ChannelPartnerId

		IF @CurrentCount+1=@TargetToReach
		BEGIN
			
			UPDATE ChannelPartnerMapping
			SET LevelId=@LevelId+1,CurrentCount=0
			WHERE ChannelPartnerId=@ChannelPartnerId

			INSERT INTO PromotionHistory VALUES(@ChannelPartnerId,@LevelId+1,'Self',GETDATE())

		END
		-- End

		SET @ChannelPartnerId=@ParentId
	END
	SELECT * FROM ChannelPartnerMapping
	SELECT * FROM CommissionHistory ORDER by Id Desc
	SELECT * FROM PromotionHistory ORDER by Id Desc
END


GO
CREATE PROCEDURE [dbo].[Classbook_CalculateCommision_MonthEnd]
AS
BEGIN
	
	-- Drop the ##Temp Tables
		DECLARE @sql nvarchar(max)        
		SELECT	@sql = isnull(@sql+';', '') + 'drop table ' + quotename(name)        
		FROM	tempdb..sysobjects
		WHERE	[name] like '##%'        
		EXEC	(@sql)

	--Start the Bonus Amount
	SELECT ChannelPartnerId,CONVERT(DECIMAL(10,2),SUM(Amount) )as DirectAmount,
	CASE
		WHEN SUM(Amount) BETWEEN 0 AND  20000 THEN 2
		WHEN SUM(Amount) BETWEEN 20001 AND  40000 THEN 4
		WHEN SUM(Amount) BETWEEN 40001 AND  60000 THEN 6
		WHEN SUM(Amount) BETWEEN 60001 AND  80000 THEN 8
		WHEN SUM(Amount) BETWEEN 80001 AND  40000 THEN 10
		ELSE 10
	END AS [Percentage]
	INTO ##Temp
	FROM CommissionHistory CH
	WHERE [Status]='Direct'
	AND GETDATE() BETWEEN dateadd(d,-(day(getdate()-1)),getdate()) 
	AND dateadd(d,-(day(dateadd(m,1,getdate()))),dateadd(m,1,getdate()))
	GROUP BY ChannelPartnerId

	INSERT INTO BonusHistory
	SELECT ChannelPartnerId,DirectAmount,[Percentage],
	CONVERT(DECIMAL(10,2),(DirectAmount*[Percentage]/100)) as BonusAmount,
	GETDATE() FROM ##Temp
	--End the Bonus Amount

	--Level the Royalty
	DECLARE @ChannelPartnerId INT;
	DECLARE @RunningTotal BIGINT = 0;
	DECLARE @DirectTotal INT = 0;
	DECLARE @TotalCount INT = 0;

 
	DECLARE CUR_TEST CURSOR FAST_FORWARD FOR
		SELECT ChannelPartnerId
		FROM  ChannelPartnerMapping
		WHERE LevelId=6
		ORDER BY ChannelPartnerId desc;
 
		OPEN CUR_TEST
		FETCH NEXT FROM CUR_TEST INTO @ChannelPartnerId
 
		WHILE @@FETCH_STATUS = 0
		BEGIN

			--Delete ##Temp Tables
			
			IF EXISTS(SELECT	*
			FROM	tempdb..sysobjects
			WHERE	[name] like '##TempStage1' )
			BEGIN
				DROP TABLE ##TempStage1
			END

			IF EXISTS(SELECT	*
			FROM	tempdb..sysobjects
			WHERE	[name] like '##TempStage2' )
			BEGIN
				DROP TABLE ##TempStage2
			END


			DECLARE @AchieveStage1 BIT=0
			DECLARE @MappingId INT=0


			SELECT @DirectTotal=ISNULL(COUNT(*),0)
			FROM CommissionHistory CH
			WHERE [Status]='Direct'
			AND GETDATE() BETWEEN dateadd(d,-(day(getdate()-1)),getdate()) 
			AND dateadd(d,-(day(dateadd(m,1,getdate()))),dateadd(m,1,getdate()))
			GROUP BY ChannelPartnerId


			SELECT @ChannelPartnerId as ChannelPartnerId,ChannelPartnerId as SupportiveCPId,
			CASE
				WHEN TotalCount > 72 THEN 72
				ELSE TotalCount
			END AS [TotalCount]
			INTO ##TempStage1
			FROM ChannelPartnerMapping
			WHERE ParentId=@ChannelPartnerId


			SELECT @ChannelPartnerId as ChannelPartnerId,ChannelPartnerId as SupportiveCPId,
			CASE
				WHEN TotalCount > 144 THEN 144
				ELSE TotalCount
			END AS [TotalCount]
			INTO ##TempStage2
			FROM ChannelPartnerMapping
			WHERE ParentId=@ChannelPartnerId


			IF NOT EXISTS(SELECT * FROM RoyaltyMapping WHERE ChannelPartnerId=@ChannelPartnerId AND [Status]='Stage-1')
			BEGIN

				SELECT @RunningTotal=ISNULL(SUM(TotalCount),0) FROM ##TempStage1
				WHERE ChannelPartnerId=@ChannelPartnerId
				GROUP BY ChannelPartnerId

				SET @TotalCount=@RunningTotal+@DirectTotal

				IF @TotalCount >= 128
				BEGIN
					SET @AchieveStage1=1
					INSERT INTO RoyaltyMapping VALUES(@ChannelPartnerId,'Stage-1',GETDATE(),0)

					SET @MappingId=SCOPE_IDENTITY()
					
					INSERT INTO RoyaltyAchievementSupport
					SELECT @MappingId,SupportiveCPId,TotalCount
					FROM ##TempStage1

				END
			END
			SET @TotalCount=0
			SET @RunningTotal=0

			IF EXISTS(SELECT * FROM RoyaltyMapping WHERE ChannelPartnerId=@ChannelPartnerId AND [Status]='Stage-1') AND
				NOT EXISTS(SELECT * FROM RoyaltyMapping WHERE ChannelPartnerId=@ChannelPartnerId AND [Status]='Stage-2')
			BEGIN

				SELECT @RunningTotal=ISNULL(SUM(TotalCount),0) FROM ##TempStage2
				WHERE ChannelPartnerId=@ChannelPartnerId
				GROUP BY ChannelPartnerId

				SET @TotalCount=@RunningTotal+@DirectTotal

				IF @AchieveStage1=1
				BEGIN
					IF @TotalCount >= 384
					BEGIN
						INSERT INTO RoyaltyMapping VALUES(@ChannelPartnerId,'Stage-2',GETDATE(),0)

						SET @MappingId=SCOPE_IDENTITY()
					
						INSERT INTO RoyaltyAchievementSupport
						SELECT @MappingId,SupportiveCPId,TotalCount
						FROM ##TempStage2
					END
				END
				ELSE
				BEGIN
					IF @TotalCount >= 256
					BEGIN
						INSERT INTO RoyaltyMapping VALUES(@ChannelPartnerId,'Stage-2',GETDATE(),0)

						SET @MappingId=SCOPE_IDENTITY()

						INSERT INTO RoyaltyAchievementSupport
						SELECT @MappingId,SupportiveCPId,TotalCount
						FROM ##TempStage2
					END
				END
			END
		FETCH NEXT FROM CUR_TEST INTO @ChannelPartnerId
	END
	CLOSE CUR_TEST
	DEALLOCATE CUR_TEST
	--End the Royalty
END

GO
CREATE PROCEDURE [dbo].[ChannelPartner_GetLevelChart]
AS
BEGIN
	SELECT 'LevelDiff' as [Type],'10%' as CurrentLevel,'15%' as PromotionLevel,'2' as [Target],'10%' as IncomePercentage
	UNION
	SELECT 'LevelDiff' as [Type],'15%' as CurrentLevel,'20%' as PromotionLevel,'4' as [Target],'15% OR 5%' as IncomePercentage
	UNION
	SELECT 'LevelDiff' as [Type],'20%' as CurrentLevel,'25%' as PromotionLevel,'8' as [Target],'20% OR 5%' as IncomePercentage
	UNION
	SELECT 'Residual' as [Type],'25%' as CurrentLevel,'25+%' as PromotionLevel,'16' as [Target],'25% OR 5%' as IncomePercentage
	UNION
	SELECT 'Residual' as [Type],'25+%' as CurrentLevel,'25++%' as PromotionLevel,'32' as [Target],'4% OR 2.5%' as IncomePercentage
	UNION
	SELECT 'Residual' as [Type],'25++%' as CurrentLevel,'25+++%' as PromotionLevel,'64' as [Target],'4% OR 2.5%' as IncomePercentage
	UNION
	SELECT 'Royalty' as [Type],'25+++%' as CurrentLevel,'Stage 1' as PromotionLevel,'128' as [Target],'4% OR 2.5%' as IncomePercentage
	UNION
	SELECT 'Royalty' as [Type],'Stage 1' as CurrentLevel,'Stage 2' as PromotionLevel,'256' as [Target],'2.5%' as IncomePercentage
END

GO
CREATE PROCEDURE [dbo].[ChannelPartner_GetChannelPartnersList]
	@ChannelPartnerId INT=0,
	@Searchkeyword VARCHAR(100)='',
	@LevelId INT=0,
	@GenerationId INT=0
AS
BEGIN

	--// Create Temp Table
	CREATE TABLE #TempChannelPartner
	(
		CpId INT NOT NULL,
		Generation INT NOT NULL,
		LevelId INT NOT NULL,
		ParentId INT NOT NULL
	)
	
	INSERT INTO #TempChannelPartner
	SELECT ChannelPartnerId,1,CPM.LevelId,CPM.ParentId FROM ChannelPartnerMapping CPM
	WHERE ParentId=@ChannelPartnerId

	INSERT INTO #TempChannelPartner
	SELECT ChannelPartnerId,2,CPM.LevelId,CPM.ParentId FROM ChannelPartnerMapping CPM
	INNER JOIN #TempChannelPartner Gtwo ON Gtwo.CpId=CPM.ParentId

	INSERT INTO #TempChannelPartner
	SELECT ChannelPartnerId,3,CPM.LevelId,CPM.ParentId FROM ChannelPartnerMapping CPM
	INNER JOIN #TempChannelPartner GThree ON GThree.CpId=CPM.ParentId
	WHERE Generation=2

	INSERT INTO #TempChannelPartner
	SELECT ChannelPartnerId,4,CPM.LevelId,CPM.ParentId FROM ChannelPartnerMapping CPM
	INNER JOIN #TempChannelPartner GFourth ON GFourth.CpId=CPM.ParentId
	WHERE Generation=3

	INSERT INTO #TempChannelPartner
	SELECT ChannelPartnerId,5,CPM.LevelId,CPM.ParentId FROM ChannelPartnerMapping CPM
	INNER JOIN #TempChannelPartner GFive ON GFive.CpId=CPM.ParentId
	WHERE Generation=4

	INSERT INTO #TempChannelPartner
	SELECT ChannelPartnerId,6,CPM.LevelId,CPM.ParentId FROM ChannelPartnerMapping CPM
	INNER JOIN #TempChannelPartner GSix ON GSix.CpId=CPM.ParentId
	WHERE Generation=5


	DECLARE @SqlQuery VARCHAR(MAX)
	DECLARE @WhereCondition VARCHAR(MAX)=''
	SET @SqlQuery='
		SELECT CP.Id,(ISNULL(CP.Firstname,'''') + '' '' + ISNULL(CP.lastName,'''')) as [FullName],
		C.[Name] as CityName,
		((Select ISNULL(FirstName,'''') +  '' '' + ISNULL(LastName,'''')  from ChannelPartner CPSub where CPSub.Id=CPT.ParentId)) As IntroducerName,
		CP.ProfilePictureURL,UniqueNo,CONVERT(VARCHAR(20),CP.CreatedDate,105) as RegistrationDate
		FROM ChannelPartner CP
		INNER JOIN #TempChannelPartner CPT ON CPT.CpId=CP.Id
		INNER JOIN City C ON CP.CityId=C.Id WHERE 1=1'

	IF @Searchkeyword!=''
	BEGIN
		SET @WhereCondition=@WhereCondition + ' AND CP.FirstName like ''%'+@Searchkeyword+'%'' OR CP.LastName like ''%'+@Searchkeyword+'%'' OR CP.Email like ''%'+@Searchkeyword+'%'' OR CP.ContactNo like ''%'+@Searchkeyword+'%'''
	END
	IF @LevelId > 0
	BEGIN
		
		SET @WhereCondition=@WhereCondition+ ' AND CPT.LevelId=' +  CONVERT(VARCHAR(10),@LevelId)
	END
	IF @GenerationId > 0
	BEGIN
		SET @WhereCondition=@WhereCondition + ' AND CPT.Generation=' +  CONVERT(VARCHAR(10),@GenerationId)
	END
	SET @SqlQuery = @SqlQuery + @WhereCondition
	EXEC (@SqlQuery)
END

GO
CREATE PROCEDURE [ChannelPartner_GetPromotionLevel]
	@ChannelPartnerId INT
AS
BEGIN
	SELECT
	@ChannelPartnerId as ChannelPartnerId,
	PC.Title as CurrentLevel,
	((Select ISNULL(Title,'') FROM PromotionalCycle PCsub where PCsub.LevelId=(PC.LevelId+1))) As NextLevel,
	PC.AchievementCount as [Target],
	CPM.CurrentCount As Achieved,
	(PC.AchievementCount-CPM.CurrentCount) as Pending
	from ChannelPartnerMapping CPM
	INNER JOIN PromotionalCycle PC ON PC.LevelId=CPM.LevelId 
	WHERE ChannelPartnerId=@ChannelPartnerId
END
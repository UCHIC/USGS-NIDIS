Drop Table #temp3
Drop Table #tempDelta
Drop Table #temp4
Drop Table #tempCalculate


--DECLARE @origVariableID  = in(13,14) 

SELECT SiteID,
      VariableID,
      ROW_NUMBER() OVER(PARTITION BY SiteID, VariableID ORDER BY LocalDateTime) AS RowNum,
      LocalDateTime,
      DataValue,
      CensorCode, UTCOffset, ValueAccuracy, OffsetValue, OffsetTypeID, SampleID, DerivedFromID      
  INTO #temp3
  FROM DataValues 
  where (VariableID =13 OR VariableID =14 )AND LocalDateTime >'2011-01-01'
  --WHERE LastValue IS NOT NULL 


SELECT a.SiteID, a.VariableID, a.RowNum As FirstRowNum, a.DataValue As FirstDataValue,  a.LocalDateTime As FirstLocalDateTime, 
		 b.RowNum, b.DataValue, b.LocalDateTime, a.CensorCode, a.UTCOffset, a.ValueAccuracy, a.OffsetValue, a.OffsetTypeID, a.SampleID, a.DerivedFromID
Into  #tempDelta 
 FROM #temp3 AS a LEFT JOIN #temp3 AS b
  ON a.SiteID = b.SiteID AND a.RowNum = b.RowNum+1
--  Join Qualifiers
--  On Qualifiers.QualifierID= b.QualifierID
--  where b.QualifierID Between 15 AND 24 or b.QualifierID = 55  --ABS(DATEDIFF(DAY, a.LocalDateTime, b.LocalDateTime)) >1 OR 
  order by SiteID, b.LocalDateTime
  
select *from #tempDelta order by SiteID,VariableID, LocalDateTime

Select a.SiteID, a.VariableID, a.FirstRowNum, a.FirstDataValue, a.FirstLocalDateTime, 
		a.RowNum,a.DataValue,a.LocalDateTime, b.DataValue As PreviousDay, b.LocalDateTime As PrevLocalDateTime,
		a.CensorCode, a.UTCOffset, a.ValueAccuracy, a.OffsetValue, a.OffsetTypeID, a.SampleID, a.DerivedFromID
	--into #tempCalculate
	from #tempDelta As a		
	Left Join
	DataValues As b
	on a.SiteID =b.SiteID
		AND b.VariableID=a.VariableID--Monthly Only 'STOR-P1M'  a.VariableID--
		AND Cast(a.FirstLocalDateTime As DATE) = CAST( DATEADD(M,1, b.LocalDateTime)As DATE)
	--where a.LocalDateTime = NULL
--  where b.QualifierID Between 15 AND 24 or b.QualifierID = 55  --ABS(DATEDIFF(DAY, a.LocalDateTime, b.LocalDateTime)) >1 OR 
order by a.SiteID, a.variableID, a.FirstRowNum, a.LocalDateTime



SELECT 
	--Case --DataValue
		--when DataValue = NULL then FirstDataValue- PreviousDay
		--else 
		(FirstDataValue -DataValue)
		--when FirstDataValue= @NoDV then @NoDV
		--when DataValue= @NoDV then @NoDV 
		--when DataValue <> NULL then FirstDataValue -DataValue
		--when PreviousDay =@NoDV then @NoDV	
		--when DataValue=NULL AND PreviousDay<> NULL then  FirstDataValue- PreviousDay
	--End 
	as EOIDataValue, 
	--Case DataValue
	--	when Null Then PreviousDay
	--	else DataValue
--	End As PreviousValue,
--	FirstDataValue As CurrValue	,
	SiteID, VariableID, FirstLocalDateTime As LocalDateTime
--into #temp4
from #tempDelta
order by SiteID, VariableID, LocalDAteTime





--Monthly
Declare @origVariableID float = 12 
DECLARE @StartDate datetime = '2000-01-01'
SELECT a.SiteID, a.VariableID, a.LocalDateTime,  a.EOIValue-b.EOIValue As Value, a.LocalDateTime As aDateTime,a.RowNum, a.EOIValue As aValue, 
	 b.LocalDateTime AS bDateTime, b.RowNum as bRow, b.EOIValue As bValue,
	 Case
		when ABS(DATEDIFF(Month, a.LocalDateTime, b.LocalDateTime)) = 1 
			then a.EOIValue-b.EOIValue
		else
			null
	End As TesterValue, 
	DATEDIFF(Month, a.LocalDateTime, b.LocalDateTime) as MonthDiff
	
FROM (
(SELECT SiteID,
      VariableID,
      ROW_NUMBER() OVER(PARTITION BY SiteID, VariableID ORDER BY LocalDateTime) AS RowNum,
      LocalDateTime,
      EOIValue
  FROM #temp1) AS a 
LEFT JOIN 
(SELECT SiteID,
      VariableID,
      ROW_NUMBER() OVER(PARTITION BY SiteID, VariableID ORDER BY LocalDateTime) AS RowNum,
      LocalDateTime,
      EOIValue  
  FROM #temp1
  Union
	Select SiteID, VariableID, 0, LocalDateTime, DataValue as EOIValue 
	from  DataValues where  VariableID = @origVariableID  AND LocalDateTime =DATEADD(month, -1,@StartDate) 
   
  ) AS b
	ON a.SiteID = b.SiteID AND a.RowNum = b.RowNum+1 AND a.VariableID= b.VariableID)
 order by SiteID, a.LocalDateTime
 
 
 --BiMonthly
 
	
	
Declare @origVariableID float = 12 
DECLARE @StartDate datetime = '2000-01-01'	

	    Select a.SiteID, a.VariableID, a.LocalDateTime,  a.EOIValue-b.EOIValue As Value, a.LocalDateTime As aDateTime,a.RowNum, a.EOIValue As aValue, 
	 b.LocalDateTime AS bDateTime, b.RowNum as bRow, b.EOIValue As bValue, 
			Case
				When a.MonthPart = 1 AND ABS(DATEDIFF(Day, a.LocalDateTime, b.LocalDateTime)) = Abs(DateDiff(Day, a.LocalDateTime,(DateAdd(Day, 16, DateAdd(Month,-1,a.LocalDateTime)))))+1
					Then a.EOIValue-b.EOIValue
				When a.MonthPart = 2 AND ABS(DATEDIFF(Day, a.LocalDateTime, b.LocalDateTime)) = 15
					Then a.EOIValue-b.EOIValue
				Else
					null
			End As TesterValue, 
			
			Case
				When a.MonthPart = 1 
					Then ABS(DATEDIFF(Day, a.LocalDateTime, b.LocalDateTime))
				When a.MonthPart = 2 
					Then ABS(DATEDIFF(Day, a.LocalDateTime, b.LocalDateTime)) 
			End As DayDiff,
			Abs(DateDiff(Day, a.LocalDateTime,(DateAdd(Day, 16, DateAdd(Month,-1,a.LocalDateTime)))))+1 as Part1Expected
		From
			(SELECT SiteID,
			VariableID,
			ROW_NUMBER() OVER(PARTITION BY SiteID, VariableID ORDER BY LocalDateTime) AS RowNum,
			LocalDateTime,
			Case 
				when DatePart(DAY, LocalDateTime) Between 1 and 15 then 1 
				else 2
			End As MonthPart, 
			EOIValue			
			FROM #temp1) 
			AS a 
	   LEFT JOIN 
			(SELECT SiteID,
				VariableID,
				ROW_NUMBER() OVER(PARTITION BY SiteID, VariableID ORDER BY LocalDateTime) AS RowNum,
				LocalDateTime,  			
				Case 
					when DatePart(DAY, LocalDateTime) Between 1 and 15 then 1 
					else 2
				End As MonthPart,
				EOIValue 
				FROM #temp1
			Union
			Select SiteID, VariableID, 0, LocalDateTime, 
				Case 
					when DatePart(DAY, LocalDateTime) Between 1 and 15 then 1 
					else 2
				End As MonthPart, 
				DataValue as EOIValue				  
			from  DataValues 
			where  VariableID = @origVariableID  AND (LocalDateTime >DATEADD(Day, -20,@StartDate) )) 
			AS b
	  ON a.SiteID = b.SiteID AND a.RowNum = b.RowNum+1 AND a.VariableID= b.VariableID
	 order by SiteID, a.LocalDateTime
	 
	 
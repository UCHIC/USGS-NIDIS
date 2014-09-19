--NCDC DATA
-- create a table with all the dates in the requested date range
-- input startdate, enddate
-- output DataValues Table, with a value for all days



--------------------------------Part 1----------------------------------------------------
--create a table with all days from start date to end date. At midnight
with mycte as
(
	select cast('2010-12-01' as datetime) DateValue
	union all
	select DateValue + 1
	from    mycte   
	where   DateValue + 1 < '2011-01-01'
)
select DateValue into #DailyValue
from    mycte
--where  DatePart (day , DateValue) =1 OR DATEPART(day, DateValue)=16
OPTION (MAXRECURSION 0)
select * from #dAilyValue
Drop Table #DailyValue

--Create a table with all of the entries from series catalog to have all dates for all sites
--join a table with every day in it with the series catalog table. This will create a list of 
--the requested dates for every entry in the Series Catalog Table. The next step is to get or create
--a datavalue for each of the entries in this table
select DV.DateValue, SC.SiteID , SC.VariableID --, SC.BeginDateTime, SC.EndDateTime
into #AllDates
from #DailyValue AS DV
inner join 
SeriesCatalog AS SC
ON 1=1
--where DV.DateValue Between SC.BeginDateTime AND SC.EndDateTime --OR DV.DateValue = SC.BeginDateTime
order by DateValue,SiteID

select SiteID from #AllDates --group by SeriesID order by SiteID


Drop Table #AllDates
-------------------------------End Part 1----------------------------------





--------------------------------Part 2 ----------------------------------------
--TEMP1

SELECT
      SiteID,
      ROW_NUMBER() OVER(PARTITION BY SiteId ORDER BY LocalDateTime) AS RowNum,
      LocalDateTime,
      DataValue,
      QualifierID,  
      ValueAccuracy, UTCOffset, Null as DateTimeUTC, 
		OffsetValue, OffsetTypeId, CensorCode, MethodID, SourceID, SampleID, DerivedFromID, QualityControlLevelID 
  INTO #temp1
  FROM DataValues
  WHERE DataValue IS NOT NULL AND LocalDateTime Between'2005-01-01' AND '2005-01-02' AND QualifierID NOT in(3, 17, 44)-- 3, 17, 44 indicate an Invalid data element with a replacement so the value is useless

Select * from #temp1


--Join the DataValues Table with itself and the previous row to get the interval
--TEMP2
SELECT a.SiteID, a.RowNum As FirstRowNum, a.DataValue As FirstDataValue,  a.LocalDateTime As FirstLocalDateTime, 
		a.QualifierID As FirstQualifierID,b.RowNum, b.DataValue, b.LocalDateTime, b.QualifierID, Qualifiers.QualifierDescription,
		Cast (NULL AS Float) As DateDifference, Cast(NULL As Float) As calcValue, a.ValueAccuracy, a.UTCOffset, a.DateTimeUTC, 
		a.OffsetValue, a.OffsetTypeId, a.CensorCode, a.MethodID, a.SourceID, a.SampleID, a.DerivedFromID, a.QualityControlLevelID
Into #temp2 
  FROM #temp1 AS a LEFT JOIN #temp1 AS b
  ON a.SiteID = b.SiteID AND a.RowNum = b.RowNum-1
  Join Qualifiers
  On Qualifiers.QualifierID= b.QualifierID
  where b.QualifierID Between 15 AND 24 or b.QualifierID = 55  --ABS(DATEDIFF(DAY, a.LocalDateTime, b.LocalDateTime)) >1 OR 
  order by SiteID, b.LocalDateTime
  

--calculate the the DataValue including the first day of the interval
Update #temp2 
	Set FirstDataValue = -99999, DateDifference= ABS(DATEDIFF(DAY, FirstLocalDateTime, LocalDateTime))+1, 
		calcValue =  DataValue/(ABS(DATEDIFF(DAY, FirstLocalDateTime, LocalDateTime))+1)
	Where FirstQualifierID in(4, 18, 27, 34, 35, 36, 37, 38, 39, 40, 41, 45) --NoDataValues	


--calculate the the DataValue excluding the first day of the interval
Update #temp2 
	Set DateDifference= ABS(DATEDIFF(DAY, FirstLocalDateTime, LocalDateTime)), calcValue = DataValue/(ABS(DATEDIFF(DAY, FirstLocalDateTime, LocalDateTime)))
	Where FirstQualifierID NOT in (4, 18, 27, 34, 35, 36, 37, 38, 39, 40, 41, 45) --DataValues


-----------------------------------------End Part 2-----------------------------------------------





-------------------------------------------Part 3-------------------------------------------------
--Join the calculated values with the #alldates table so that we have all the required values far all of the dates in the period
Select  ad.DateValue, ad.VariableID, ad.SiteID, t.FirstDataValue,  t.FirstLocalDateTime, 
		t.FirstQualifierID, t.RowNum, t.DataValue, t.LocalDateTime, QualifierID, t.QualifierDescription,
		t.DateDifference, t.calcValue, t.ValueAccuracy, t.UTCOffset, t.DateTimeUTC, 
		t.OffsetValue, t.OffsetTypeId, t.CensorCode, t.MethodID, t.SourceID, t.SampleID, t.DerivedFromID, t.QualityControlLevelID
into #temp3
from #AllDates as ad
left join
--join
#temp2 as t
on ad.DateValue Between t.FirstLocalDateTime AND t.LocalDateTime AND ad.SiteID = t.SiteID--AND ad.VariableID= t.VariableID
order by SiteID, DateValue


--Select * --into #temp4
--from #temp3 as t3
--left join
--DataValues AS DV
--on  (DatePart(day, t3.DateValue) = DatePart(day, DV.LocalDateTime) AND DatePart(YEAR, t3.DateValue) = DatePart(YEAR, DV.LocalDateTime) 
--	AND DatePart(MONTH, t3.DateValue) = DatePart(MONTH, DV.LocalDateTime)) AND DV.SiteID= t3.SiteID AND DV.VariableID = t3.VariableID
----where calcvalue <> NULL
--order by t3.SiteID, DateValue


--Join the table with all dates and calculated values with the DAtaValues Table so we have all possible values associated with correct dates in one table
Select t3.DateValue, 
	t3.VariableID AS CalcVariableID, t3.SiteID AS CalcSiteID, t3.calcValue AS CalcDataValue, t3.DateValue AS CalcLocalDateTime, 
	t3.QualifierID AS CalcQualifierID, t3.ValueAccuracy AS CalcValueAccuracy, t3.UTCOffset AS CalcUTCOffset, 
	DATEADD(hour, -t3.UTCOffset, t3.DateValue) AS CalcDateTimeUTC,
	t3.OffsetValue AS CalcOffsetValue, t3.OffsetTypeID AS CalcOffsetTypeID, t3.CensorCode AS CalcCensorCode, t3.MethodID AS CalcMethodID, 
	t3.SourceID AS CalcSourceID, t3.SampleID AS CalcSampleID, t3.DerivedFromID AS CalcDerivedFromID, t3.QualityControlLevelID AS CalcQualityControlLevelID,
	DV.VariableID, DV.SiteID, DV.DataValue, DV.LocalDateTime, DV.QualifierID, DV.ValueAccuracy, DV.UTCOffset, DV.DateTimeUTC, 
	DV.OffsetValue, DV.OffsetTypeId, DV.CensorCode, DV.MethodID, DV.SourceID, DV.SampleID, DV.DerivedFromID, DV.QualityControlLevelID
into #temp5
from #temp3 as t3
left join
DataValues AS DV
on  (DatePart(day, t3.DateValue) = DatePart(day, DV.LocalDateTime) AND DatePart(YEAR, t3.DateValue) = DatePart(YEAR, DV.LocalDateTime) 
AND DatePart(MONTH, t3.DateValue) = DatePart(MONTH, DV.LocalDateTime)) AND DV.SiteID= t3.SiteID AND DV.VariableID = t3.VariableID
--where calcvalue <> NULL
order by t3.SiteID, DateValue

Select * from #temp5


Declare @NoDataValue float= -99999
Select Null As ValueID, DateValue, 
	Case 	
		When DataValue is NULL AND CalcDataValue is NULL then @NoDataValue
		When DataValue is NULL then CalcDataValue		
		When QualifierID Between 34 And 41 OR QualifierID Between 15 AND 24  then CalcDataValue
		Else DataValue
	End As DataValue,  
	Case 
		When CalcValueAccuracy is not NULL AND ValueAccuracy is NULL then CalcValueAccuracy
		Else ValueAccuracy 
	End As ValueAccuracy, 
	Case 
		When LocalDateTime is Not NULL Then LocalDateTime
		When CalcLocalDateTime is not NULL Then CalcLocalDateTime
		Else LocalDateTime
	End As LocalDateTime, 
	Case 
		When UTCOffset is not NULL Then UTCOffset
		When CalcUTCOffset is not NULL Then CalcUTCOffset
		Else ( Select Top 1 UTCOffset from DataValues where SiteID=T5.SiteID AND VariableID = T5.VariableID )	
	End As UTCOffset,
	Case 
		When DateTimeUTC is not NULL Then DateTimeUTC
		When CalcDateTimeUTC is not NULL Then CalcDateTimeUTC
		Else DateAdd( HOUR, -(Select Top 1 UTCOffset from DataValues where SiteID=T5.SiteID AND VariableID = T5.VariableID), LocalDateTime )
	End As DateTimeUTC, 
	Case 
		When SiteID is NULL then CalcSiteID
		Else SiteID
	End As 	SiteID, 	
	Case 	
		when VariableID is NULL then CalcVariableID
		Else VariableID		
	End As VariableID, 
	Case 
		When CalcOffsetValue is not  NULL AND OffsetValue is NULL then CalcOffsetValue
		Else OffsetValue 		
	End As OffsetValue, 
	Case 
		When CalcOffsetTypeId is not  NULL AND OffsetTypeId is NULL then CalcOffsetTypeId
		Else OffsetTypeID 		
	End As OffsetTypeId,	
	Case 
		When CalcCensorCode is not  NULL AND CensorCode is NULL then CalcCensorCode
		When CensorCode is not Null then CensorCode
		Else ( Select Top 1 CensorCode from DataValues where SiteID=T5.SiteID AND VariableID = T5.VariableID )
	End As CensorCode,
	Case 
		When DataValue is NULL AND CalcDataValue is NULL then Null
		When DataValue is NULL then CalcQualifierID		
		--When QualifierID Between 34 And 41 OR QualifierID Between 15 AND 24 then 99
		Else QualifierID		
	End As QualifierID,
	Case 
		When CalcMethodID is not  NULL AND MethodID is NULL then CalcMethodID
		When MethodID is not Null then MethodID
		Else ( Select Top 1 MethodID from DataValues where SiteID=T5.SiteID AND VariableID = T5.VariableID )
	End As MethodID, 
	Case 
		When CalcSourceID is not  NULL AND SourceID is NULL then CalcSourceID
		When SourceID is not Null then SourceID
		Else ( Select Top 1 SourceID from DataValues where SiteID=T5.SiteID AND VariableID = T5.VariableID )
	End As SourceID, 
	Case 
		When CalcSampleID is not  NULL AND SampleID is NULL then CalcSampleID
		Else SampleID 
	End As SampleID, 
	Case 
		When CalcDerivedFromID is not  NULL AND DerivedFromID is NULL then CalcDerivedFromID
		Else DerivedFromID
	End As DerivedFromID, 
	Case 
		When CalcQualityControlLevelID is not  NULL AND QualityControlLevelID is NULL then CalcQualityControlLevelID
		When QualityControlLevelID is not NULL then QualityControlLevelID
		Else ( Select Top 1 QualityControlLevelID from DataValues where SiteID=T5.SiteID AND VariableID = T5.VariableID )
	End As QualityControlLevelID	
	
into #FinalValues
from #temp5 As T5

--Delete from #FinalValues Where ( UTCOffset is Null AND OffsetValue is Null And CensorCode is Null And QualifierID is null And MethodID is Null  and SourceID is NuLL)
	

----Join the DataValues table with the AllDates table so that we have dataValues for all of the dates in the Required time peried
--Select	
--	ad.DateValue, ad.VariableID, ad.SiteID, DV.DataValue, DV.LocalDateTime, DV.QualifierID, DV.ValueAccuracy, DV.UTCOffset, DV.DateTimeUTC, 
--	DV.OffsetValue, DV.OffsetTypeId, DV.CensorCode, DV.MethodID, DV.SourceID, DV.SampleID, DV.DerivedFromID, DV.QualityControlLevelID	
--into #temp4
--from #AllDates as ad
--left join
--DataValues AS DV
--on  (DatePart(day, ad.DateValue) = DatePart(day, DV.LocalDateTime) AND DatePart(YEAR, ad.DateValue) = DatePart(YEAR, DV.LocalDateTime) 
	--AND DatePart(MONTH, ad.DateValue) = DatePart(MONTH, DV.LocalDateTime)) AND DV.SiteID= ad.SiteID AND DV.VariableID = ad.VariableID
----where calcvalue <> NULL
--order by  ad.SiteID, ad.VariableID,	DateValue



--Select * into #FinalValues
--from #temp4
--Union
--Select DateValue, VariableID, SiteID, calcValue, DateValue, QualifierID, ValueAccuracy, UTCOffset, DATEADD(hour, -UTCOffset, DateValue),
--		OffsetValue, OffsetTypeID, CensorCode, MethodID, SourceID, SampleID, DerivedFromID, QualityControlLevelID
--from #temp3
--Order by #temp4.SiteID, #temp4.VariableID, #temp4.LocalDateTime







--GO


Drop Table #temp1
Drop Table #temp2
Drop Table #temp3
Drop Table #temp4
Drop Table #temp5
Drop Table #AllDates
Drop Table #DailyValue
Drop Table #FinalValues








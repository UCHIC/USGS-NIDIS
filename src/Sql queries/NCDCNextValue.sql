
Drop Table #temp1
Drop Table #temp2
 --create Table #temp2

SELECT
      SiteID,
      ROW_NUMBER() OVER(PARTITION BY SiteId ORDER BY LocalDateTime) AS RowNum,
      LocalDateTime,
      DataValue,
      QualifierID   
  INTO #temp1
  FROM DataValues
  WHERE DataValue IS NOT NULL  AND QualifierID NOT in(3, 17, 44)--AND LocalDateTime >'1999-01-01'

--SELECT
--      SiteID,
--      ROW_NUMBER() OVER(PARTITION BY SiteId ORDER BY LocalDateTime) AS RowNum,
--      LocalDateTime,
--      DataValue,
--      QualifierID      
--  INTO #temp2
--  FROM DataValues
--  WHERE DataValue IS NOT NULL

SELECT a.SiteID, a.RowNum As FirstRowNum, a.DataValue As FirstDataValue,  a.LocalDateTime As FirstLocalDateTime, 
		a.QualifierID As FirstQualifierID, b.RowNum, b.DataValue, b.LocalDateTime, b.QualifierID, Qualifiers.QualifierDescription,
		Cast (NULL AS Float) As DateDifference, Cast(NULL As Float) As calcValue

Into #temp2 
  FROM #temp1 AS a LEFT JOIN #temp1 AS b
  ON a.SiteID = b.SiteID AND a.RowNum = b.RowNum-1
  Join Qualifiers
  On Qualifiers.QualifierID= b.QualifierID
  where b.QualifierID Between 15 AND 24 or b.QualifierID = 55  --ABS(DATEDIFF(DAY, a.LocalDateTime, b.LocalDateTime)) >1 OR 
  order by SiteID, b.LocalDateTime
Select * from #temp1 where  QualifierID Between 15 AND 24 or QualifierID = 55 

Update #temp2 
	Set FirstDataValue = -99999, DateDifference= ABS(DATEDIFF(DAY, FirstLocalDateTime, LocalDateTime))+1, calcValue =  DataValue/(ABS(DATEDIFF(DAY, FirstLocalDateTime, LocalDateTime))+1)
	Where FirstQualifierID in(4, 18, 27, 34, 35, 36, 37, 38, 39, 40, 41, 45) --NoDataValues	

Update #temp2 
	Set DateDifference= ABS(DATEDIFF(DAY, FirstLocalDateTime, LocalDateTime)), calcValue = DataValue/(ABS(DATEDIFF(DAY, FirstLocalDateTime, LocalDateTime)))
	Where FirstQualifierID NOT in (4, 18, 27, 34, 35, 36, 37, 38, 39, 40, 41, 45) --DataValues

Select * From #temp2
GO


--Drop Table #temp1
--Drop Table #temp2


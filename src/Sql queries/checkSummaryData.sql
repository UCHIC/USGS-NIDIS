SELECT a.SiteID,
         SiteCode,
         COUNT(*) as DVCount,
         MIN(YEAR(LocalDateTime)) as BeginYear,
         MAX(YEAR(LocalDateTime)) as EndYear
      FROM [Summary].[dbo].[DataValues] AS a LEFT JOIN [Summary].[dbo].Sites AS b ON a.SiteID = b.SiteID
    WHERE VariableID = 4  -- 6 = Raw stream guage data
    GROUP BY a.SiteID, SiteCode
    HAVING COUNT(*) > 1 AND MAX(YEAR(LocalDateTime)) > 2005 -- Filter = # of data values > 100 and at least 1 data value more recent then 2005
    ORDER BY EndYear DESC
GO

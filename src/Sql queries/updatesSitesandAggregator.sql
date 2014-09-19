



update L1HarvestList 
set  SiteID = 
Case  
	when  (select  SiteID from #temp As T
		where T.SiteCode= L1HarvestList.SiteCode) 
		Is Null then 0
	else 
		(
		select SiteID from #temp As T
		where T.SiteCode=  L1HarvestList.SiteCode	
		)
End

  
  update Summary.dbo.Sites
 set SiteType = 
  case SiteType 
  when 'NRCS SNOTEL Site' then 'NRCS-SNOTEL'
  when 'USBR Reservoir' then 'USBR-Reservoir'
  when 'USGS Streamflow Gage' then 'USGS-Streamflow'
  when 'NCDC Weather Station' then 'NCDC-Weather'
  end  


update SummaryNew.dbo.L1HarvestList
set  SiteID = 
	Case  
		when  (select SiteID from [SNOTEL-L1].dbo.Sites As T
			where T.SiteCode= SUBSTRING(L1HarvestList.SiteCode, CHARINDEX(':', L1HarvestList.SiteCode, 0)+1 ,LEN(L1HarvestList.SiteCode)) 
			)
			Is Null then 0
		else 
			(
			select SiteID from [SNOTEL-L1].dbo.Sites As T
			where T.SiteCode=  SUBSTRING(L1HarvestList.SiteCode, CHARINDEX(':', L1HarvestList.SiteCode, 0)+1 ,LEN(L1HarvestList.SiteCode))
			)
	End
where SiteType = 'NRCS-SNOTEL'
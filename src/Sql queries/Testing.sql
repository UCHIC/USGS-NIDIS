Declare @frominitstring Text = 'Data Source=wasser.uwrl.usu.edu;Catalog=Summary;User ID=ODM;Password=odm'
Declare @table nvarchar = 'Summary'

select top 1000 * from
OPENDATASOURCE(
         'SQLOLEDB',
         'Data Source=wasser.uwrl.usu.edu;Catalog=LittleBear11;User ID=ODM;Password=odm'
         ).@table.dbo.DataValues
         order by ValueID desc
         
 select top 1000 * from         
OPENDATASOURCE(
         'SQLOLEDB',
         'Data Source=drought.usu.edu;Initial Catalog=Summary;User ID=ODM;Password=odm'
         ).Summary.dbo.DataValues
          order by ValueID desc

--sp_configure 'show advanced options', 1;
--GO
--RECONFIGURE;
--GO
--sp_configure 'Ad Hoc Distributed Queries', 1;
--GO
--RECONFIGURE;
--GO



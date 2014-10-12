# -*- coding: utf-8 -*-
"""
Created on Fri Oct 10 16:12:33 2014

@author: Jiri
"""
import datetime
import re
import sqlalchemy as sa
import requests
import urllib2
from bs4 import BeautifulSoup


class SNOTELHarvester():
    def __init__(self, connection_string):
        #code to initialize the database connection object
        self.connection_string = connection_string
        self.db = sa.create_engine(self.connection_string)
        self.metadata = sa.MetaData()
        self.variable_lookup = {u'Snow Depth':'SNWD',
                                u'Air Temperature':'TAVG',
                                u'Precipitation Accumulation':'PREC',
                                u'Snow Water Equivalent':'WTEQ'}
        self.unit_lookup = {'SNWD':'centimeter',
                            'PREC':'millimeter',
                            'WTEQ':'millimeter',
                            'PRCP':'millimeter',
                            'TMAX':'degree celsius',
                            'TMIN':'degree celsius',
                            'TAVG':'degree celsius',
                            'TEMP':'degree celsius'}
    
    
    def get_odm_object(self,table_name, primary_key, object_id):
        with self.db.begin() as conn:
            t = sa.Table(table_name, self.metadata, autoload=True, autoload_with=conn)
            s = t.select(t.c[primary_key] == object_id)    
            rs = conn.execute(s)
            r = rs.fetchone()
            return r
    
    
    def add_odm_object(self, obj, table_name, primary_key, unique_column):
        with self.db.begin() as conn:

            # will load table_name exactly once, then store it persistently
            # within the above MetaData
            t = sa.Table(table_name, self.metadata, autoload=True, autoload_with=conn)

            s = t.select(t.c[unique_column] == obj[unique_column])    
            rs = conn.execute(s)
            r = rs.fetchone()
            if not r:
                i_res = conn.execute(t.insert(), obj)
                v_id = i_res.inserted_primary_key[0]
                return v_id
            else:
                return r[primary_key]   
    
    
    def update_source(self):
        #fills the fields in the ODM sources table
        source = {'Organization':'USDA NRCS',
                  'SourceDescription':'SNOTEL Data',
                  'SourceLink':'http://www.wcc.nrcs.usda.gov/snow/',
                  'ContactName':'Jiri Kadlec',
                  'Phone':'385-204-2998',
                  'Email':'jirikadlec2@gmail.com',
                  'Address':'Clyde Building',
                  'City':'Provo',
                  'State':'Utah',
                  'ZipCode':'84604',
                  'Citation':'add citation here..',
                  'MetadataID':None}
        md = {'TopicCategory':'climatology/meteorology/atmosphere',
              'Title':'title01',
              'Abstract':'abstract01',
              'ProfileVersion':'Profile01',
              'MetadataLink':'Link01'}
        md_key = self.add_odm_object(md, 'isometadata','MetadataID','Title')
        source['MetadataID'] = md_key
        self.add_odm_object(source,'sources','SourceID','Organization')
        
        
    def update_variables(self):
        variables = [
        {'VariableCode':'WTEQ',
         'VariableName':'Snow Water Equivalent',
         'Speciation':'Not applicable',
         'VariableunitsID':54,
         'SampleMedium':'Snow',
         'ValueType':'Field Observation',
         'IsRegular':'True',
         'TimeSupport':0,
         'TimeunitsID':104,
         'DataType':'Continuous',
         'GeneralCategory':'Climate',
         'NoDataValue':-9999
         },
         {'VariableCode':'PREC',
         'VariableName':'Precipitation',
         'Speciation':'Not applicable',
         'VariableunitsID':54,
         'SampleMedium':'Precipitation',
         'ValueType':'Field Observation',
         'IsRegular':'True',
         'TimeSupport':1,
         'TimeunitsID':104,
         'DataType':'Cumulative',
         'GeneralCategory':'Climate',
         'NoDataValue':-9999
         },
         {'VariableCode':'PRCP',
         'VariableName':'Precipitation',
         'Speciation':'Not applicable',
         'VariableunitsID':54,
         'SampleMedium':'Precipitation',
         'ValueType':'Field Observation',
         'IsRegular':'True',
         'TimeSupport':1,
         'TimeunitsID':104,
         'DataType':'Incremental',
         'GeneralCategory':'Climate',
         'NoDataValue':-9999
         },
         {'VariableCode':'TMAX',
         'VariableName':'Temperature',
         'Speciation':'Not applicable',
         'VariableunitsID':96,
         'SampleMedium':'Air',
         'ValueType':'Field Observation',
         'IsRegular':'True',
         'TimeSupport':1,
         'TimeunitsID':104,
         'DataType':'Maximum',
         'GeneralCategory':'Climate',
         'NoDataValue':-9999
         },
         {'VariableCode':'TMIN',
         'VariableName':'Temperature',
         'Speciation':'Not applicable',
         'VariableunitsID':96,
         'SampleMedium':'Air',
         'ValueType':'Field Observation',
         'IsRegular':'True',
         'TimeSupport':1,
         'TimeunitsID':104,
         'DataType':'Minimum',
         'GeneralCategory':'Climate',
         'NoDataValue':-9999
         },
         {'VariableCode':'TAVG',
         'VariableName':'Temperature',
         'Speciation':'Not applicable',
         'VariableunitsID':96,
         'SampleMedium':'Air',
         'ValueType':'Field Observation',
         'IsRegular':'True',
         'TimeSupport':1,
         'TimeunitsID':104,
         'DataType':'Average',
         'GeneralCategory':'Climate',
         'NoDataValue':-9999
         },
         {'VariableCode':'SNWD',
         'VariableName':'Snow Depth',
         'Speciation':'Not applicable',
         'VariableunitsID':47,
         'SampleMedium':'Air',
         'ValueType':'Field Observation',
         'IsRegular':'True',
         'TimeSupport':0,
         'TimeunitsID':104,
         'DataType':'Continuous',
         'GeneralCategory':'Climate',
         'NoDataValue':-9999
         }
        ]
        for v in variables:
            self.add_odm_object(v, 'variables', 'VariableID', 'VariableCode')
  
  
    def strip_quotes(self,text):
        return text.replace('"','').replace('[','')
    
        
    def update_sites(self):
        #fills in the SNOTEL sites
        #uses the file: 'http://www.wcc.nrcs.usda.gov/ftpref/data/water/wcs/map/stations.js'
        sites_url = 'http://www.wcc.nrcs.usda.gov/ftpref/data/water/wcs/map/stations.js'
        sites = []
        spatial_ref = self.get_odm_object('spatialreferences','SRSID',4326)
        spatial_ref_id = spatial_ref[0]
        print spatial_ref
        r = requests.get(sites_url)

        for line in r.iter_lines():
            attrs = line.split(',')
            if len(attrs) > 6:
                network = self.strip_quotes(attrs[5])
                site = {'SiteName':self.strip_quotes(attrs[0]),
                    'Latitude':attrs[1],
                    'Longitude':attrs[2],
                    'SiteCode':attrs[3].replace('"',''),
                    'State':attrs[4].replace('"',''),
                    'Elevation_m':attrs[6],
                    'SiteType':'Atmosphere',
                    'LatLongDatumID':spatial_ref_id,
                    'VerticalDatum':'Unknown'
                }
                if network == 'SNTL':
                    sites.append(site)
        
        for s in sites:
            self.add_odm_object(s, 'sites','SiteID','SiteCode')
    

    def get_variable_code(variable_name):
        return 'vcode'
                

    def get_site_info(self, site_code):
        snotel_vars = [u'Precipitation Accumulation',u'Snow Depth', 
        u'Snow Water Equivalent',u'Air Temperature']
        url = "http://www.wcc.nrcs.usda.gov/nwcc/site?sitenum=" + str(site_code)
        #for extracting the date in the yyyy-mm-dd format
        date_regex = r'\b\d{4}-\d{1,2}-\d{1,2}\b'
        variables = []
        content = urllib2.urlopen(url).read()
        soup = BeautifulSoup(content)
        found = soup.find('select', attrs= {'id':'report'})
        opts = found.find_all('option')
        for o in opts:
            option_text = str(o)
            found_dates = re.findall(date_regex, option_text)
            if len(found_dates) > 0:
                found_date = datetime.datetime.strptime(found_dates[0],'%Y-%m-%d')
                variable_name = o.contents[0].replace('(','').strip()
                if variable_name in snotel_vars:
                    variables.append({'variable':variable_name, 'date':found_date})

        return variables
        
        
    def insert_series(self, site_code, variable_code, begin_time):
        site = self.get_odm_object('sites', 'SiteCode', site_code)
        source = self.get_odm_object('sources','Organization','USDA NRCS') 
        vo = self.get_odm_object('variables','VariableCode', v_code)
            
        sc = {'SiteID':site['SiteID'],
              'SiteCode':site['SiteCode'],
              'SiteName':site['SiteName'],
              'SiteType':site['SiteType'],
              'VariableID':vo['VariableID'],
              'VariableCode':vo['VariableCode'],
              'Speciation':vo['Speciation'],
              'VariableunitsID':vo['VariableunitsID'],
              'VariableunitsName':self.variable_lookup[vo['VariableCode']],
              'SampleMedium':vo['SampleMedium'],
              'ValueType':vo['ValueType'],
              'TimeSupport':vo['TimeSupport'],
              'TimeunitsID':vo['TimeunitsID'],
              'TimeunitsName':'day',
              'DataType':vo['DataType'],
              'GeneralCategory':vo['GeneralCategory'],
              'MethodID':1,
              'MethodName':'No method specified',
              'SourceID':source['SourceID'],
              'Organization':source['Organization'],
              'Citation':source['Citation'],
              'QualityControlLevelID':1,
              'QualityControlLevelCode':'1'
              }
        with self.db.begin() as conn:
            t = sa.Table('seriescatalog', self.metadata, autoload=True, autoload_with=conn)
            conn.execute(t.insert(), sc)

    def update_series(self, site_code, variable_code):
        return 0

            
    def update_series_catalog(self, site_code, variables):
        #get the site object
        utc_offset = -7
        
        #for each variable update the sc
        for v in variables:
            v_code = self.variable_lookup[v['variable']]
            
            #check if an entry already exists in seriesCatalog:
            with self.db.begin() as conn:
                t = sa.Table('seriescatalog', self.metadata, autoload=True, autoload_with=conn)
                s = t.select(t.c['SiteCode'] == site_code and t.c['VariableCode'] == v['VariableCode'])
                rs = conn.execute(s)
                r = rs.fetchone()
                if not r:
                    #insert new item to seriesCatalog
                    insert_series(site_code, v_code)
                else:
                    #update the seriesCatalog items valueCount and EndDate
                    update_series(site_code, v_code)
            rs = conn.execute(s)
            r = rs.fetchone()
            return r            
            
            
#get the db's variable
            v2 = 'vole'
        return 1


if __name__ == '__main__':
    sh = SNOTELHarvester('mysql+pymysql://WWO_Admin:isaiah4118@worldwater.byu.edu/snotel')
        
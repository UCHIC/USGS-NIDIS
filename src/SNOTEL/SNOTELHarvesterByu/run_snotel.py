__author__ = 'Jiri'

from snotel_harvester import SNOTELHarvester
sh = SNOTELHarvester('mysql+pymysql://WWO_Admin:isaiah4118@worldwater.byu.edu/snotel')
sites = sh.get_site_codes()
for s in sites:
    if s == "823":
        sh.update_series_catalog(s)
sh.update_series_all()

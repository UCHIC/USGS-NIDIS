using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for clsUSBRList
/// </summary>
public class clsUSBRList
{
    static private List<USBRSite> sites;

    public static List<USBRSite> SitesList
    {
        get { return clsUSBRList.sites; }
        set { clsUSBRList.sites = value; }
    }
    static clsUSBRList()
    {
        sites = new List<USBRSite>();
    }

    static public List<USBRSite> createSiteList()
    {
        //Release: cubic ft/sec
        //Inflow: cubic ft/sec
        //Pool elevation: Feet
        //Storage: Acre-feet
        //Electric Energy: megawatt hrs
        //Power release: cubic ft/sec
        //Side inflow: cubic ft/sec 
        List<USBRSite> sites = new List<USBRSite>();
        List <USBRVariable> vars = new List <USBRVariable>();
        vars.Add(new USBRVariable("Pool Elevation", "19126"));
        sites.Add(new USBRSite("ABIQUIU+RESERVOIR", "Abiquiu Reservoir", "USBR_ABR", vars));//<input type='hidden' name='19127' value='No_data' />

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1734")); vars.Add(new USBRVariable("Inflow", "1805")); vars.Add(new USBRVariable("Release", "1885")); vars.Add(new USBRVariable("Pool Elevation", "1941"));
        sites.Add(new USBRSite("BIG+SANDY+RESERVOIR", "Big Sandy Reservoir", "USBR_BSR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Electric Energy", "2265"));
        sites.Add(new USBRSite("BLUE+MESA+POWER+PLANT", "Blue Mesa Power Plant", "USBR_BMPP", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1714")); vars.Add(new USBRVariable("Inflow", "1787")); vars.Add(new USBRVariable("Release", "1867")); vars.Add(new USBRVariable("Pool Elevation", "1923")); vars.Add(new USBRVariable("Power Release ", "1857"));
        sites.Add(new USBRSite("BLUE+MESA+RESERVOIR", "Blue Mesa Reservoir", "USBR_BMR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1748")); vars.Add(new USBRVariable("Inflow", "1819")); vars.Add(new USBRVariable("Release", "1899")); vars.Add(new USBRVariable("Pool Elevation", "1955"));
        sites.Add(new USBRSite("BOTTLE+HOLLOW+RESERVOIR%2C+UTAH", "Bottle Hollow Reservoir, Utah", "USBR_BHR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1735")); vars.Add(new USBRVariable("Inflow", "1806")); vars.Add(new USBRVariable("Release", "1886")); vars.Add(new USBRVariable("Pool Elevation", "1942"));
        sites.Add(new USBRSite("BRANTLEY+LAKE", "Brantley Lake", "USBR_BRL", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "2678")); vars.Add(new USBRVariable("Inflow", "2680")); vars.Add(new USBRVariable("Release", "2682")); vars.Add(new USBRVariable("Pool Elevation", "2679"));
        sites.Add(new USBRSite("CABALLO+RESERVOIR", "Caballo Reservoir", "USBR_CBR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1736")); vars.Add(new USBRVariable("Inflow", "1807")); vars.Add(new USBRVariable("Release", "2682")); vars.Add(new USBRVariable("Pool Elevation", "2679"));
        sites.Add(new USBRSite("CAUSEY+RESERVOIR", "Causey Reservoir", "USBR_CSR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "19045")); vars.Add(new USBRVariable("Inflow", "19058")); vars.Add(new USBRVariable("Release", "19072")); vars.Add(new USBRVariable("Pool Elevation", "19047"));
        sites.Add(new USBRSite("COCHITI+LAKE+NEAR+COCHITI+PUEBLO", "Cochiti Lake Near Cochiti Pueblo", "USBR_CCP", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1749")); vars.Add(new USBRVariable("Inflow", "1820")); vars.Add(new USBRVariable("Release", "1900")); vars.Add(new USBRVariable("Pool Elevation", "1956"));
        sites.Add(new USBRSite("CRAWFORD+RESERVOIR%2C+COLORADO", "Crawford Reservoir, Colorado", "USBR_CWR", vars));
        //megawatt hrs
        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Electric Energy", "2270"));
        sites.Add(new USBRSite("CRYSTAL+POWER+PLANT", "Crystal Power Plant", "USBR_CRPP", vars));
        //acre feet
        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1716")); vars.Add(new USBRVariable("Inflow", "1789")); vars.Add(new USBRVariable("Release", "1869")); vars.Add(new USBRVariable("Pool Elevation", "1925")); vars.Add(new USBRVariable("Side Inflow", "1844")); vars.Add(new USBRVariable("Power Release", "1859"));
        sites.Add(new USBRSite("CRYSTAL+RESERVOIR", "Crystal Reservoir", "USBR_CRR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1750")); vars.Add(new USBRVariable("Inflow", "1821")); vars.Add(new USBRVariable("Release", "1901")); vars.Add(new USBRVariable("Pool Elevation", "1957"));
        sites.Add(new USBRSite("CURRANT+CREEK+RESERVOIR%2C+UTAH", "Currant Creek Reservoir, Utah", "USBR_CCR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1751")); vars.Add(new USBRVariable("Inflow", "1822")); vars.Add(new USBRVariable("Release", "1902")); vars.Add(new USBRVariable("Pool Elevation", "1958"));
        sites.Add(new USBRSite("DEER+CREEK+RESERVOIR%2C+UTAH", "Deer Creek Reservoir, Utah", "USBR_DCR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1738")); vars.Add(new USBRVariable("Inflow", "1809")); vars.Add(new USBRVariable("Release", "1889")); vars.Add(new USBRVariable("Pool Elevation", "1945"));
        sites.Add(new USBRSite("EAST+CANYON+RESERVOIR+NEAR+MORGAN", "East Canyon Reservoir Near Morgan", "USBR_ECR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1739")); vars.Add(new USBRVariable("Inflow", "1810")); vars.Add(new USBRVariable("Release", "1890")); vars.Add(new USBRVariable("Pool Elevation", "1946"));
        sites.Add(new USBRSite("ECHO+RESERVOIR", "Echo Reservoir", "USBR_EHR", vars));

        //Historical only
        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1752")); vars.Add(new USBRVariable("Inflow", "1823")); vars.Add(new USBRVariable("Release", "1903")); vars.Add(new USBRVariable("Pool Elevation", "1959"));
        sites.Add(new USBRSite("EDEN+RESERVOIR", "Eden Reservoir", "USBR_EDR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Pool Elevation", "19547"));
        sites.Add(new USBRSite("EL+VADO+RESERVOIR", "El Vado Reservoir", "USBR_EVR", vars)); //no Data: 19548,no Data: 19626

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "2684")); vars.Add(new USBRVariable("Inflow", "2686")); vars.Add(new USBRVariable("Release", "2688")); vars.Add(new USBRVariable("Pool Elevation", "2685"));
        sites.Add(new USBRSite("ELEPHANT+BUTTE+RESERVOIR", "Elephant Butte Reservoir", "USBR_EBR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Electric Energy", "2266"));
        sites.Add(new USBRSite("FLAMING+GORGE+POWER+PLANT", "Flaming Gorge Power Plant", "USBR_FGPP", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1718")); vars.Add(new USBRVariable("Inflow", "1791")); vars.Add(new USBRVariable("Release", "1871")); vars.Add(new USBRVariable("Pool Elevation", "1927")); vars.Add(new USBRVariable("Power Release", "1861"));
        sites.Add(new USBRSite("FLAMING+GORGE+RESERVOIR", "Flaming Gorge Reservoir", "USBR_FGR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Electric Energy", "2267"));
        sites.Add(new USBRSite("FONTENELLE+POWER+PLANT", "Fontenelle Power Plant", "USBR_FOPP", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1717")); vars.Add(new USBRVariable("Inflow", "1790")); vars.Add(new USBRVariable("Release", "1870")); vars.Add(new USBRVariable("Pool Elevation", "1926")); vars.Add(new USBRVariable("Power Release", "1860"));
        sites.Add(new USBRSite("FONTENELLE+RESERVOIR", "Fontenelle Reservoir", "USBR_FOR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1753")); vars.Add(new USBRVariable("Inflow", "1824")); vars.Add(new USBRVariable("Release", "1904")); vars.Add(new USBRVariable("Pool Elevation", "1960"));
        sites.Add(new USBRSite("FRUITGROWERS+RESERVOIR%2C+COLORADO", "Fruitgrowers Reservoir, Colorado", "USBR_FRR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Electric Energy", "2305"));
        sites.Add(new USBRSite("GLEN+CANYON+DAM+POWER+PLANT", "Glen Canyon Dam Power Plant", "USBR_GCD", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Pool Elevation", "19175"));
        sites.Add(new USBRSite("HERON+RESERVOIR+NR+TIERRA+AMARILLA", "Heron Reservoir Nr Tierra Amarilla", "USBR_HER", vars));//name='19176' value='No_data'/> name='19609' value='No_data' />

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1754")); vars.Add(new USBRVariable("Inflow", "1825")); vars.Add(new USBRVariable("Release", "1905")); vars.Add(new USBRVariable("Pool Elevation", "1961"));
        sites.Add(new USBRSite("HUNTINGTON+NORTH+RESERVOIR", "Huntington North Reservoir", "USBR_HNR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1755")); vars.Add(new USBRVariable("Inflow", "1826")); vars.Add(new USBRVariable("Release", "1906")); vars.Add(new USBRVariable("Pool Elevation", "1962"));
        sites.Add(new USBRSite("HYRUM+RESERVOIR", "Hyrum Reservoir", "USBR_HYR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1733")); vars.Add(new USBRVariable("Inflow", "1804")); vars.Add(new USBRVariable("Release", "1884")); vars.Add(new USBRVariable("Pool Elevation", "1940"));
        sites.Add(new USBRSite("JACKSON+GULCH+RESERVOIR", "Jackson Gulch Reservoir", "USBR_JGR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Pool Elevation", "19570"));
        sites.Add(new USBRSite("JEMEZ+CANYON+RESERVOIR+NEAR+BERNALILLO", "Jemez Canyon Reservoir Near Bernalillo", "USBR_JCR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1730")); vars.Add(new USBRVariable("Inflow", "1801")); vars.Add(new USBRVariable("Release", "1881")); vars.Add(new USBRVariable("Pool Elevation", "1937"));
        sites.Add(new USBRSite("JOES+VALLEY+RESERVOIR", "Joes Valley Reservoir", "USBR_JVR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1762")); vars.Add(new USBRVariable("Inflow", "1833")); vars.Add(new USBRVariable("Release", "1913")); vars.Add(new USBRVariable("Pool Elevation", "1969"));
        sites.Add(new USBRSite("JORDANELLE+RESERVOIR", "Jordanelle Reservoir", "USBR_JOR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "14623")); vars.Add(new USBRVariable("Inflow", "14617")); vars.Add(new USBRVariable("Release", "14620")); vars.Add(new USBRVariable("Pool Elevation", "14621"));
        sites.Add(new USBRSite("LAKE+NIGHTHORSE%2C+", "Lake Nighthorse, ", "USBR_LNH", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1719")); vars.Add(new USBRVariable("Inflow", "1792")); vars.Add(new USBRVariable("Release", "1872")); vars.Add(new USBRVariable("Pool Elevation", "1928")); vars.Add(new USBRVariable("Power Release", "1862"));
        sites.Add(new USBRSite("LAKE+POWELL", "Lake Powell", "USBR_LPO", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Pool Elevation", "1948"));
        sites.Add(new USBRSite("LAKE+SUMNER+NR+FORT+SUMNER", "Lake Sumner Nr Fort Sumner", "USBR_LSU", vars));//<input type='hidden' name='1741' value='No_data' /><input type='hidden' name='1812' value='No_data' /><input type='hidden' name='1892' value='No_data' />

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1732")); vars.Add(new USBRVariable("Inflow", "1803")); vars.Add(new USBRVariable("Release", "1883")); vars.Add(new USBRVariable("Pool Elevation", "1939"));
        sites.Add(new USBRSite("LEMON+RESERVOIR", "Lemon Reservoir", "USBR_LER", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1740")); vars.Add(new USBRVariable("Inflow", "1811")); vars.Add(new USBRVariable("Release", "1891")); vars.Add(new USBRVariable("Pool Elevation", "1947"));
        sites.Add(new USBRSite("LOST+CREEK+RESERVOIR", "Lost Creek Reservoir", "USBR_LCR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1756")); vars.Add(new USBRVariable("Inflow", "1827")); vars.Add(new USBRVariable("Release", "1907")); vars.Add(new USBRVariable("Pool Elevation", "1963"));
        sites.Add(new USBRSite("MCPHEE+RESERVOIR", "Mcphee Reservoir", "USBR_MHR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1742")); vars.Add(new USBRVariable("Inflow", "1813")); vars.Add(new USBRVariable("Release", "1893")); vars.Add(new USBRVariable("Pool Elevation", "1949"));
        sites.Add(new USBRSite("MEEKS+CABIN+RESERVOIR", "Meeks Cabin Reservoir", "USBR_MCR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1728")); vars.Add(new USBRVariable("Inflow", "1799")); vars.Add(new USBRVariable("Release", "1879")); vars.Add(new USBRVariable("Pool Elevation", "1935"));
        sites.Add(new USBRSite("MOON+LAKE+RESERVOIR", "Moon Lake Reservoir", "USBR_MLR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1715")); vars.Add(new USBRVariable("Inflow", "1788")); vars.Add(new USBRVariable("Side Inflow", "1843")); vars.Add(new USBRVariable("Release", "1868")); vars.Add(new USBRVariable("Power Release", "1858")); vars.Add(new USBRVariable("Pool Elevation", "1924"));
        sites.Add(new USBRSite("MORROW+POINT+RESERVOIR", "Morrow Point Reservoir", "USBR_MPR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Electric Energy", "2268"));
        sites.Add(new USBRSite("MORROW+POINT+RESERVOIR+POWER+PLANT", "Morrow Point Reservoir Power Plant", "USBR_MPPP", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Pool Elevation", "19166"));
        sites.Add(new USBRSite("NAMBE+FALLS+RESERVOIR", "Nambe Falls Reservoir", "USBR_NFR", vars));//<input type='hidden' name='19167' value='No_data' />

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1720")); vars.Add(new USBRVariable("Inflow", "1793")); vars.Add(new USBRVariable("Release", "1873")); vars.Add(new USBRVariable("Pool Elevation", "1929"));
        sites.Add(new USBRSite("NAVAJO+RESERVOIR", "Navajo Reservoir", "USBR_NAR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1757")); vars.Add(new USBRVariable("Inflow", "1828")); vars.Add(new USBRVariable("Release", "1908")); vars.Add(new USBRVariable("Pool Elevation", "1964"));
        sites.Add(new USBRSite("NEWTON+RESERVOIR", "Newton Reservoir", "USBR_NER", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1743")); vars.Add(new USBRVariable("Inflow", "1814")); vars.Add(new USBRVariable("Release", "1894")); vars.Add(new USBRVariable("Pool Elevation", "1950"));
        sites.Add(new USBRSite("PAONIA+RESERVOIR%2C+COLORADO", "Paonia Reservoir, Colorado", "USBR_PAR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1744")); vars.Add(new USBRVariable("Inflow", "1815")); vars.Add(new USBRVariable("Release", "1895")); vars.Add(new USBRVariable("Pool Elevation", "1951"));
        sites.Add(new USBRSite("PINEVIEW+RESERVOIR", "Pineview Reservoir", "USBR_PVR", vars));//<input type='hidden' name='1864' value='No_data' />

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1758")); vars.Add(new USBRVariable("Inflow", "1829")); vars.Add(new USBRVariable("Release", "1909")); vars.Add(new USBRVariable("Pool Elevation", "1965"));
        sites.Add(new USBRSite("RED+FLEET+RESERVOIR", "Red Fleet Reservoir", "USBR_RFR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1746")); vars.Add(new USBRVariable("Inflow", "1817")); vars.Add(new USBRVariable("Release", "1897")); vars.Add(new USBRVariable("Pool Elevation", "1953"));
        sites.Add(new USBRSite("RIDGWAY+RESERVOIR", "Ridgway Reservoir", "USBR_RWR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1759")); vars.Add(new USBRVariable("Inflow", "1830")); vars.Add(new USBRVariable("Release", "1910")); vars.Add(new USBRVariable("Pool Elevation", "1966"));
        sites.Add(new USBRSite("RIFLE+GAP+RESERVOIR", "Rifle Gap Reservoir", "USBR_RGR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1745")); vars.Add(new USBRVariable("Inflow", "1816")); vars.Add(new USBRVariable("Release", "1896")); vars.Add(new USBRVariable("Pool Elevation", "1952"));
        sites.Add(new USBRSite("ROCKPORT+RESERVOIR", "Rockport Reservoir", "USBR_RPR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1729")); vars.Add(new USBRVariable("Inflow", "1800")); vars.Add(new USBRVariable("Release", "1880")); vars.Add(new USBRVariable("Pool Elevation", "1936"));
        sites.Add(new USBRSite("SCOFIELD+RESERVOIR", "Scofield Reservoir", "USBR_SCR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1737")); vars.Add(new USBRVariable("Inflow", "1808")); vars.Add(new USBRVariable("Release", "1888")); vars.Add(new USBRVariable("Pool Elevation", "1944"));
        sites.Add(new USBRSite("SILVER+JACK+RESERVOIR", "Silver Jack Reservoir", "USBR_SJR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1726")); vars.Add(new USBRVariable("Inflow", "1798")); vars.Add(new USBRVariable("Release", "1878")); vars.Add(new USBRVariable("Pool Elevation", "1934"));
        sites.Add(new USBRSite("STARVATION+RESERVOIR", "Starvation Reservoir", "USBR_STR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1747")); vars.Add(new USBRVariable("Inflow", "1818")); vars.Add(new USBRVariable("Release", "1898")); vars.Add(new USBRVariable("Pool Elevation", "1954"));
        sites.Add(new USBRSite("STATELINE+RESERVOIR", "Stateline Reservoir", "USBR_SLR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1725")); vars.Add(new USBRVariable("Inflow", "1797")); vars.Add(new USBRVariable("Release", "1877")); vars.Add(new USBRVariable("Pool Elevation", "1933"));
        sites.Add(new USBRSite("STEINAKER+RESERVOIR", "Steinaker Reservoir", "USBR_SNR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1760")); vars.Add(new USBRVariable("Inflow", "1831")); vars.Add(new USBRVariable("Release", "1911")); vars.Add(new USBRVariable("Pool Elevation", "1967"));
        sites.Add(new USBRSite("STRAWBERRY+RESERVOIR+%28EXPANDED%29", "Strawberry Reservoir (expanded)", "USBR_SBR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1713")); vars.Add(new USBRVariable("Inflow", "1786")); vars.Add(new USBRVariable("Release", "1866")); vars.Add(new USBRVariable("Pool Elevation", "1922"));
        sites.Add(new USBRSite("TAYLOR+PARK+RESERVOIR", "Taylor Park Reservoir", "USBR_TPR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1761")); vars.Add(new USBRVariable("Inflow", "1832")); vars.Add(new USBRVariable("Release", "1912")); vars.Add(new USBRVariable("Pool Elevation", "1968"));
        sites.Add(new USBRSite("UPPER+STILLWATER+RESERVOIR", "Upper Stillwater Reservoir", "USBR_USR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Storage", "1731")); vars.Add(new USBRVariable("Inflow", "1802")); vars.Add(new USBRVariable("Release", "1882")); vars.Add(new USBRVariable("Pool Elevation", "1938"));
        sites.Add(new USBRSite("VALLECITO+RESERVOIR", "Vallecito Reservoir", "USBR_VAR", vars));

        vars = new List <USBRVariable>(); vars.Add(new USBRVariable("Inflow", "1796")); vars.Add(new USBRVariable("Release", "1876")); vars.Add(new USBRVariable("Pool Elevation", "1932"));
        sites.Add(new USBRSite("WILLARD+BAY+RESERVOIR", "Willard Bay Reservoir", "USBR_WBR", vars));
        return sites;

    }
}
public class USBRVariable
{
    public string VariableName { get; set; }
    public string VariableCode { get; set; }
    public USBRVariable(string name, string code)
    {
        this.VariableName = name;
        this.VariableCode = code;

    }

}
public class USBRSite
{
    public string SiteName { get; set; }
    public string URLName { get; set; }
    public string SiteCode { get; set; }
    public List <USBRVariable> variables { get; set; }
    public USBRSite(string URL, string Site, string sitecode, List <USBRVariable> vars)
    {
        this.SiteName = Site;
        this.URLName = URL;
        this.variables = vars;
        this.SiteCode = sitecode;
    }

}
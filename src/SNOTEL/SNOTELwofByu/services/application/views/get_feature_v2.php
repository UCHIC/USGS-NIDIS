<wfs:FeatureCollection xmlns:tows='<?php echo base_url(); ?>'
 xmlns:cite="http://www.opengeospatial.net/cite" xmlns:wfs="http://www.opengis.net/wfs/2.0" xmlns:gml="http://www.opengis.net/gml/3.2"
 xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" numberMatched='<?php echo count($features);?>' numberReturned='<?php echo count($features);?>' timeStamp='<?php echo date('Y-m-d H:i:s T');?>' xsi:schemaLocation="
<?php echo base_url(); ?>
 <?php echo base_url(); ?>index.php/wfs/write_xml?VariableID=8&amp;service=WFS&amp;version=2.0.0&amp;request=DescribeFeatureType&amp;Typename=hydro_site
http://www.opengis.net/gml/3.2 http://schemas.opengis.net/gml/3.2.1/gml.xsd http://www.opengis.net/wfs/2.0 http://schemas.opengis.net/wfs/2.0/wfs.xsd">

<wfs:boundedBy>
<?php
echo "<gml:Envelope srsName=\"urn:ogc:def:crs:EPSG::4326\">\n";
	echo "<gml:LowerCorner>$bounding->ymin $bounding->xmin</gml:LowerCorner>\n";
	echo "<gml:UpperCorner>$bounding->ymax $bounding->xmax</gml:UpperCorner>\n";	
echo "</gml:Envelope>\n";	
?>
</wfs:boundedBy>


<?php 
$i=1;
foreach( $features as $feat ) {
?>
	<wfs:member>
	    <tows:hydro_site gml:id="hydro_site.<?php echo $i;?>">
            <gml:boundedBy>
                <gml:Envelope srsDimension="2" srsName="urn:ogc:def:crs:EPSG::4326">
                    <gml:lowerCorner><?php echo $feat->Latitude; ?> <?php echo $feat->Longitude; ?></gml:lowerCorner>
                    <gml:upperCorner><?php echo $feat->Latitude; ?> <?php echo $feat->Longitude; ?></gml:upperCorner>
                </gml:Envelope>
            </gml:boundedBy>
            <tows:site_id><?php echo $feat->SiteID; ?></tows:site_id>
			<tows:name><?php echo $feat->SiteName; ?></tows:name>
			<tows:lon><?php echo $feat->Longitude; ?></tows:lon>
			<tows:lat><?php echo $feat->Latitude; ?></tows:lat>
			<tows:waterml2url><?php echo base_url().'index.php/cuahsi/GetValues?version=2.0&amp;location=networkCode:'.$feat->SiteCode.'&amp;variable=networkCode:'.$feat->VariableCode; ?></tows:waterml2url>
            <tows:watermlurl><?php echo base_url().'index.php/cuahsi/GetValues?version=1.0&amp;location=networkCode:'.$feat->SiteCode.'&amp;variable=networkCode:'.$feat->VariableCode; ?></tows:watermlurl>
			<tows:graphurl><?php echo ''; ?></tows:graphurl>
			<tows:downloadurl><?php echo ''; ?></tows:downloadurl>
			<tows:begindate><?php echo $feat->BeginDateTimeUTC; ?></tows:begindate>
			<tows:enddate><?php echo $feat->EndDateTimeUTC; ?></tows:enddate>
			<tows:descriptor><?php echo $feat->SiteName; ?></tows:descriptor>
			<tows:source><?php echo $feat->SourceDescription; ?></tows:source>
            <tows:geom>
       			<gml:Point srsDimension="2" srsName="urn:ogc:def:crs:EPSG::4326">
        			<gml:pos><?php echo $feat->Latitude; ?> <?php echo $feat->Longitude; ?></gml:pos>
       			</gml:Point>
        </tows:geom>
		
	</tows:hydro_site>
	</wfs:member>
<?php
$i+=1;}
?>



</wfs:FeatureCollection>
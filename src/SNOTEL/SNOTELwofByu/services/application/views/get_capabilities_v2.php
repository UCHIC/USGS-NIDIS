<WFS_Capabilities
version="2.0.0"
xmlns="http://www.opengis.net/wfs/2.0"
xmlns:gml="http://www.opengis.net/gml/3.2"
xmlns:fes="http://www.opengis.net/fes/2.0"
xmlns:xlink="http://www.w3.org/1999/xlink"
xmlns:ows="http://www.opengis.net/ows/1.1"
xmlns:xsd="http://www.w3.org/2001/XMLSchema"
xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
xsi:schemaLocation="http://www.opengis.net/wfs/2.0
http://schemas.opengis.net/wfs/2.0.0/wfs.xsd
http://www.opengis.net/ows/1.1
http://schemas.opengis.net/ows/1.1.0/owsAll.xsd">
   
	<ows:ServiceIdentification>
        <ows:Title>HydroServerLite WFS</ows:Title>
        <ows:Abstract>Testing WFS Service Version 2.0.0</ows:Abstract>
        <ows:Keywords>
        <ows:Keyword>WFS</ows:Keyword>
        <ows:Keyword>HydroServerLite</ows:Keyword>
        <ows:Keyword>Hydroserver</ows:Keyword>
        <ows:Keyword></ows:Keyword>
        <ows:Type>String</ows:Type>
        </ows:Keywords>
        <ows:ServiceType>WFS</ows:ServiceType>
        <ows:ServiceTypeVersion>2.0.0</ows:ServiceTypeVersion>
        <ows:ServiceTypeVersion>1.0.0</ows:ServiceTypeVersion>
        <ows:Fees>NONE</ows:Fees>
        <ows:AccessConstraints>NONE</ows:AccessConstraints>
    </ows:ServiceIdentification>
    
    <ows:ServiceProvider>
        <ows:ProviderName>To Enter</ows:ProviderName>
        <ows:ProviderSite xlink:href="To Enter"/>
        <ows:ServiceContact>
            <ows:IndividualName>To Enter</ows:IndividualName>
            <ows:PositionName>To Enter</ows:PositionName>
                <ows:ContactInfo>
                <ows:Phone>
                    <ows:Voice>To Enter</ows:Voice>
                    <ows:Facsimile>To Enter</ows:Facsimile>
                </ows:Phone>
                <ows:Address>
                    <ows:DeliveryPoint>To Enter</ows:DeliveryPoint>
                    <ows:City>To Enter</ows:City>
                    <ows:AdministrativeArea>To Enter</ows:AdministrativeArea>
                    <ows:PostalCode>To Enter</ows:PostalCode>
                    <ows:Country>To Enter</ows:Country>
                    <ows:ElectronicMailAddress>To Enter</ows:ElectronicMailAddress>
                </ows:Address>
                <ows:OnlineResource xlink:href="<?php echo base_url('wfs/write_xml'); ?>"/>
                <ows:HoursOfService>24x7</ows:HoursOfService>
                    <ows:ContactInstructions>
                    To Enter
                    </ows:ContactInstructions>
                </ows:ContactInfo>
            <ows:Role>To Enter</ows:Role>
        </ows:ServiceContact>
    </ows:ServiceProvider>
    
    <ows:OperationsMetadata>
    <?php
		//PHP code here to print out operations available for this WFS Service
		
		$list_of_operations = array("GetCapabilities","DescribeFeatureType","ListStoredQueries","DescribeStoresQueries","GetFeature");
		
		foreach ($list_of_operations as $operation):
			echo'<ows:Operation name="'.$operation.'">';
				echo'<ows:DCP>';
					echo'<ows:HTTP>';
					echo'<ows:Get xlink:href="'.base_url('index.php/wfs/write_xml?').'"/>';
					echo'<ows:Post xlink:href="'.base_url('index.php/wfs/write_xml').'"/>';
					echo'</ows:HTTP>';
				echo'</ows:DCP>';
				echo'<ows:Parameter name="AcceptVersions">';
					echo'<ows:AllowedValues>';
						echo'<ows:Value>2.0.0</ows:Value>';
						echo'<ows:Value>1.0.0</ows:Value>';
					echo'</ows:AllowedValues>';
				echo'</ows:Parameter>';
			echo'</ows:Operation>';
		endforeach;
	
	?>
    <ows:Constraint name="ImplementsBasicWFS">
		<ows:NoValues/>
		<ows:DefaultValue>TRUE</ows:DefaultValue>
	</ows:Constraint>
    <ows:Constraint name="KVPEncoding">
		<ows:NoValues/>
		<ows:DefaultValue>TRUE</ows:DefaultValue>
	</ows:Constraint>
    <ows:Constraint name="XMLEncoding">
		<ows:NoValues/>
		<ows:DefaultValue>TRUE</ows:DefaultValue>
	</ows:Constraint>
    
    </ows:OperationsMetadata>
 
	<FeatureTypeList>
		
    <?php
    
	//To create a new feature layer for each variable. 
	
	foreach ($variables as $variable):
		echo "<FeatureType>\n";
			echo "<Name>".$variable->VariableName."_$variable->VariableID"."</Name>";
			echo "<Title>Variable Code:$variable->VariableCode</Title>\n";
			echo "<Abstract>Variable Data Type: $variable->DataType</Abstract>\n";
			echo "<ows:Keywords>\n";
				echo "<ows:Keyword>$variable->VariableName</ows:Keyword>\n";
				echo "<ows:Keyword>$variable->SampleMedium</ows:Keyword>\n"	;
			echo "</ows:Keywords>\n";	
			echo "<DefaultCRS>urn:ogc:def:crs:EPSG:4326</DefaultCRS>\n";	
			echo "<ows:WGS84BoundingBox>\n";
				echo "<ows:LowerCorner>$variable->xmin $variable->ymin</ows:LowerCorner>\n";
				echo "<ows:UpperCorner>$variable->xmax $variable->ymax</ows:UpperCorner>\n";	
			echo "</ows:WGS84BoundingBox>\n";				
		echo "</FeatureType>\n";
	endforeach;
	
    ?>
	</FeatureTypeList>
</WFS_Capabilities>

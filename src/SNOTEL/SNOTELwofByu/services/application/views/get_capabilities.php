<WFS_Capabilities version="1.0.0"
xmlns="http://www.opengis.net/wfs"
xmlns:ogc="http://www.opengis.net/ogc"
xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
xsi:schemaLocation="http://www.opengis.net/wfs http://schemas.opengis.net/wfs/1.0.0/wfs.xsd">
   <Service>
		<Name>WebFeatureServer</Name>
		<Title>HydroServerLite WFS</Title>
		<Abstract>Test wfs 1.0.0</Abstract>
		<Keywords>WFS</Keywords>
		<OnlineResource><?php echo base_url('wfs/write_xml'); ?></OnlineResource>
		<Fees>NONE</Fees>
		<AccessConstraints>NONE</AccessConstraints>
   </Service>
   <Capability>
      <Request>
         <GetCapabilities>
            <DCPType>
               <HTTP>
                  <Get onlineResource="<?php echo base_url('wfs/write_xml?'); ?>"/>
               </HTTP>
            </DCPType>
            <DCPType>
				<HTTP>
				  <Post onlineResource="<?php echo base_url('wfs/write_xml'); ?>"/>
				</HTTP>
            </DCPType>
         </GetCapabilities>
         <DescribeFeatureType>
            <SchemaDescriptionLanguage>
               <XMLSCHEMA/>
            </SchemaDescriptionLanguage>
            <DCPType>
               <HTTP>
                  <Get onlineResource="<?php echo base_url('index.php/wfs/write_xml?'); ?>"/>
               </HTTP>
            </DCPType>
            <DCPType>
			   <HTTP>
				  <Post onlineResource="<?php echo base_url('index.php/wfs/write_xml'); ?>"/>
			   </HTTP>
            </DCPType>
         </DescribeFeatureType>
         <GetFeature>
            <ResultFormat>
               <GML2/>
            </ResultFormat>
            <DCPType>
               <HTTP>
                  <Get onlineResource="<?php echo base_url('index.php/wfs/write_xml?'); ?>"/>
               </HTTP>
            </DCPType>
            <DCPType>
			   <HTTP>
				  <Post onlineResource="<?php echo base_url('index.php/wfs/write_xml'); ?>"/>
			   </HTTP>
            </DCPType>
         </GetFeature>
         <Transaction>
            <DCPType>
               <HTTP>
                  <Get onlineResource="<?php echo base_url('index.php/wfs/write_xml?'); ?>"/>
               </HTTP>
            </DCPType>
            <DCPType>
			   <HTTP>
				  <Post onlineResource="<?php echo base_url('index.php/wfs/write_xml'); ?>"/>
			   </HTTP>
            </DCPType>
         </Transaction>
      </Request>
      <VendorSpecificCapabilities>
      </VendorSpecificCapabilities>
   </Capability>

	<FeatureTypeList>
		<Operations>
			<Query/>
			<Insert/>
			<Update/>
			<Delete/>
		</Operations>
      <?php
	
	foreach ($variables as $variable):
		echo "<FeatureType>\n";
			echo "<Name>hydroserver_$variable->VariableID"."</Name>";
			echo "<Title>Variable Code:$variable->VariableCode</Title>\n";
			echo "<Abstract>Variable Data Type: $variable->DataType</Abstract>\n";
			echo "<SRS>EPSG:4326</SRS>\n";
			echo "<Keywords>" . $variable->SampleMedium . "</Keywords>\n";
			echo "<LatLongBoundingBox minx='$variable->xmin' miny='$variable->ymin' maxx='$variable->xmax' maxy='$variable->ymax' ></LatLongBoundingBox>\n";	
		echo "</FeatureType>\n";
	endforeach;
	
    ?>
    
	</FeatureTypeList>
</WFS_Capabilities>

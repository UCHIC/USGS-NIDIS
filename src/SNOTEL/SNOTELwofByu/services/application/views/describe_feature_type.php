<xs:schema targetNamespace='<?php echo base_url(); ?>' 
		   xmlns:tows='<?php echo base_url(); ?>' 
		   xmlns:ogc='http://www.opengis.net/ogc' 
		   xmlns:xs='http://www.w3.org/2001/XMLSchema' 
		   xmlns='http://www.w3.org/2001/XMLSchema' 
		   xmlns:gml='http://www.opengis.net/gml' elementFormDefault='qualified' version='1.0'>
	<xs:import namespace='http://www.opengis.net/gml' schemaLocation='http://schemas.opengis.net/gml/2.1.2/feature.xsd'/>

	
	<xs:complexType name='hydro_siteType'>
		<xs:complexContent>
			<xs:extension base='gml:AbstractFeatureType'>
				<xs:sequence>
					<xs:element name ='site_id' type='int' nillable='false' minOccurs='1' maxOccurs='1'/>
					<xs:element name ='name' nillable='true' minOccurs='0' maxOccurs='1'>
						<xs:simpleType>
							<xs:restriction base='string'><xs:maxLength value='80'/></xs:restriction>
						</xs:simpleType>
					</xs:element>    
					<xs:element name ='lon' type='double' nillable='true' minOccurs='0' maxOccurs='1'/>
					<xs:element name ='lat' type='double' nillable='true' minOccurs='0' maxOccurs='1'/>
					<xs:element name ='waterml2url' nillable='false' minOccurs='1' maxOccurs='1'/>
                    <xs:element name ='watermlurl' nillable='false' minOccurs='1' maxOccurs='1'/>
					<xs:element name ='graphurl' nillable='false' minOccurs='0' maxOccurs='1'/>
					<xs:element name ='downloadurl' nillable='false' minOccurs='0' maxOccurs='1'/>
					<xs:element name ='begindate' nillable='false' minOccurs='1' maxOccurs='1'/>
					<xs:element name ='enddate' nillable='false' minOccurs='0' maxOccurs='1'/>
					<xs:element name ='descriptor' nillable='false' minOccurs='0' maxOccurs='1'/>
					<xs:element name ='source' nillable='false' minOccurs='0' maxOccurs='1'/>
					<xs:element name ='geom' type='gml:PointPropertyType' nillable='true' minOccurs='0' maxOccurs='1'/>
				</xs:sequence>
			</xs:extension>
		</xs:complexContent>
	</xs:complexType>
	<xs:element name='hydro_site' type='tows:hydro_siteType' substitutionGroup='gml:_Feature' />
</xs:schema>
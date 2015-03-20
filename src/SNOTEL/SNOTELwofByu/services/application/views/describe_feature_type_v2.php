<xsd:schema 
xmlns:cite="http://www.opengeospatial.net/cite" 
xmlns:gml="http://www.opengis.net/gml/3.2" 
xmlns:tows='<?php echo base_url(); ?>'
xmlns:wfs="http://www.opengis.net/wfs/2.0" 
xmlns:xsd="http://www.w3.org/2001/XMLSchema" 
elementFormDefault="qualified" 
targetNamespace='<?php echo base_url(); ?>'
>

<xsd:import namespace='http://www.opengis.net/gml'
schemaLocation='http://schemas.opengis.net/gml/3.2.1/gml.xsd'/>

<xsd:complexType name="hydro_siteType">
    <xsd:complexContent>
        <xsd:extension base="gml:AbstractFeatureType">
            <xsd:sequence>
                <xsd:element name ='site_id' type='int' nillable='false' minOccurs='1' maxOccurs='1'/>
                <xsd:element name ='name' nillable='true' minOccurs='0' maxOccurs='1'>
                    <xsd:simpleType>
                        <xsd:restriction base='string'><xsd:maxLength value='80'/></xsd:restriction>
                    </xsd:simpleType>
                </xsd:element>    
                <xsd:element name ='lon' type='double' nillable='true' minOccurs='0' maxOccurs='1'/>
                <xsd:element name ='lat' type='double' nillable='true' minOccurs='0' maxOccurs='1'/>
                <xsd:element name ='waterml2url' nillable='false' minOccurs='1' maxOccurs='1'/>
                <xsd:element name ='watermlurl' nillable='false' minOccurs='1' maxOccurs='1'/>
                <xsd:element name ='graphurl' nillable='false' minOccurs='0' maxOccurs='1'/>
                <xsd:element name ='downloadurl' nillable='false' minOccurs='0' maxOccurs='1'/>
                <xsd:element name ='begindate' nillable='false' minOccurs='1' maxOccurs='1'/>
                <xsd:element name ='enddate' nillable='false' minOccurs='0' maxOccurs='1'/>
                <xsd:element name ='descriptor' nillable='false' minOccurs='0' maxOccurs='1'/>
                <xsd:element name ='source' nillable='false' minOccurs='0' maxOccurs='1'/>
                <xsd:element name ='geom' type='gml:PointPropertyType' nillable='true' minOccurs='0' maxOccurs='1'/>
            </xsd:sequence>
        </xsd:extension>
    </xsd:complexContent>
</xsd:complexType>

<xsd:element name='hydro_site' type='tows:hydro_siteType' substitutionGroup='gml:AbstractFeature'/>

</xsd:schema>
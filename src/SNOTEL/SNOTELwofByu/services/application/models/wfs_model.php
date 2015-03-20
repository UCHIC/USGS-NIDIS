<?php

class Wfs_Model extends CI_Model
{
	public function get_feature_SQ( $siteID )
	{
		$this->db->select('sites.SiteID, sites.SiteName, sites.SiteCode, VariableCode,VariableName, Latitude, Longitude, BeginDateTimeUTC, EndDateTimeUTC, SourceDescription'); 
		$this->db->where('seriescatalog.SiteCode', $siteID);
		$this->db->from('seriescatalog');
		$this->db->join('sites', 'sites.SiteID = seriescatalog.SiteID');

		$result = $this->db->get();
		return $result->result();		
	}
	
	public function get_features( $variableID )
	{
		$this->db->select('sites.SiteID, sites.SiteName, sites.SiteCode, VariableCode,VariableName, Latitude, Longitude, BeginDateTimeUTC, EndDateTimeUTC, SourceDescription'); 
		$this->db->where('seriescatalog.VariableID', $variableID);
		$this->db->from('seriescatalog');
		$this->db->join('sites', 'sites.SiteID = seriescatalog.SiteID');

		$result = $this->db->get();
		return $result->result();		
	}
	
	public function get_features_all ()
	
	{
		
		//Subquery procedure as just using a simple join returns only one row
		//Also made changes to db_active_rec to use the protected functions compile and reset. 
		//May not be the best method, but this hack gets it working
		
		//Also min is reduced and Max is added by 0.001 to make a slight difference else the extents are not recognised where only one site exists.  
		
		$this->db
        ->select(' longitude , latitude , SiteID')
        ->from('sites') ; 

		$subquery = $this->db->_compile_select();

		$this->db->_reset_select(); 

		$query  =   $this->db
					
                    ->select('VariableID, VariableName, DataType, VariableCode,VariableunitsName, SampleMedium, (MIN( longitude )-0.001) AS xmin, (MAX( longitude )+0.001) AS xmax, (MIN( latitude )-0.001) AS ymin, (MAX( latitude )+0.001) AS ymax')
                    ->from('seriescatalog')
					->group_by('VariableID')
					->where('VariableID !=', 'NULL')
                    ->join("($subquery)  sitess","seriescatalog.SiteID = sitess.SiteID")
                    ->get();
					
		return $query->result();		
			
	}
	
	public function get_watermlurl($feat)
	{
			return htmlspecialchars(base_url() . 'services/' . 'cuahsi_1_1.asmx/GetValuesObject?location=' . $this->config->item('service_code') . ':' . trim($feat->SiteCode) . '&variable=' . $this->config->item('service_code') . ':' . trim($feat->VariableCode));
	}
	
	public function get_sites()
	{
		//$this->db->limit(1);
		$this->db->order_by('SiteID', 'Asc');
		$result = $this->db->get('sites');
		return $result->result();
	}
	
	public function get_boundingbox($variableID)
	{
		$this->db->select('VariableID, VariableName, SampleMedium, (MIN( longitude )-0.001) AS xmin, (MAX( longitude )+0.001) AS xmax, (MIN( latitude )-0.001) AS ymin, (MAX( latitude )+0.001) AS ymax');
		$this->db->where('seriescatalog.VariableID', $variableID);
		$this->db->from('sites');
		$this->db->join('seriescatalog', 'seriescatalog.SiteID = sites.SiteID');

		$result = $this->db->get();
		return $result->row();		
		
		//$this->db->order_by('SiteID', 'Asc');
		//$result = $this->db->get('sites');
		//return $result->result();
	}
	
	public function get_boundingbox_SQ($siteID)
	{
		$this->db->select('VariableID, VariableName, SampleMedium, (MIN( longitude )-0.001) AS xmin, (MAX( longitude )+0.001) AS xmax, (MIN( latitude )-0.001) AS ymin, (MAX( latitude )+0.001) AS ymax');
		$this->db->where('seriescatalog.SiteCode', $siteID);
		$this->db->from('sites');
		$this->db->join('seriescatalog', 'seriescatalog.SiteID = sites.SiteID');

		$result = $this->db->get();
		return $result->row();		
	
	}
	
	public function get_variables()
	{
		$this->db->order_by('VariableID', 'Asc');
		$result = $this->db->get('variables');
		return $result->result();
	}
	
	public function check_features( $features )
	{
		if( $features->BeginDateTimeUTC == '1950-01-01 00:00:00')
		{
			$features->BeginDateTimeUTC = '';
		}
		
		if( $features->EndDateTimeUTC == '1952-08-01 00:00:00' )
		{
			$features->EndDateTimeUTC = '';
		}
		return $features;
	}
	
	// Methods for Server response/requests delivery
	function makeExceptionReport($value)
	{
		ob_get_clean();
		ob_start();

		echo '<ServiceExceptionReport
		version="1.2.0"
		xmlns="http://www.opengis.net/ogc"
		xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
		xsi:schemaLocation="http://www.opengis.net/ogc http://wfs.plansystem.dk:80/geoserver/schemas//wfs/1.0.0/OGC-exception.xsd">
		<ServiceException>';
			 if (is_array($value)) {
				 print_r($value);
			 } else {
				 print $value;
			 }
			 echo '</ServiceException>
			</ServiceExceptionReport>';
			 $data = ob_get_clean();
			 echo $data;
			 logfile::write($data);
			 die();
	}
	
	function altUseCdataOnStrings($value) {
		if (!is_numeric($value) && ($value)) {
			//$value = "<![CDATA[".$value."]]>";
			$value = str_replace("&","&#38;",$value);
			$result = $value;
		}
		else {
			$result = $value;
		}
		return $result;
	}
	
	function drop_namespace($tag) {
		//$tag = html_entity_decode($tag);
		//$tag = gmlConverter::oneLineXML($tag);
		$tag = preg_replace('/ xmlns(?:.*?)?=\".*?\"/',"",$tag); // Remove xmlns with "
		$tag = preg_replace('/ xmlns(?:.*?)?=\'.*?\'/',"",$tag); // Remove xmlns with '
		$tag = preg_replace('/ xsi(?:.*?)?=\".*?\"/',"",$tag); // remove xsi:schemaLocation with "
		$tag = preg_replace('/ xsi(?:.*?)?=\'.*?\'/',"",$tag); // remove xsi:schemaLocation with '
		$tag = preg_replace('/ cs(?:.*?)?=\".*?\"/',"",$tag);  //
		$tag = preg_replace('/ cs(?:.*?)?=\'.*?\'/',"",$tag);
		$tag = preg_replace('/ ts(?:.*?)?=\".*?\"/',"",$tag);
		$tag = preg_replace('/ decimal(?:.*?)?=\".*?\"/',"",$tag);
		$tag = preg_replace('/ decimal(?:.*?)?=\'.*?\'/',"",$tag);
		$tag = preg_replace("/[\w-]*:(?![\w-]*:)/", "", $tag);// remove any namespaces
		return ($tag);
	}
	
	function drop_all_namespaces($tag) {

		$tag = preg_replace("/[\w-]*:/", "", $tag);// remove any namespaces
		return ($tag);
	}
	
	function drop_last_chrs($str, $no) {
		$strLen=strlen($str);
		return substr($str, 0, ($strLen)-$no);
	}

	function drop_first_chrs($str, $no) {
		$strLen=strlen($str);
		return substr($str, $no, $strLen);
	}
	
	function genBBox($XMin, $YMin, $XMax, $YMax) {
		global $depth;
		global $tables;
		global $db;
		global $srs;

		writeTag("open", "gml", "boundedBy", null, True, True);
		$depth++;
		writeTag("open", "gml", "Box", array("srsName"=>"EPSG:".$srs), True, True);
		$depth++;
		writeTag("open", "gml", "coordinates", array("decimal"=>".", "cs"=>",", "ts"=>" "), True, False);
		print $XMin.",".$YMin." ".$XMax.",".$YMax;
		writeTag("close", "gml", "coordinates", null, False, True);
		$depth--;
		writeTag("close", "gml", "Box", null, True, True);
		$depth--;
		writeTag("close", "gml", "boundedBy", null, True, True);
	}
}
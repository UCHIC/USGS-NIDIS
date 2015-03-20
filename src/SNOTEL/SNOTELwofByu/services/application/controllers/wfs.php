<?php

class Wfs extends CI_Controller
{
	public function __construct()
	{
		parent::__construct();
		$this->load->model('wfs_model');
	}
	
	function write_xml()
	{
		// TO DO: check to see the conditions of the different attribute
		// what to display and when to display
		header('Content-Type:text/xml; charset=UTF-8', TRUE);
	
		if( $this->input->get('version') == NULL && $this->input->post('version') == NULL && $this->input->get('VERSION') == NULL && $this->input->post('VERSION') == NULL)
		{
			//No version found
			$version = "2.0.0";
		}
		else
		{
		
			if ($this->input->get('version'))
				$version=$this->input->get('version');
			if ($this->input->post('version'))
				$version=$this->input->post('version');
			if ($this->input->get('VERSION'))
				$version=$this->input->get('VERSION');
			if ($this->input->post('VERSION'))
				$version=$this->input->post('VERSION');

		}
		
		//Get The Request
		
			if( $this->input->get('request') == NULL && $this->input->post('request') == NULL && $this->input->get('REQUEST') == NULL && $this->input->post('REQUEST') == NULL)
		{
			//No request found
			header('Content-Type:text/xml; charset=UTF-8', TRUE);
			$this->load->view('request_error');
			return;
		}
		else
		{
		
			if ($this->input->get('request'))
				$request=$this->input->get('request');
			if ($this->input->post('request'))
				$request=$this->input->post('request');
			if ($this->input->get('REQUEST'))
				$request=$this->input->get('REQUEST');
			if ($this->input->post('REQUEST'))
				$request=$this->input->post('REQUEST');

		}
	
		//Get stored Query if exists and verify if this query has been defined. 
		
		$definedQuery = "urn:ogc:def:query:OGC-WFS::GetFeatureById";
		
			
		//Get The Request
		
			if( $this->input->get('STOREDQUERY_ID') == NULL && $this->input->post('STOREDQUERY_ID') == NULL)
		{
			//No stored query id found
			$queryID = NULL;
		}
		else
		{
		
			if ($this->input->get('STOREDQUERY_ID'))
				$queryID=$this->input->get('STOREDQUERY_ID');
			if ($this->input->post('STOREDQUERY_ID'))
				$queryID=$this->input->post('STOREDQUERY_ID');
			
			if($queryID != $definedQuery):
				$this->load->view('get_feature_error'); return;
			endif;
			
			$id=NULL;
			
			if ($this->input->get('ID'))
				$id=$this->input->get('ID');
			if ($this->input->post('ID'))
				$id=$this->input->post('ID');
				
			if ($id==NULL):
				$this->load->view('get_feature_error'); return;
			endif;
			
		}
		
		
	
		switch($request):
		
		//Query for stored
		case 'GETFEATURES':
		case 'GETFEATURE':
		case 'GetFeatures':
		case 'GetFeature':
			//Get TypeNames from the URL, if no type name specified load the error view: 
			
			if( $this->input->get('typeNames') == NULL && $this->input->post('typeNames') == NULL && $this->input->get('TYPENAME') == NULL && $this->input->post('TYPENAME') == NULL)
			{
				//No typename Found
				if (!($queryID && $id!=NULL)):
				$this->load->view('get_feature_error'); return;
				endif;
				$typeNames  = "dumb_1";
			}
			else
			{
			
				if ($this->input->get('typeNames'))
					$typeNames=$this->input->get('typeNames');
				if ($this->input->post('typeNames'))
					$typeNames=$this->input->post('typeNames');
				if ($this->input->get('TYPENAME'))
					$typeNames=$this->input->get('TYPENAME');
				if ($this->input->post('TYPENAME'))
					$typeNames=$this->input->post('TYPENAME');

			}
			
			//Version can be entered as two different parameters , determining the version here...if not specified it will be set to version 2.0.0
			
			//Assuming here that only one typename is being passed. Getting the variable code outofit

		  $parts=explode("_",$typeNames);
		  
		  $variableID =1; 

		  if (count($parts)>1)
		  {
			  $variableID=$parts[count($parts)-1];
		  }
			
			$data['variableID'] = $variableID;
			
			//If query defined and Id is not null
			
			if ($queryID && $id!=NULL):
			
			//We are directly fetching the site. We don't need the variable id. 
			
			$data['features'] = array_slice($this->wfs_model->get_feature_SQ( $id ), 0, 1);
			$data['bounding'] = 	$this->wfs_model->get_boundingbox_SQ( $id );
			
			else:
			
				$data['features'] = $this->wfs_model->get_features( $variableID );
				$data['bounding'] = $this->wfs_model->get_boundingbox( $variableID );
			
			endif;
			
			switch($version):
					case '1.0.0' : $this->load->view('get_feature', $data); break;
					case '2.0.0' : $this->load->view('get_feature_v2', $data); break;
					default : //Defaulting to version 2.0.0
						 $this->load->view('get_feature_v2', $data); break;
				endswitch;
		
		
		break;
		case 'DescribeFeatureType':
	
			header('Content-Type:text/xml; charset=UTF-8', TRUE);
			
			switch($version):
				case '1.0.0' : $this->load->view('describe_feature_type'); break;
				case '2.0.0' : $this->load->view('describe_feature_type_v2'); break;
				default : //Defaulting to version 2.0.0
					 $this->load->view('describe_feature_type_v2'); break;
			endswitch;

		break;
		case 'GetCapabilities':
		
			header('Content-Type:text/xml; charset=UTF-8', TRUE);
			$data['variables']  = $this->wfs_model->get_features_all();		
				
			//Adding Version Control Here. To enable support for Version 2.0.0 since as of now it doesnt matter which version is being used. 
			switch($version):
				case '1.0.0' : $this->load->view('get_capabilities', $data); break;
				case '2.0.0' : $this->load->view('get_capabilities_v2', $data); break;
				default : //Defaulting to version 2.0.0
					 $this->load->view('get_capabilities_v2', $data); break;
			endswitch;

		break;
		case 'DescribeStoredQueries':
		
			header('Content-Type:text/xml; charset=UTF-8', TRUE);
			//This feature only exists in version 2.0.0, so no support for version 1.0.0
			$this->load->view('dec_stored_queries'); 
			
		break;
		
		case 'ListStoredQueries':
		
			header('Content-Type:text/xml; charset=UTF-8', TRUE);
			//This feature only exists in version 2.0.0, so no support for version 1.0.0
			$this->load->view('list_stored_queries'); 
		break;

		default:
			header('Content-Type:text/xml; charset=UTF-8', TRUE);
			$this->load->view('request_error');
		endswitch;
		
	}
	
	function variables()
	{
		// show the variables from the Variables table in DB
		// based on them do the listing of the sites and seriescatalog on REQUESTS
		$data['variables'] = $this->wfs_model->get_variables();
		//print_r($variables);
		$this->load->view('variables', $data);
	}
	
	function wfs_server()
	{

		// Manually tested this piece of code in order to check the requests.
		// Code based on a similar idea but build in Java
		
		header('Content-Type:text/xml; charset=UTF-8', TRUE);
		header('Connection:close', TRUE);


		//$userFromUri = "mhoegh"; // for testing

		logfile::write($userFromUri."\n\n");

		
		// We connect to the users db
		$postgisdb = $userFromUri;
		$srs=$srsFromUri;
		$postgisschema = $schemaFromUri;



		$postgisObject = new postgis();
		//$user = new users($userFromUri);
		//$version = new version($user);

		$geometryColumnsObj = new GeometryColumns();

		function microtime_float()
		{
			list($utime, $time) = explode(" ", microtime());
			return ((float)$utime + (float)$time);
		}
		$startTime = microtime_float();

		//ini_set("display_errors", "On");

		$thePath= "http://".$_SERVER['SERVER_NAME'].$_SERVER['REDIRECT_URL'];
		//$thePath= "http://".$_SERVER['SERVER_NAME'].$_SERVER['PHP_SELF'];
		$server="http://".$_SERVER['SERVER_NAME'];
		$BBox=null;
		//end added
		$currentTable=null;
		$currentTag=null;
		$gen=array();
		$gen[0]="";
		$level=0;
		$depth=0;
		$tables=array();
		$fields=array();
		$wheres=array();
		$limits=array();

		logfile::write("\nRequest\n\n");
		logfile::write($HTTP_RAW_POST_DATA."\n\n");

		$unserializer_options = array (
			'parseAttributes' => TRUE,
			'typeHints' => FALSE
		);
		$unserializer = new XML_Unserializer($unserializer_options);

		/*$HTTP_RAW_POST_DATA = '<?xml version="1.0" encoding="utf-8"?><wfs:Transaction service="WFS" version="1.0.0" xmlns="http://www.opengis.net/wfs" xmlns:mrhg="http://twitter/mrhg" xmlns:ogc="http://www.opengis.net/ogc" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><wfs:Insert idgen="GenerateNew"><mrhg:hej><the_geom><gml:MultiPolygon srsName="urn:x-ogc:def:crs:EPSG:6.9:4326"><gml:polygonMember><gml:Polygon><gml:exterior><gml:LinearRing><gml:coordinates>5.0657329559,-41.1107215881 8.4824724197,-39.3435783386 4.3241734505,-34.6001853943 5.0657329559,-41.1107215881 </gml:coordinates></gml:LinearRing></gml:exterior></gml:Polygon></gml:polygonMember></gml:MultiPolygon></the_geom></mrhg:hej></wfs:Insert></wfs:Transaction>';*/


		/*$HTTP_RAW_POST_DATA = '<?xml version="1.0"?><DescribeFeatureType  version="1.1.0"  service="WFS"  xmlns="http://www.opengis.net/wfs"  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"  xsi:schemaLocation="http://www.opengis.net/wfs http://schemas.opengis.net/wfs/1.1.0/wfs.xsd">    <TypeName>california_coastline</TypeName></DescribeFeatureType>';
		 */

		// Post method is used
		if ($HTTP_RAW_POST_DATA) {
			//$forUseInSpatialFilter = $HTTP_RAW_POST_DATA; // We store a unaltered version of the raw request
			$HTTP_RAW_POST_DATA = dropNameSpace($HTTP_RAW_POST_DATA);
			logfile::write($HTTP_RAW_POST_DATA."\n\n");

			$status = $unserializer->unserialize($HTTP_RAW_POST_DATA);
			$arr = $unserializer->getUnserializedData();
			$request = $unserializer->getRootName();
			//print_r($arr);
			switch ($request){
				case "GetFeature":
					if (!is_array($arr['Query'][0])){
						$arr['Query'] = array(0 => $arr['Query']);
					}
					for ($i=0;$i<sizeof($arr['Query']);$i++){
						if (!is_array($arr['Query'][$i]['PropertyName'])) {
							$arr['Query'][$i]['PropertyName'] = array(0 => $arr['Query'][$i]['PropertyName']);
						}
					}
					$HTTP_FORM_VARS["REQUEST"] = "GetFeature";
					foreach ($arr['Query'] as $queries) {
						$HTTP_FORM_VARS["TYPENAME"].= $queries['typeName'].",";
						if ($queries['PropertyName'][0]) {
							foreach ($queries['PropertyName'] as $PropertyNames) {
								// We check if typeName is prefix and add it if its not
								if (strpos($PropertyNames, ".")) {
									$HTTP_FORM_VARS["PROPERTYNAME"].= $PropertyNames.",";
								}
								else {
									$HTTP_FORM_VARS["PROPERTYNAME"].= $queries['typeName'].".".$PropertyNames.",";
								}
							}
						}
						if (is_array($queries['Filter']) && $arr['version']=="1.0.0") {
							@$checkXml = simplexml_load_string($queries['Filter']);
							if($checkXml===FALSE) {
								makeExceptionReport("Filter is not valid");
							}
							$wheres[$queries['typeName']] = parseFilter($queries['Filter'],$queries['typeName']);
						}
					}
					$HTTP_FORM_VARS["TYPENAME"] = dropLastChrs($HTTP_FORM_VARS["TYPENAME"], 1);
					$HTTP_FORM_VARS["PROPERTYNAME"] = dropLastChrs($HTTP_FORM_VARS["PROPERTYNAME"], 1);
					break;
				case "DescribeFeatureType":
					$HTTP_FORM_VARS["REQUEST"] = "DescribeFeatureType";
					$HTTP_FORM_VARS["TYPENAME"] = $arr['TypeName'];
					//if (!$HTTP_FORM_VARS["TYPENAME"]) $HTTP_FORM_VARS["TYPENAME"] = $arr['typeName'];
					break;
				case "GetCapabilities":
					$HTTP_FORM_VARS["REQUEST"] = "GetCapabilities";
					break;
				case "Transaction":
					$HTTP_FORM_VARS["REQUEST"] = "Transaction";
					if (isset($arr["Insert"])) {
						$transactionType = "Insert";
					}
					if ($arr["Update"]) {
						$transactionType = "update";
					}
					if ($arr["Delete"]) $transactionType = "Delete";

					break;
			}
		}
		// Get method is used
		else {
			if (sizeof($_GET) > 0) {
				logfile::write($_SERVER['QUERY_STRING']."\n\n");
				$HTTP_FORM_VARS = $_GET;
				$HTTP_FORM_VARS = array_change_key_case($HTTP_FORM_VARS,CASE_UPPER);// Make keys case insensative
				$HTTP_FORM_VARS["TYPENAME"] = dropNameSpace($HTTP_FORM_VARS["TYPENAME"]);// We remove name space, so $where will get key without it.

				if ($HTTP_FORM_VARS['FILTER']) {
					@$checkXml = simplexml_load_string($HTTP_FORM_VARS['FILTER']);
					if($checkXml===FALSE) {
						makeExceptionReport("Filter is not valid");
					}
					//$forUseInSpatialFilter = $HTTP_FORM_VARS['FILTER'];
					$status = $unserializer->unserialize(dropNameSpace($HTTP_FORM_VARS['FILTER']));
					$arr = $unserializer->getUnserializedData();
					$wheres[$HTTP_FORM_VARS['TYPENAME']] = parseFilter($arr,$HTTP_FORM_VARS['TYPENAME']);
				}
			}
			else {
				$HTTP_FORM_VARS = array("");
			}
		}

		//HTTP_FORM_VARS is set in script if POST is used
		$HTTP_FORM_VARS = array_change_key_case($HTTP_FORM_VARS,CASE_UPPER);// Make keys case
		$HTTP_FORM_VARS["TYPENAME"] = dropNameSpace($HTTP_FORM_VARS["TYPENAME"]);
		$tables = explode(",",$HTTP_FORM_VARS["TYPENAME"]);
		$properties = explode(",", dropNameSpace($HTTP_FORM_VARS["PROPERTYNAME"]));
		$featureids = explode(",", $HTTP_FORM_VARS["FEATUREID"]);
		$bbox = explode(",", $HTTP_FORM_VARS["BBOX"]);

		// Start HTTP basic authentication
		//if(!$_SESSION["oauth_token"]) {
		$auth = $postgisObject->getGeometryColumns($postgisschema.".".$HTTP_FORM_VARS["TYPENAME"],"authentication");
		
		//}
		// End HTTP basic authentication
		print ("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
		ob_start();
		if (!(empty($properties[0]))) {
			foreach ($properties as $property) {
				$__u=explode(".", $property); // Is it "/" for get method?
				// We first check if typeName is namespace
				if ($__u[1]) {
					foreach ($tables as $table) {
						if ($table==$__u[0]) {
							$fields[$table].=$__u[1].",";
						}
					}
				}
				// No, typeName is not a part of value
				else {
					foreach ($tables as $table) {
						$fields[$table].=$property.",";
					}
				}

			}
		}
		if (!(empty($featureids[0]))) {
			foreach ($featureids as $featureid) {
				$__u=explode(".", $featureid);
				foreach ($tables as $table) {
					$primeryKey = $postgisObject->getPrimeryKey($postgisschema.".".$table);
					if ($table==$__u[0]) {
						$wheresArr[$table][]="{$primeryKey['attname']}={$__u[1]}";
					}
					$wheres[$table] = implode(" OR ",$wheresArr[$table]);
				}
			}
		}
		
		//get the request
		switch (strtoupper($HTTP_FORM_VARS["REQUEST"])) {
			case "GETCAPABILITIES":
				getCapabilities($postgisObject);
				break;
			case "GETFEATURE":
				if (!$gmlFeatureCollection) {
					$gmlFeatureCollection = "wfs:FeatureCollection";
				}
				print "<".$gmlFeatureCollection."\n";
				print "xmlns=\"http://www.opengis.net/wfs\"\n";
				print "xmlns:wfs=\"http://www.opengis.net/wfs\"\n";
				print "xmlns:gml=\"http://www.opengis.net/gml\"\n";
				print "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n";
				print "xmlns:{$gmlNameSpace}=\"{$gmlNameSpaceUri}\"\n";

				if ($gmlSchemaLocation) {
					print "xsi:schemaLocation=\"{$gmlSchemaLocation}\"";
				}
				else {
					//print "xsi:schemaLocation=\"{$gmlNameSpaceUri} {$thePath}?REQUEST=DescribeFeatureType&amp;TYPENAME=".$HTTP_FORM_VARS["TYPENAME"]." http://www.opengis.net/wfs ".str_replace("server.php","",$thePath)."schemas/wfs/1.0.0/WFS-basic.xsd\"";
					print "xsi:schemaLocation=\"{$gmlNameSpaceUri} {$thePath}?REQUEST=DescribeFeatureType&amp;TYPENAME=".$HTTP_FORM_VARS["TYPENAME"]." http://www.opengis.net/wfs http://wfs.plansystem.dk:80/geoserver/schemas/wfs/1.0.0/WFS-basic.xsd\"";
				}
				print ">\n";
				doQuery("Select");
				print "</".$gmlFeatureCollection.">";

				break;
			case "DESCRIBEFEATURETYPE":
				getXSD($postgisObject);
				break;
			case "TRANSACTION":
				doParse($arr);
				break;
			default:
				makeExceptionReport("Don't know that request");
				break;
		}
	}
}
?>

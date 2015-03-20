<?php if ( ! defined('BASEPATH')) exit('No direct script access allowed');

class Cuahsi extends CI_Controller {

	/**
	 * Index page
	 *
	 */
	public function index()
	{
		RunService();
		exit;
	}

	/**
	 * Implement GetSiteInfo
	 *
	 */
	public function GetSiteInfo()
	{
		if ($this->validate_token()) {
			if (!isset($_REQUEST["site"])) {
	            echo "Value cannot be null.\nParameter name: SiteCode";
	        } else {
	            $site = $_REQUEST["site"];
	            if ($site == "") {
	                echo "Value cannot be null.\nParameter name: SiteCode";
	            } else {
	                header("Content-type: text/xml");
	                echo chr(60) . chr(63) . 'xml version="1.0" encoding="utf-8" ' . chr(63) . chr(62);
	                echo "<string>" . htmlspecialchars(wof_GetSiteInfo_REST($site)) . "</string>";
	            }
	        }
	        exit;
        }
	}

	/**
	 * Implement GetSiteInfoMultpleObject
	 *
	 */
	public function GetSiteInfoMultpleObject()
	{
		if ($this->validate_token()) {
			if (!isset($_REQUEST["site"])) {
	            echo "Value cannot be null.\nParameter name: SiteCode";
	        } else {
	            $site = $_REQUEST["site"];
	            if ($site == "") {
	                echo "Value cannot be null.\nParameter name: SiteCode";
	            } else {
	                write_XML_header();
	                echo wof_GetSiteInfoMultipleObject(explode(",",$site));
	            }
	        }
	        exit;
   		}
	}

	/**
	 * Implement GetSiteInfoObject
	 *
	 */
	public function GetSiteInfoObject()
	{
		if ($this->validate_token()) {
			if (!isset($_REQUEST["site"])) {
	            echo "Value cannot be null.\nParameter name: SiteCode";
	        } else {
	            $site = $_REQUEST["site"];
	            if ($site == "") {
	                echo "Value cannot be null.\nParameter name: SiteCode";
	            } else {
	                write_XML_header();
	                echo wof_GetSiteInfo($site);
	            }
	        }
	        exit;
   		}
	}

	/**
	 * Implement GetSites
	 *
	 */
	public function GetSites()
	{
		if ($this->validate_token()) {
			$site = isset($_REQUEST['site'])? explode(",",$_REQUEST['site']):array();
	        write_XML_header();
	        echo "<string>" . htmlspecialchars(wof_GetSites($site)) . "</string>";
	        exit;
   		}
	}

	/**
	 * Implement GetSitesObject
	 *
	 */
	public function GetSitesObject()
	{
		if ($this->validate_token()) {
			$site = isset($_REQUEST['site'])? explode(",",$_REQUEST['site']):array();
	        header("Content-type: text/xml");
	        echo chr(60) . chr(63) . 'xml version="1.0" encoding="utf-8" ' . chr(63) . chr(62);
	        echo wof_GetSites($site);
	        exit;
   		}
	}

	/**
	 * Implement GetSitesByBoxObject
	 *
	 */
	public function GetSitesByBoxObject()
	{
		if ($this->validate_token()) {
			$includeSeries = FALSE;
	        if (!isset($_REQUEST["west"])) {
	            echo "Missing parameter: west";
	            exit;
	        }
	        if (!isset($_REQUEST["south"])) {
	            echo "Missing parameter: south";
	            exit;
	        }
	        if (!isset($_REQUEST["east"])) {
	            echo "Missing parameter: east";
	            exit;
	        }
	        if (!isset($_REQUEST["north"])) {
	            echo "Missing parameter: north";
	            exit;
	        }
	        if (isset($_REQUEST["IncludeSeries"])) {
	            $includeSeries = $_REQUEST["IncludeSeries"]; //TRUE or FALSE
	        }
	        $west = $_REQUEST['west'];
	        if ($west == NULL) {
	            echo "Value cannot be null.\nParameter name: west";
	            exit;
	        }
	        $south = $_REQUEST['south'];
	        if ($south == NULL) {
	            echo "Value cannot be null.\nParameter name: south";
	            exit;
	        }
	        $east = $_REQUEST['east'];
	        if ($east == NULL) {
	            echo "Value cannot be null.\nParameter name: east";
	            exit;
	        }
	        $north = $_REQUEST['north'];
	        if ($north == NULL) {
	            echo "Value cannot be null.\nParameter name: north";
	            exit;
	        }
	        write_XML_header();
	        echo wof_GetSitesByBox($west, $south, $east, $north, $includeSeries);
	        exit;
   		}
	}

	/**
	 * Implement GetValues
	 *
	 */
	public function GetValues()
	{
		if ($this->validate_token()) {
			
			
			if (!isset($_REQUEST['location'])) {
	            echo "Missing parameter: location";
	            exit;
	        }
	        if (!isset($_REQUEST['variable'])) {
	            echo "Missing parameter: variable";
	            exit;
	        }

			
	        $location = $_REQUEST["location"];
	        $variable = $_REQUEST["variable"];
	        $startDate = isset($_REQUEST["startDate"])? $_REQUEST["startDate"]:"";
	        $endDate = isset($_REQUEST["endDate"])? $_REQUEST["endDate"]:"";
	        
			//Adding Version checking here to enable support for WaterML 2.0. Note : Only function changed is the Get_Values operation. Maybe some day all the WaterOneFlow services
			//will be upgraded to WaterML 2.0 . But until then , just this is supported. This is the data service endpoint that will be served to the users of WFS services. 
			
			if(!isset($_REQUEST['version']))
			{
				//No version found defaulting to 2
				$version = "2.0";
			}
			else
			{
				$version=$_REQUEST['version'];
			}
			
			write_XML_header();
			
			if($version=="2.0"):
				global $dictionary;
				$dictionary = array();
				echo wof_GetValues_2($location, $variable, $startDate, $endDate);
			else:
				echo '<string>';
				echo htmlspecialchars(wof_GetValues($location, $variable, $startDate, $endDate));
				echo "</string>";
			endif;
			
	        exit;
   		}
	}

	/**
	 * Implement GetValuesObject
	 *
	 */
	public function GetValuesObject()
	{
		if ($this->validate_token()) {
			if (!isset($_REQUEST['location'])) {
	            echo "Missing parameter: location";
	            exit;
	        }
	        if (!isset($_REQUEST['variable'])) {
	            echo "Missing parameter: variable";
	            exit;
	        }
	        $location = $_REQUEST["location"];
	        $variable = $_REQUEST["variable"];
	        $startDate = isset($_REQUEST["startDate"])? $_REQUEST["startDate"]:"";
	        $endDate = isset($_REQUEST["endDate"])? $_REQUEST["endDate"]:"";
	        write_XML_header();
	        echo wof_GetValues($location, $variable, $startDate, $endDate);
	        exit;
   		}
	}

	/**
	 * Implement GetValuesForASiteObject
	 *
	 */
	public function GetValuesForASiteObject()
	{
		if ($this->validate_token()) {
			if (!isset($_REQUEST['site'])) {
	            echo "Missing parameter: site";
	            exit;
	        }
	        if (!isset($_REQUEST['startDate'])) {
	            echo "Missing parameter: startDate";
	            exit;
	        }
	        if (!isset($_REQUEST['endDate'])) {
	            echo "Missing parameter: endDate";
	            exit;
	        }
	        $site = $_REQUEST['site'];
	        $startDate = $_REQUEST['startDate'];
	        $endDate = $_REQUEST['endDate'];
	        write_XML_header();
	        echo wof_GetValuesForASite($site, $startDate, $endDate);
	        exit;
   		}
	}

	/**
	 * Implement GetVariableInfo
	 *
	 */
	public function GetVariableInfo()
	{
		if ($this->validate_token()) {
			$variable = "";
	        if (isset($_REQUEST['variable'])) {
	            $variable = $_REQUEST['variable'];
	        }
	        write_XML_header();
	        echo '<string>';
	        echo htmlspecialchars(wof_GetVariableInfo($variable));
	        echo '</string>';
	        exit;
        }
	}

	/**
	 * Implement GetVariableInfoObject
	 *
	 */
	public function GetVariableInfoObject()
	{
		if ($this->validate_token()) {
			$variable = "";
	        if (isset($_REQUEST['variable'])) {
	            $variable = $_REQUEST['variable'];
	        }
	        write_XML_header();
	        echo wof_GetVariableInfo($variable);
	        exit;
        }
	}

	/**
	 * Implement GetVariables
	 *
	 */
	public function GetVariables()
	{
		if ($this->validate_token()) {
			write_XML_header();
	        echo '<string>';
	        echo htmlspecialchars(wof_GetVariables());
	        echo '</string>';
	        exit;
        }
	}

	/**
	 * Implement GetVariablesObject
	 *
	 */
	public function GetVariablesObject()
	{
		if ($this->validate_token()) {
			write_XML_header();
	        echo wof_GetVariables();
	        exit;
        }
	}

	/**
	 * Implement web tester
	 *
	 */
	public function test()
	{
		$this->load->view("test");
	}

	/**
	 * Get Method parameter(s) by ajax
	 *
	 */
	public function method_get_params($method = "") {
		$this->load->helper("hydroservices");

		$content = "";
		if (isset($method) && $method != "") {
	 		switch ($method) {
		      	case "GetSites":
		          	break;
		      	case "GetSiteInfo":
		          	$content .= "
					  	<div class=\"param_container\">
						  	<label>SiteCode</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"site\" name=\"site\" value=\"".$this->config->item('service_code').":".get_random_site()."\" class=\"must\" title=\"The site code in the format: network:site_code. For example: KALA:SiteCode.\" />
						  	</div>
						</div>";
		          	break;
		      	case "GetSiteInfoMultpleObject":
		          	$content .= "
					  	<div class=\"param_container\">
						  	<label>SiteCode</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"site\" name=\"site\" value=\"".$this->config->item('service_code').":".get_random_site()."\" class=\"must\" title=\"The site code in the format: network:site_code. For example: KALA:SiteCode. To show multiple sites, enter a list of codes separated by comma (,)\" />
						  	</div>
						</div>";
		          	break;
		      	case "GetSiteInfoObject":
		          	$content .= "
					  	<div class=\"param_container\">
						  	<label>SiteCode</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"site\" name=\"site\" value=\"".$this->config->item('service_code').":".get_random_site()."\" class=\"must\" title=\"The site code in the format: network:site_code. For example: KALA:SiteCode.\" />
						  	</div>
						</div>";
		          	break;
		      	case "GetSitesObject":
		          	break;
		      	case "GetSitesByBoxObject":
		      		$coordinate = get_random_site_coordinate();
		          	$content .= "
					  	<div class=\"param_container\">
						  	<label>West</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"west\" name=\"west\" value=\"".$coordinate["Longitude"]."\" class=\"numeric must\" title=\"Fill with Longitude coordinate.\" />
						  	</div>
					  	</div>
					  	<div class=\"param_container\">
						  	<label>South</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"south\" name=\"south\" value=\"".$coordinate["Latitude"]."\" class=\"numeric must\" title=\"Fill with Latitude coordinate.\" />
						  	</div>
					  	</div>
					  	<div class=\"param_container\">
						  	<label>East</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"east\" name=\"east\" value=\"".$coordinate["Longitude"]."\" class=\"numeric must\" title=\"Fill with Longitude coordinate.\" />
						  	</div>
					  	</div>
					  	<div class=\"param_container\">
						  	<label>North</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"north\" name=\"north\" value=\"".$coordinate["Latitude"]."\" class=\"numeric must\" title=\"Fill with Latitude coordinate.\" />
						  	</div>
					  	</div>
					  	<div class=\"param_container\">
						  	<label>Include Series</label>
						  	<div class=\"content\">
							  	<input type=\"checkbox\" id=\"IncludeSeries\" name=\"IncludeSeries\" title=\"Check for include series.\" />
						  	</div>
					  	</div>";
		          	break;
		      	case "GetValues":
		          	$content .= "
					  	<div class=\"param_container\">
						  	<label>Location</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"location\" name=\"location\" value=\"".$this->config->item('service_code').":".get_random_site()."\" class=\"must\" title=\"The location in the format: network:site_code. For example: KALA:SiteCode.\" />
						  	</div>
						</div>
					  	<div class=\"param_container\">
						  	<label>Variable</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"variable\" name=\"variable\" value=\"".$this->config->item('service_code').":".get_random_variable()."\" class=\"must\" title=\"The variable in the format: network:variable_code. For example: KALA:IDCS-5-Avg.\" />
						  	</div>
					  	</div>
					  	<div class=\"param_container\">
						  	<label>Start Date</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"startDate\" name=\"startDate\" value=\"\" class=\"datepicker\" /> 
							  	<a onclick=\"javascript:remove(this);\" class=\"remove\">x</a>
						  	</div>
					  	</div>
					  	<div class=\"param_container\">
						  	<label>End Date</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"endDate\" name=\"endDate\" value=\"\" class=\"datepicker\" /> 
							  	<a onclick=\"javascript:remove(this);\" class=\"remove\">x</a>
						  	</div>
					  	</div>";
		          	break;
		      	case "GetValuesObject":
		          	$content .= "
					  	<div class=\"param_container\">
						  	<label>Location</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"location\" name=\"location\" value=\"".$this->config->item('service_code').":".get_random_site()."\" class=\"must\" title=\"The location in the format: network:site_code. For example: KALA:SiteCode.\" />
						  	</div>
						</div>
					  	<div class=\"param_container\">
						  	<label>Variable</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"variable\" name=\"variable\" value=\"".$this->config->item('service_code').":".get_random_variable()."\" class=\"must\" title=\"The variable in the format: network:variable_code. For example: KALA:IDCS-5-Avg.\" />
						  	</div>
					  	</div>
					  	<div class=\"param_container\">
						  	<label>Start Date</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"startDate\" name=\"startDate\" value=\"\" class=\"datepicker\" /> 
							  	<a onclick=\"javascript:remove(this);\" class=\"remove\">x</a>
						  	</div>
					  	</div>
					  	<div class=\"param_container\">
						  	<label>End Date</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"endDate\" name=\"endDate\" value=\"\" class=\"datepicker\" /> 
							  	<a onclick=\"javascript:remove(this);\" class=\"remove\">x</a>
						  	</div>
					  	</div>";
		          	break;
		      	case "GetValuesForASiteObject":
		          	$content .= "
					  	<div class=\"param_container\">
						  	<label>SiteCode</label>
					  		<div class=\"content\">
							  	<input type=\"text\" id=\"site\" name=\"site\" value=\"".$this->config->item('service_code').":".get_random_site()."\" class=\"must\" title=\"The site code in the format: network:site_code. For example: KALA:SiteCode.\" />
						  	</div>
						</div>
					  	<div class=\"param_container\">
						  	<label>Start Date</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"startDate\" name=\"startDate\" value=\"\" class=\"datepicker must\" />
						  	</div>
					  	</div>
					  	<div class=\"param_container\">
						  	<label>End Date</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"endDate\" name=\"endDate\" value=\"\" class=\"datepicker must\" />
						  	</div>
					  	</div>";
		          	break;
		      	case "GetVariables":
		          	break;
		      	case "GetVariablesObject":
		          	break;
		      	case "GetVariableInfo":
		          	$content .= "
					  	<div class=\"param_container\">
						  	<label>Variable</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"variable\" name=\"variable\" value=\"".get_random_variable()."\" class=\"must\" title=\"Fill with variable code.\" />
						  	</div>
					  	</div>";
		          	break;
		      	case "GetVariableInfoObject":
		          	$content .= "
				  		<div class=\"param_container\">
						  	<label>Variable</label>
						  	<div class=\"content\">
							  	<input type=\"text\" id=\"variable\" name=\"variable\" value=\"".get_random_variable()."\" class=\"must\" title=\"Fill with variable code.\" />
						  	</div>
					  	</div>";
		          	break;
		    }
	    }

		echo $content;
	}

	/**
	 * Generate by ajax
	 *
	 */
	public function generate_url() {
		$target = base_url()."index.php/cuahsi_1_1.asmx/".$_POST["method"];

		$url = "";
		foreach($_POST as $k=>$v) {
			if (is_array($v)) {
				$url .= http_build_query(array($k=>$v), '', '&amp;')."&";
			} else {
				if ($k != "method") {
					if (strtolower($v) != "on") {
						$url .= $k."=".$v."&";
					} else {
						$url .= $k."=true&";
					}
				}
			}
		}

		$url = strlen($url) > 0? substr($url,0,-1):"";

		$url = $target.(strlen($url) > 0? "?".$url:"");

		echo "<div class=\"param_container\"><label>URL</label><div class=\"content\"><a href=\"".$url."\" target=\"_blank\">".$url."</a></div></div>";
	}

	/**
	 * token validation
	 *
	 */
	private function validate_token() {
		$validToken = $this->config->item('auth_token');
	    $authToken = isset($_POST['authToken'])? $_POST['authToken']:'';
	    
	    if ($validToken == "" || ($validToken != "" && $validToken == $authToken)) {
	    	return true;
	    } else {
			header("Status: 401 Unauthorized");
			header("Content-type: text/plain");
			echo "HTTP/1.0 401 Unauthorized";
		}
	}
}
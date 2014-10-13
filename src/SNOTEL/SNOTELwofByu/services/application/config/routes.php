<?php  if ( ! defined('BASEPATH')) exit('No direct script access allowed');
/*
| -------------------------------------------------------------------------
| URI ROUTING
| -------------------------------------------------------------------------
| This file lets you re-map URI requests to specific controller functions.
|
| Typically there is a one-to-one relationship between a URL string
| and its corresponding controller class/method. The segments in a
| URL normally follow this pattern:
|
|	example.com/class/method/id/
|
| In some instances, however, you may want to remap this relationship
| so that a different class/function is called than the one
| corresponding to the URL.
|
| Please see the user guide for complete details:
|
|	http://codeigniter.com/user_guide/general/routing.html
|
| -------------------------------------------------------------------------
| RESERVED ROUTES
| -------------------------------------------------------------------------
|
| There area two reserved routes:
|
|	$route['default_controller'] = 'welcome';
|
| This route indicates which controller class should be loaded if the
| URI contains no data. In the above example, the "welcome" class
| would be loaded.
|
|	$route['404_override'] = 'errors/page_missing';
|
| This route will tell the Router what URI segments to use if those provided
| in the URL cannot be matched to a valid route.
|
*/

$route['default_controller'] = "welcome";

// REST routing
$route["cuahsi_1_1.asmx"] 							= "cuahsi";
$route["cuahsi_1_1.asmx/GetSites"] 					= "cuahsi/GetSites";
$route["cuahsi_1_1.asmx/GetSiteInfo"] 				= "cuahsi/GetSiteInfo";
$route["cuahsi_1_1.asmx/GetSiteInfoMultpleObject"] 	= "cuahsi/GetSiteInfoMultpleObject";
$route["cuahsi_1_1.asmx/GetSiteInfoObject"] 		= "cuahsi/GetSiteInfoObject";
$route["cuahsi_1_1.asmx/GetSitesObject"] 			= "cuahsi/GetSitesObject";
$route["cuahsi_1_1.asmx/GetSitesByBoxObject"] 		= "cuahsi/GetSitesByBoxObject";
$route["cuahsi_1_1.asmx/GetValues"] 				= "cuahsi/GetValues";
$route["cuahsi_1_1.asmx/GetValuesObject"] 			= "cuahsi/GetValuesObject";
$route["cuahsi_1_1.asmx/GetValuesForASiteObject"] 	= "cuahsi/GetValuesForASiteObject";
$route["cuahsi_1_1.asmx/GetVariables"] 				= "cuahsi/GetVariables";
$route["cuahsi_1_1.asmx/GetVariablesObject"] 		= "cuahsi/GetVariablesObject";
$route["cuahsi_1_1.asmx/GetVariableInfo"] 			= "cuahsi/GetVariableInfo";
$route["cuahsi_1_1.asmx/GetVariableInfoObject"] 	= "cuahsi/GetVariableInfoObject";
// end of REST routing

// Test Page routing
$route["test"] 										= "cuahsi/test";
// end of Test Page routing

// Update CV Page routing
$route["updatecv.php"] 								= "updatecv";
// end of Test Page routing

$route['404_override'] = '';


/* End of file routes.php */
/* Location: ./application/config/routes.php */
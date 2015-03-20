<?php
if (!defined('BASEPATH'))
    exit('No direct script access allowed');

//run the service
if (!function_exists('parseControlledVocabularyObject')) {

    function parseControlledVocabularyObject($xmlObj) {
    	$xml   = simplexml_load_string($xmlObj);
		$array = json_decode(json_encode((array) $xml), 1);
		$array = array($xml->getName() => $array);
		return $array;
    }
}

if (!function_exists('checkExistingControlledVocabulary')) {

	function checkExistingControlledVocabulary($table,$cond,$value) {
	    $ci = &get_instance();

		$ci->db->select("1",FALSE);
		$ci->db->where($cond,$value);

	    $result = $ci->db->get($table);
	    if ($result->num_rows() > 0) {
	        return true;
	    }
	
	    return false;
	}
}

if (!function_exists('insertNewControlledVocabularyData')) {

	function insertNewControlledVocabularyData($table,$data) {
	    $ci = &get_instance();

		foreach($data as $row) {
			$keys = array_keys($row);
			if (!checkExistingControlledVocabulary($table,$keys[0],$row[$keys[0]])) {
				$ci->db->insert($table,$row);
			}
	    }
	}
}

if (!function_exists('getControlledVocabularyData')) {

	function getControlledVocabularyData($table) {
	    $ci = &get_instance();

		return $ci->db->get($table);
	}
}
?>
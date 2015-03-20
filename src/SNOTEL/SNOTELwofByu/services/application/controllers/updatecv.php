<?php if ( ! defined('BASEPATH')) exit('No direct script access allowed');

class Updatecv extends CI_Controller {

	public function index() {
		$this->load->view('updatecv');
	}

	public function update($method) {
		$this->load->helper('controlledvocabulary');
		$client = new SoapClient($this->config->item('odm_service'));
		$xmlData = $client->{$method}();
		$data = parseControlledVocabularyObject($xmlData->{$method.'Result'});
		insertNewControlledVocabularyData(strtolower(str_replace("Get","",$method)),$data[$method."Response"]["Records"]["Record"]);

		$records = getControlledVocabularyData(strtolower(str_replace("Get","",$method)));

		$vocabs = array();
		foreach($records->result_array() as $row) {
			$vocabs[] = $row;
		}

		echo json_encode($vocabs);
	}
}

/* End of file updatecv.php */
/* Location: ./application/controllers/updatecv.php */
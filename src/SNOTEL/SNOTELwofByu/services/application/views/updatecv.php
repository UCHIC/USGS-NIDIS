<!DOCTYPE html>
<html lang="en">
<head>
	<meta charset="utf-8"/>
	<title>Update Controlled Vocabulary</title>
	<link rel="shortcut icon" href="<?=base_url()?>assets/images/favicon.ico" type="image/x-icon" />
	<link rel="bookmark" href="<?=base_url()?>assets/images/favicon.ico" />

	<link href="<?=base_url()?>assets/css/style.css" rel="stylesheet"/>

	<link href="<?=base_url()?>assets/css/smoothness/jquery-ui-1.10.3.custom.css" rel="stylesheet"/>

	<script type="text/javascript" src="<?=base_url()?>assets/js/jquery-1.9.1.min.js"></script>
	<script type="text/javascript" src="<?=base_url()?>assets/js/jquery-ui-1.10.3.custom.min.js"></script>

</head>
<body>
<script type="text/javascript">
$(document).ready(function() {
	$("#method").change(function() {
		if ($(this).val() != "") {
			$("#update_param").show();
		} else {
			$("#update_param").hide();
		}
	});

	$("#btnUpdate").click(function(e) {
		$("#current_data").html('<img src="<?=base_url().'assets/images/loading.gif'?>" />');
	    $.ajax({
           type: "GET",
           url: "<?=base_url()?>updatecv/update/"+$("#method").val(),
           success: function(data)
           {
           		var result = '';
				$record = $.parseJSON(data);
				if ($record.length > 0) {
					result += '<table border="1">';

					result += '<tr>';
					$.each($record[0], function(key, value) {
				      	result += '<td>'+key+'</td>';
				   	});
					result += '</tr>';

					$record.forEach(function(v) {
						result += '<tr>';
						$.each(v, function(key, value) {
					      	result += '<td>'+value+'</td>';
					   	});
						result += '</tr>';
					});

					result += '/<table>';
				}
               	$("#current_data").html(result); // show response from the php script.
           }
        });
	    e.preventDefault(); // avoid to execute the actual submit of the form.
	});
});
</script>
<div id="container">
	<h1>Update Controlled Vocabulary</h1>

	<div id="body">
		<form id="form_url_generator">
		<div id="base_param">
			<div class="param_container">
			    <label>Web Method Name</label>
				<div class="content">
					<select id="method" name="method" title="Select web service method here.">
						<option value="">.: Select Web Method :.</option>
						<option class="opt" value="GetCensorCodeCV">GetCensorCodeCV</option>
						<option class="opt" value="GetGeneralCategoryCV">GetGeneralCategoryCV</option>
						<option class="opt" value="GetSiteTypeCV">GetSiteTypeCV</option>
						<option class="opt" value="GetSampleMediumCV">GetSampleMediumCV</option>
						<option class="opt" value="GetSampleTypeCV">GetSampleTypeCV</option>
						<option class="opt" value="GetSpatialReferences">GetSpatialReferences</option>
						<option class="opt" value="GetSpeciationCV">GetSpeciationCV</option>
						<option class="opt" value="GetTopicCategoryCV">GetTopicCategoryCV</option>
						<option class="opt" value="GetUnits">GetUnits</option>
						<option class="opt" value="GetValueTypeCV">GetValueTypeCV</option>
						<option class="opt" value="GetVariableNameCV">GetVariableNameCV</option>
						<option class="opt" value="GetVerticalDatumCV">GetVerticalDatumCV</option>
					</select>
				</div> 
			</div>
		</div>
		<div id="method_param"></div>
		<div id="update_param" style="display:none;">
			<div class="param_container">
			    <label>&nbsp;</label>
				<div class="content">
					<input type="submit" id="btnUpdate" value="Update Controlled Vocabulary" />
				</div> 
			</div>
		</div>
		</form>
		<br />
		<div id="current_data"></div>

	</div>

	<p class="footer"><font color=#000000 face=Arial, Helvetica, sans-serif size=2><i>Copyright &copy; 2012. <a href='http://hydroserverlite.codeplex.com/' target='_blank' class='reversed'>Hydroserver Lite</a>. All Rights Reserved. <a href='http://hydroserverlite.codeplex.com/team/view' target='_blank' class='reversed'>Meet the Developers</a></i></font></p>
</div>

</body>
</html>
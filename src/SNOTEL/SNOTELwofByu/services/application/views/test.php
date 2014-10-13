<!DOCTYPE html>
<html lang="en">
<head>
	<meta charset="utf-8"/>
	<title>Hydrodata Web Tester</title>
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
	$(".datepicker").datepicker("option", "dateFormat", "yy-mm-dd");
	$("#method").change(function() {
		$("#generated_url").html("");
		if ($(this).val() != "") {
			$("#generator_param").show();
		} else {
			$("#generator_param").hide();
		}

		$.ajax({
			url:"<?=base_url()?>/index.php/cuahsi/method_get_params/" + $(this).val(),
			success: function(response){
				$("#method_param").html(response);
				$(document).tooltip();
				$(".datepicker").datepicker("destroy");
			    $("#endDate").datepicker({ dateFormat: 'yy-mm-dd' });
			    $("#startDate").datepicker({
			    	dateFormat: 'yy-mm-dd',
			    	onSelect: function(dateStr) {
			    		$("#endDate").datepicker("option", "setDate", $(this).datepicker('getDate'));
			    		$("#endDate").datepicker("option", "minDate", $(this).datepicker('getDate'));
			    	}
				});
	  		},
	  		dataType:"html"
	  	});
	});

	$("#btnGenerator").click(function(e) {
	    $.ajax({
           type: "POST",
           url: "<?=base_url()?>/index.php/cuahsi/generate_url/",
           data: $("#form_url_generator").serialize(), // serializes the form's elements.
           success: function(data)
           {
               $("#generated_url").html(data); // show response from the php script.
           }
        });
	    e.preventDefault(); // avoid to execute the actual submit of the form.
	});
});

function remove(obj) {
	$(obj).parent().parent().remove();
}
</script>
<div id="container">
	<h1>Hydrodata Web Tester</h1>

	<div id="body">
		<form id="form_url_generator">
		<div id="base_param">
			<div class="param_container">
			    <label>Service Method</label>
				<div class="content">
					<select id="method" name="method" title="Select service method here.">
						<option value="">.: Select Method :.</option>
						<option class="opt" value="GetSiteInfo">GetSiteInfo</option>
						<option class="opt" value="GetSiteInfoMultpleObject">GetSiteInfoMultpleObject</option>
						<option class="opt" value="GetSiteInfoObject">GetSiteInfoObject</option>
						<option class="opt" value="GetSites">GetSites</option>
						<option class="opt" value="GetSitesByBoxObject">GetSitesByBoxObject</option>
						<option class="opt" value="GetSitesObject">GetSitesObject</option>
						<option class="opt" value="GetValues">GetValues</option>
						<option class="opt" value="GetValuesForASiteObject">GetValuesForASiteObject</option>
						<option class="opt" value="GetValuesObject">GetValuesObject</option>
						<option class="opt" value="GetVariableInfo">GetVariableInfo</option>
						<option class="opt" value="GetVariableInfoObject">GetVariableInfoObject</option>
						<option class="opt" value="GetVariables">GetVariables</option>
						<option class="opt" value="GetVariablesObject">GetVariablesObject</option>
					</select>
				</div> 
			</div>
		</div>
		<div id="method_param"></div>
		<div id="generator_param" style="display:none;">
			<div class="param_container">
			    <label>&nbsp;</label>
				<div class="content">
					<input type="submit" id="btnGenerator" value="Generate URL" />
				</div> 
			</div>
		</div>
		</form>
		<br />
		<div id="generated_url"></div>

	</div>

	<p class="footer"><font color=#000000 face=Arial, Helvetica, sans-serif size=2><i>Copyright &copy; 2012. <a href='http://hydroserverlite.codeplex.com/' target='_blank' class='reversed'>Hydroserver Lite</a>. All Rights Reserved. <a href='http://hydroserverlite.codeplex.com/team/view' target='_blank' class='reversed'>Meet the Developers</a></i></font></p>
</div>

</body>
</html>
<?php $this->load->view('header'); ?>

		<p>
		The HydroServer Lite Interactive Web Client is an online software tool that helps store, organize, and publish data provided by citizen scientists.
		</p>
		<p>
What are citizen scientists? They can be anyone who collects and shares scientific data with professional scientists to achieve common goals.
		</p>
		<p>
Once you are a registered user, you will be able to login and upload your own data into our database to provide valuable input into the research being done in your area as well as around the world. 
		</p>
		<br />
		<div id="base_info">
			<div class="info_container">
			    <label class="info_label"><a href="<?php echo 'index.php/test';?>" class="info_link">REST Service Test</a></label>
				<div class="info_content">
					<div class="link_desc">
						&nbsp;You can perform tests on all of the methods in Hydrodata Server on this page. In this case the test for REST Service.
					</div>
				</div> 
			</div>
			<div class="info_container">
			    <label class="info_label"><a href="<?php echo 'index.php/cuahsi_1_1.asmx';?>" class="info_link">SOAP Web Service</a></label>
				<div class="info_content">
					<div class="link_desc">
						&nbsp;Hydroserver soap service page.
					</div>
				</div> 
			</div>
			<div class="info_container">
			    <label class="info_label"><a href="<?php echo 'index.php/updatecv.php';?>" class="info_link">Update Controlled Vocabulary</a></label>
				<div class="info_content">
					<div class="link_desc">
						&nbsp;Update Controlled Vocabulary from HIS Central.
					</div>
				</div> 
			</div>
			<div class="info_container">
			    <label class="info_label"><a href="<?php echo 'index.php/wfs/write_xml?service=WFS&request=GetCapabilities&version=1.0.0';?>" class="info_link">WFS Services</a></label>
				<div class="info_content">
					<div class="link_desc">
						&nbsp;WFS 1.0.0.
					</div>
				</div> 
			</div>
			<div class="info_container">
			    <label class="info_label"><a href="<?php echo 'index.php/wfs/write_xml?service=WFS&request=GetCapabilities&version=2.0.0';?>" class="info_link">WFS Services</a></label>
				<div class="info_content">
					<div class="link_desc">
						&nbsp;WFS 2.0.0.
					</div>
				</div> 
			</div>
		</div>
	
<?php $this->load->view('footer'); ?>
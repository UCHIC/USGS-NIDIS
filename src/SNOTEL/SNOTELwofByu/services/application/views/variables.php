<?php $this->load->view('header'); ?>

		<div id="base_info">
			<?php 
			foreach( $variables as $variable ) {
			?>
			<div class="info_container">
			    <label class="info_label"><a href="<?php echo '../wfs/write_xml?VariableID=' . $variable->VariableID . '&request=GetCapabilities';?>" class="info_link"><?php echo $variable->VariableCode; ?></a></label>
				<div class="info_content">
					<div class="link_desc">
						&nbsp;<?php echo $variable->VariableName;?>
					</div>
				</div> 
			</div>
			<?php 
			}
			?>
		</div>

<?php $this->load->view('footer'); ?>
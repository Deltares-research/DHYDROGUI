<?xml version="1.0" ?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns="http://schemas.microsoft.com/wix/2006/wi"
    xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
    exclude-result-prefixes="wix"  >

	<!-- Copy all attributes and elements to the output. -->
	<xsl:template match="@*|*">
		<xsl:copy>
			<xsl:apply-templates select="@*" />
			<xsl:apply-templates select="*" />
		</xsl:copy>
	</xsl:template>

	<!-- Add define wxi. -->
	<xsl:template match="wix:Wix"> 
		<xsl:copy> 
			<xsl:processing-instruction name="include">$(var.ProjectDir)..\Harvest\HarvestDefines.wxi</xsl:processing-instruction> 
			<xsl:apply-templates/> 
		</xsl:copy> 
	</xsl:template> 

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()" />
		</xsl:copy>	
	</xsl:template>

	<xsl:output method="xml" indent="yes" version="1.0" encoding="utf-8"/>  

  <!-- identity template -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>
  
    
  <!-- Replace RGFGrid Folder attribute id -->
  <xsl:key name="rgfgrid-file-search" match="wix:Component[contains(wix:File/@Source, 'rgfgrid.cmd')]" use="ancestor::wix:Directory/@Id" />
  <xsl:template match="wix:Directory[key('rgfgrid-file-search', @Id) and @Name='x64']/@Id">
    <xsl:attribute name="{name()}">RGFGridLibDir</xsl:attribute>
  </xsl:template>
  
  <!-- Replace Dido Folder attribute id -->
  <xsl:key name="dido-file-search" match="wix:Component[contains(wix:File/@Source, 'dido.cmd')]" use="ancestor::wix:Directory/@Id" />
  <xsl:template match="wix:Directory[key('dido-file-search', @Id) and @Name='x64']/@Id">
    <xsl:attribute name="{name()}">DidoLibDir</xsl:attribute>
  </xsl:template>
  
  <!-- Create searches for the directories to remove or added on a condition. -->
  <xsl:key name="DeltaShell.Plugins.DelftModels.WaterQualityModel-folder-search" match="wix:Directory[contains(@Name, 'DeltaShell.Plugins.DelftModels.WaterQualityModel')]" use="descendant::wix:Component/@Id" />
  <xsl:key name="dimr-folder-search" match="wix:Directory[@Name = 'kernels']" use="descendant::wix:Component/@Id" />
  <xsl:key name="RGFGridLibDir-folder-search" match="wix:Directory[contains(wix:Component/wix:File/@Source, 'rgfgrid.cmd') and @Name='x64']" use="descendant::wix:Component/@Id" />
  
  <!-- Create searches for the files to added on condition. -->
  <xsl:key name="waqScripts-file-search" match="wix:Component[contains(wix:File/@Source, 'WaterQualityModel')]" use="@Id" />
  <xsl:key name="waqKernel-file-search" match="wix:Component[contains(wix:File/@Source, 'plct') or contains(wix:File/@Source, 'WaterQualityModel\plugins-qt') or contains(wix:File/@Source, 'WaterQualityModel\waq_kernel')]" use="@Id" />
  
  <!-- Create searches for the files to remove. -->
  <xsl:key name="pdb-file-search" match="wix:Component[contains(wix:File/@Source, '.pdb')]" use="@Id" />
  <xsl:key name="pspdb-file-search" match="wix:Component[contains(wix:File/@Source, '.pspdb')]" use="@Id" />
  <xsl:key name="pssym-file-search" match="wix:Component[contains(wix:File/@Source, '.pssym')]" use="@Id" />

  <!-- Exclude special PLCT files -->
  <xsl:key name="coup203-exe-file-search" match="wix:Component[contains(wix:File/@Source, 'coup203.exe')]" use="@Id" />
  <xsl:key name="coupsds-exe-file-search" match="wix:Component[contains(wix:File/@Source, 'coupsds.exe')]" use="@Id" />
  <xsl:key name="install_open_proc_lib-tcl-file-search" match="wix:Component[contains(wix:File/@Source, 'install_open_proc_lib.tcl')]" use="@Id" />
  <xsl:key name="nestwq1-exe-file-search" match="wix:Component[contains(wix:File/@Source, 'nestwq1.exe')]" use="@Id" />
  <xsl:key name="nestwq2-exe-file-search" match="wix:Component[contains(wix:File/@Source, 'nestwq2.exe')]" use="@Id" />
  <xsl:key name="plct-bin-netcdf-dll-file-search" match="wix:Component[contains(wix:File/@Source, 'plct\bin\netcdf.dll')]" use="@Id" />
  <xsl:key name="restore_d3d_proc_lib-tcl-file-search" match="wix:Component[contains(wix:File/@Source, 'restore_d3d_proc_lib.tcl')]" use="@Id" />
  <xsl:key name="SIMETF-file-search" match="wix:Component[contains(wix:File/@Source, 'SIMETF')]" use="@Id" />
  <xsl:key name="SIMONA-ENV-file-search" match="wix:Component[contains(wix:File/@Source, 'SIMONA.ENV')]" use="@Id" />
  <xsl:key name="start_compiler-tcl-file-search" match="wix:Component[contains(wix:File/@Source, 'start_compiler.tcl')]" use="@Id" />
  <xsl:key name="ucrtbased-dll-file-search" match="wix:Component[contains(wix:File/@Source, 'ucrtbased.dll')]" use="@Id" />
  <xsl:key name="waq-gui-exe-file-search" match="wix:Component[contains(wix:File/@Source, 'waq_gui.exe')]" use="@Id" />


  <!-- When adding text to wxs no indenting is done, this line will nicely indent the new lines-->
  <xsl:template match="text()[normalize-space() = '']"/>
  
  <!-- Only install DeltaShell.Plugins.DelftModels.WaterQualityModel when FM with WAQ installer -->
  <xsl:template match="wix:Component[key('DeltaShell.Plugins.DelftModels.WaterQualityModel-folder-search', @Id)]" >
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <Condition><![CDATA[(INSTALLER_VARIANT <> "fm")]]></Condition>
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>

  <!-- Only install DeltaShell.Plugins.DelftModels.WaterQualityModel when FM with WAQ installer -->
  <xsl:template match="wix:Component[key('waqScripts-file-search', @Id)]" >
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <Condition><![CDATA[(INSTALLER_VARIANT <> "fm")]]></Condition>
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>

  <!-- Only install DeltaShell.Plugins.DelftModels.WaterQualityModel and kernels when FM with WAQ and not Open installer -->
  <xsl:template match="wix:Component[key('waqKernel-file-search', @Id)]" >
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <Condition><![CDATA[(INSTALLER_VARIANT <> "fm" AND INSTALLER_VARIANT <> "fmo" )]]></Condition>
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>


  <!-- Only install kernels when not FM open installer -->
  <xsl:template match="wix:Component[key('dimr-folder-search', @Id)]" >
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <Condition><![CDATA[(INSTALLER_VARIANT <> "fmo")]]></Condition>
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>
  <xsl:template match="wix:Component[key('RGFGridLibDir-folder-search', @Id)]" >
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <Condition><![CDATA[(INSTALLER_VARIANT <> "fmo")]]></Condition>
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>
  
  <!-- Remove Components referencing those directories & files. -->
  <xsl:template match="wix:Component[key('pdb-file-search', @File)]" />
  <xsl:template match="wix:Component[key('pspdb-file-search', @File)]" />
  <xsl:template match="wix:Component[key('pssym-file-search', @File)]" />

  <!--Remove ComponentRefs referencing those directories & files.-->
  <xsl:template match="wix:Component[key('pdb-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('pdb-file-search', @Id)]" />

  <xsl:template match="wix:Component[key('pspdb-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('pspdb-file-search', @Id)]" />

  <xsl:template match="wix:Component[key('pssym-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('pssym-file-search', @Id)]" />

  <!-- Remove Components referencing to-be-excluded PLCT files. -->
  <xsl:template match="wix:Component[key('coup203-exe-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('coup203-exe-file-search', @Id)]" />
  
  <xsl:template match="wix:Component[key('coupsds-exe-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('coupsds-exe-file-search', @Id)]" />

  <xsl:template match="wix:Component[key('install_open_proc_lib-tcl-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('install_open_proc_lib-tcl-file-search', @Id)]" />

  <xsl:template match="wix:Component[key('nestwq1-exe-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('nestwq1-exe-file-search', @Id)]" />

  <xsl:template match="wix:Component[key('nestwq2-exe-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('nestwq2-exe-file-search', @Id)]" />

  <xsl:template match="wix:Component[key('plct-bin-netcdf-dll-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('plct-bin-netcdf-dll-file-search', @Id)]" />

  <xsl:template match="wix:Component[key('restore_d3d_proc_lib-tcl-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('restore_d3d_proc_lib-tcl-file-search', @Id)]" />

  <xsl:template match="wix:Component[key('SIMETF-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('SIMETF-file-search', @Id)]" />

  <xsl:template match="wix:Component[key('SIMONA-ENV-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('SIMONA-ENV-file-search', @Id)]" />

  <xsl:template match="wix:Component[key('start_compiler-tcl-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('start_compiler-tcl-file-search', @Id)]" />

  <xsl:template match="wix:Component[key('ucrtbased-dll-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('ucrtbased-dll-file-search', @Id)]" />

  <xsl:template match="wix:Component[key('waq-gui-exe-file-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('waq-gui-exe-file-search', @Id)]" />


  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match='/'>
    <xsl:comment>*** DO NOT EDIT OR CHECK-IN: Generated by heat.exe; transformed by HarvestedFilesStandardTransform.xsl</xsl:comment>
    <xsl:apply-templates select="@*|node()"/>
  </xsl:template>

</xsl:stylesheet>

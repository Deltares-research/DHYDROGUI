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
  
   <!-- Create searches for the files to remove. -->
  <xsl:key name="pdb-file-search" match="wix:Component[contains(wix:File/@Source, '.pdb')]" use="@Id" />
  <xsl:key name="pspdb-file-search" match="wix:Component[contains(wix:File/@Source, '.pspdb')]" use="@Id" />
  <xsl:key name="pssym-file-search" match="wix:Component[contains(wix:File/@Source, '.pssym')]" use="@Id" />
  
  
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

  <!-- Replace file attribute id's -->
  <xsl:template match="wix:File[@Source='$(var.deltashell_gui_bin)\DeltaShell.Gui.exe.config']/@Id">
    <xsl:attribute name="{name()}">DeltaShell.Gui.exe.config</xsl:attribute>
  </xsl:template>
  <xsl:template match="wix:File[@Source='$(var.deltashell_gui_bin)\DeltaShell.Gui.exe']/@Id">
    <xsl:attribute name="{name()}">DeltaShell.Gui.exe</xsl:attribute>
  </xsl:template>
  <xsl:template match="wix:File[@Source='$(var.deltashell_gui_bin)\DeltaShell.Console.exe.config']/@Id">
    <xsl:attribute name="{name()}">DeltaShell.Console.exe.config</xsl:attribute>
  </xsl:template>
  <xsl:template match="wix:File[@Source='$(var.deltashell_gui_bin)\DeltaShell.Console.exe']/@Id">
    <xsl:attribute name="{name()}">DeltaShell.Console.exe</xsl:attribute>
  </xsl:template>
    
  <!-- identity template -->
  <xsl:key name="gui-file-search" match="wix:Component[contains(wix:File/@Source, 'DeltaShell.Gui.exe') and not(substring-after(wix:File/@Source, 'DeltaShell.Gui.exe'))]" use="@Id" />
  
  <!-- When adding text to wxs no indenting is done, this line will nicely indent the new lines-->
  <xsl:template match="text()[normalize-space() = '']"/>

  <xsl:template  match="wix:Component[key('gui-file-search', @Id)]">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <xsl:comment>Added by Peelen transform</xsl:comment>
      <xsl:processing-instruction name="if">$(var.Configuration) != "Release"</xsl:processing-instruction>
      <xsl:processing-instruction name="ifdef">env.BUILD_NUMBER</xsl:processing-instruction>
      <Shortcut
  			Id="ProgramMenuDeltaShellExeShortcut"
  			Icon="icon.ico"
  			Name="!(loc.FullProductName) ($(env.BUILD_NUMBER))"
  			Directory="ProgramGroupMenuDir"
  			Advertise="yes">
        <xsl:attribute name="WorkingDirectory">ProductDir</xsl:attribute>
      </Shortcut>
      <Shortcut
        Id="DesktopDeltaShellExeShortcut"
        Icon="icon.ico"
        Name="!(loc.FullProductName) ($(env.BUILD_NUMBER))"
        Directory="DesktopFolder"
        Advertise="yes"
        WorkingDirectory="LocalAppDataFolder" 
        />
      <xsl:processing-instruction name="else"/>
      <Shortcut
        Id="ProgramMenuDeltaShellExeShortcut"
        Icon="icon.ico"
        Name="!(loc.FullProductName)"
        Directory="ProgramGroupMenuDir"
        Advertise="yes">
        <xsl:attribute name="WorkingDirectory">ProductDir</xsl:attribute>
      </Shortcut>
      <Shortcut
        Id="DesktopDeltaShellExeShortcut"
        Icon="icon.ico"
        Name="!(loc.FullProductName)"
        Directory="DesktopFolder"
        Advertise="yes"
        WorkingDirectory="LocalAppDataFolder"
      />
      <xsl:processing-instruction name="endif"/>
      <xsl:processing-instruction name="else"/>
      <Shortcut
        Id="ProgramMenuDeltaShellExeShortcut"
        Icon="icon.ico"
        Name="!(loc.FullProductName)"
        Directory="ProgramGroupMenuDir"
        Advertise="yes">
        <xsl:attribute name="WorkingDirectory">ProductDir</xsl:attribute>
      </Shortcut>
      <Shortcut
        Id="DesktopDeltaShellExeShortcut"
        Icon="icon.ico"
        Name="!(loc.FullProductName)"
        Directory="DesktopFolder"
        Advertise="yes"
        WorkingDirectory="LocalAppDataFolder"
      />
      <xsl:processing-instruction name="endif"/>
      <!-- Capabilities keys for Vista/7 "Set Program Access and Defaults" -->
      <RegistryValue Root="HKLM" Key="SOFTWARE\[Manufacturer]\DeltaShell\Capabilities" Name="ApplicationDescription" Value="DeltaShell" Type="string" />
      <RegistryValue Root="HKLM" Key="SOFTWARE\[Manufacturer]\DeltaShell\Capabilities" Name="ApplicationIcon" Value="[BINDIR]DeltaShell.Gui.exe,0" Type="string" />
      <RegistryValue Root="HKLM" Key="SOFTWARE\[Manufacturer]\DeltaShell\Capabilities" Name="ApplicationName" Value="DeltaShell" Type="string" />
      <RegistryValue Root="HKLM" Key="SOFTWARE\[Manufacturer]\DeltaShell\Capabilities\DefaultIcon" Value="[BINDIR]DeltaShell.Gui.exe,1" Type="string" />
      <RegistryValue Root="HKLM" Key="SOFTWARE\[Manufacturer]\DeltaShell\Capabilities\FileAssociations" Name=".dsproj" Value="DeltaShell.ProjectFile" Type="string" />
      <RegistryValue Root="HKLM" Key="SOFTWARE\[Manufacturer]\DeltaShell\Capabilities\MIMEAssociations" Name="application/dsproj" Value="DeltaShell.ProjectFile" Type="string" />
      <RegistryValue Root="HKLM" Key="SOFTWARE\[Manufacturer]\DeltaShell\Capabilities\shell\Open\command" Value="&quot;[BINDIR]DeltaShell.Gui.exe&quot; &quot;%1&quot;" Type="string" />
      <RegistryValue Root="HKLM" Key="SOFTWARE\RegisteredApplications" Name="DeltaShell" Value="SOFTWARE\[Manufacturer]\DeltaShell\Capabilities" Type="string" />

      <!-- App Paths to support Start,Run -> "myapp" -->
      <RegistryValue Root="HKLM" Key="SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\DeltaShell.Gui.exe" Value="[BINDIR]DeltaShell.Gui.exe" Type="string" />
      <RegistryValue Root="HKLM" Key="SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\DeltaShell.Gui.exe" Name="Path" Value="[BINDIR]" Type="string" />

      <!-- Extend to the "open with" list + Win7 jump menu pinning  -->
      <RegistryValue Root="HKLM" Key="SOFTWARE\Classes\Applications\DeltaShell.Gui.exe\SupportedTypes" Name=".dsproj" Value="" Type="string" />
      <RegistryValue Root="HKLM" Key="SOFTWARE\Classes\Applications\DeltaShell.Gui.exe\shell\open" Name="FriendlyAppName" Value="DeltaShell" Type="string" />

      <!-- BATCH_PRIMARY_ASSOCIATION_DESC ProgID -->
      <RegistryValue Root="HKLM" Key="SOFTWARE\Classes\DeltaShell.ProjectFile" Name="FriendlyTypeName" Value="DeltaShell" Type="string" />
      <ProgId Id="DeltaShell.ProjectFile" Description="DeltaShell" Icon="icon.ico" Advertise="yes">
        <Extension Id="dsproj">
          <Verb Id="open" Argument="&quot;%1&quot;" />
          <MIME Advertise="yes" ContentType="application/dsproj" Default="yes" />
        </Extension>
      </ProgId>
      <RegistryValue Root="HKCR" Key=".dsproj" Action="write" Type="string" Value="DeltaShell.dsproj" />
      <RegistryValue Root="HKCR" Key="DeltaShell.dsproj" Action="write" Type="string" Value="DeltaShell data project file" />
      <RegistryValue Root="HKCR" Key="DeltaShell.dsproj\DefaultIcon" Action="write" Type="string" Value="[BINDIR]DeltaShell.Gui.exe,0" />
    </xsl:copy>

  </xsl:template>
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>
 
  <xsl:template match='/'>
    <xsl:comment>*** DO NOT EDIT OR CHECK-IN: Generated by heat.exe; transformed by HarvestedFilesTransform.xsl</xsl:comment>
    <xsl:apply-templates select="@*|node()"/>
  </xsl:template>

</xsl:stylesheet>

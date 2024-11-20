<?xml version="1.0" ?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">

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
			<xsl:processing-instruction name="include">DimrDefines.wxi</xsl:processing-instruction> 
			<xsl:apply-templates/> 
		</xsl:copy> 
	</xsl:template> 

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()" />
		</xsl:copy>	
	</xsl:template>

	<xsl:output method="xml" indent="yes" />

	<!-- Create searches for the directories to remove. -->
	<xsl:key name="dflowfm-folder-search" match="wix:Directory[@Name = 'dflowfm']" use="descendant::wix:Component/@Id" />
	<xsl:key name="swan-folder-search" match="wix:Directory[@Name = 'swan']" use="descendant::wix:Component/@Id" />
	<xsl:key name="esmf-folder-search" match="wix:Directory[@Name = 'esmf']" use="descendant::wix:Component/@Id" />
	<xsl:key name="wave-folder-search" match="wix:Directory[@Name = 'dwaves']" use="descendant::wix:Component/@Id" />
	<xsl:key name="rr-folder-search" match="wix:Directory[@Name = 'drr']" use="descendant::wix:Component/@Id" />
	<xsl:key name="f1d-folder-search" match="wix:Directory[@Name = 'dflow1d']" use="descendant::wix:Component/@Id" />
	<xsl:key name="f1d2d-folder-search" match="wix:Directory[@Name = 'dflow1d2d']" use="descendant::wix:Component/@Id" />
	<xsl:key name="drtc-folder-search" match="wix:Directory[@Name = 'drtc']" use="descendant::wix:Component/@Id" />
	<xsl:key name="dimr-folder-search" match="wix:Directory[@Name = 'dimr']" use="descendant::wix:Component/@Id" />
	<xsl:key name="dwaq-folder-search" match="wix:Directory[@Name = 'dwaq']" use="descendant::wix:Component/@Id" />
	<xsl:key name="scripts-folder-search" match="wix:Directory[@Name = 'scripts']" use="descendant::wix:Component/@Id" />
	<xsl:key name="share-folder-search" match="wix:Directory[@Name = 'share']" use="descendant::wix:Component/@Id" />

	
	<!-- Remove directories. -->
	<xsl:template match="wix:Directory[@Name='dflowfm']" />
	<xsl:template match="wix:Directory[@Name='swan']" />
	<xsl:template match="wix:Directory[@Name='esmf']" />
	<xsl:template match="wix:Directory[@Name='dwaves']" />
	<xsl:template match="wix:Directory[@Name='drr']" />
	<xsl:template match="wix:Directory[@Name='dflow1d']" />
	<xsl:template match="wix:Directory[@Name='dflow1d2d']" />
	<xsl:template match="wix:Directory[@Name='drtc']" />
	<xsl:template match="wix:Directory[@Name='dimr']" />
	<xsl:template match="wix:Directory[@Name='dwaq']" />
	<xsl:template match="wix:Directory[@Name='scripts']" />
	<xsl:template match="wix:Directory[@Name='share']" />

	<!-- Remove Components referencing those directories & files. -->
	<xsl:template match="wix:Component[key('dflowfm-folder-search', @Directory)]" />
	<xsl:template match="wix:Component[key('swan-folder-search', @Directory)]" />
	<xsl:template match="wix:Component[key('esmf-folder-search', @Directory)]" />
	<xsl:template match="wix:Component[key('wave-folder-search', @Directory)]" />
	<xsl:template match="wix:Component[key('rr-folder-search', @Directory)]" />
	<xsl:template match="wix:Component[key('f1d-folder-search', @Directory)]" />
	<xsl:template match="wix:Component[key('f1d2d-folder-search', @Directory)]" />
	<xsl:template match="wix:Component[key('drtc-folder-search', @Directory)]" />
	<xsl:template match="wix:Component[key('dimr-folder-search', @Directory)]" />
	<xsl:template match="wix:Component[key('dwaq-folder-search', @Directory)]" />
	<xsl:template match="wix:Component[key('scripts-folder-search', @Directory)]" />
	<xsl:template match="wix:Component[key('share-folder-search', @Directory)]" />


	<!-- Remove DirectoryRefs (and their parent Fragments) referencing those directories. -->
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('dflowfm-folder-search', @Id)]]" />
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('swan-folder-search', @Id)]]" />
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('esmf-folder-search', @Id)]]" />
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('wave-folder-search', @Id)]]" />
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('rr-folder-search', @Id)]]" />
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('f1d-folder-search', @Id)]]" />
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('f1d2d-folder-search', @Id)]]" />
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('drtc-folder-search', @Id)]]" />
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('dimr-folder-search', @Id)]]" />
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('dwaq-folder-search', @Id)]]" />
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('scripts-folder-search', @Id)]]" />
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('share-folder-search', @Id)]]" />

	<!--Remove ComponentRefs referencing those directories & files.-->
	<xsl:template match="wix:ComponentRef[key('dflowfm-folder-search', @Id)]" />
	<xsl:template match="wix:ComponentRef[key('swan-folder-search', @Id)]" />
	<xsl:template match="wix:ComponentRef[key('esmf-folder-search', @Id)]" />
	<xsl:template match="wix:ComponentRef[key('wave-folder-search', @Id)]" />
	<xsl:template match="wix:ComponentRef[key('rr-folder-search', @Id)]" />
	<xsl:template match="wix:ComponentRef[key('f1d-folder-search', @Id)]" />
	<xsl:template match="wix:ComponentRef[key('f1d2d-folder-search', @Id)]" />
	<xsl:template match="wix:ComponentRef[key('drtc-folder-search', @Id)]" />
	<xsl:template match="wix:ComponentRef[key('dimr-folder-search', @Id)]" />
	<xsl:template match="wix:ComponentRef[key('dwaq-folder-search', @Id)]" />
	<xsl:template match="wix:ComponentRef[key('scripts-folder-search', @Id)]" />
	<xsl:template match="wix:ComponentRef[key('share-folder-search', @Id)]" />
		
</xsl:stylesheet>

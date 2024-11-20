<?xml version="1.0" ?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"	
    xmlns="http://schemas.microsoft.com/wix/2006/wi"
  exclude-result-prefixes="wix">

	<xsl:output method="xml" indent="yes" />


	<!-- Copy all attributes and elements to the output. -->
	<xsl:template match="@*|*">
		<xsl:copy>
			<xsl:apply-templates select="@*" />
			<xsl:apply-templates select="*" />
		</xsl:copy>
	</xsl:template>

	<xsl:output method="xml" indent="yes" />
	
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
	<xsl:key name="rr-folder-search" match="wix:Directory[@Name = 'drr']" use="descendant::wix:Component/@Id" />
	<xsl:key name="f1d-folder-search" match="wix:Directory[@Name = 'dflow1d']" use="descendant::wix:Component/@Id" />
	<xsl:key name="f1d2d-folder-search" match="wix:Directory[@Name = 'dflow1d2d']" use="descendant::wix:Component/@Id" />

	<!-- Remove directories. -->
	<xsl:template match="wix:Directory[@Name='drr']" />
	<xsl:template match="wix:Directory[@Name='dflow1d']" />
	<xsl:template match="wix:Directory[@Name='dflow1d2d']" />

	<!-- Remove Components referencing those directories & files. -->
	<xsl:template match="wix:Component[key('rr-folder-search', @Directory)]" />
	<xsl:template match="wix:Component[key('f1d-folder-search', @Directory)]" />
	<xsl:template match="wix:Component[key('f1d2d-folder-search', @Directory)]" />

	<!-- Remove DirectoryRefs (and their parent Fragments) referencing those directories. -->
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('rr-folder-search', @Id)]]" />
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('f1d-folder-search', @Id)]]" />
	<xsl:template match="wix:Fragment[wix:DirectoryRef[key('f1d2d-folder-search', @Id)]]" />

	<!--Remove ComponentRefs referencing those directories & files.-->
	<xsl:template match="wix:ComponentRef[key('rr-folder-search', @Id)]" />
	<xsl:template match="wix:ComponentRef[key('f1d-folder-search', @Id)]" />
	<xsl:template match="wix:ComponentRef[key('f1d2d-folder-search', @Id)]" />
</xsl:stylesheet>

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
			<xsl:processing-instruction name="include">RGFGridDefines.wxi</xsl:processing-instruction> 
			<xsl:apply-templates/> 
		</xsl:copy> 
	</xsl:template> 

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()" />
		</xsl:copy>	
	</xsl:template>

	<xsl:output method="xml" indent="yes" />
</xsl:stylesheet>

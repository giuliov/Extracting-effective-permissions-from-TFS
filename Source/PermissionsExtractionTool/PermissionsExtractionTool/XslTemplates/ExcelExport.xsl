<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" >
    <xsl:output method="xml" encoding="utf-8"/>
    <xsl:template match="/">
        <xsl:processing-instruction name="mso-application">progid="Excel.Sheet"</xsl:processing-instruction>
        <Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet"
         xmlns:o="urn:schemas-microsoft-com:office:office"
         xmlns:x="urn:schemas-microsoft-com:office:excel"
         xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
         xmlns:html="http://www.w3.org/TR/REC-html40">
            <Styles>
                <Style ss:ID="Default" ss:Name="Normal">
                    <Alignment ss:Vertical="Bottom"/>
                    <Borders/>
                    <Font ss:FontName="Calibri" x:Family="Swiss" ss:Size="11" ss:Color="#000000"/>
                    <Interior/>
                    <NumberFormat/>
                    <Protection/>
                </Style>
                <Style ss:ID="s27">
                    <Alignment ss:Vertical="Center"/>
                    <Font ss:FontName="Segoe UI" x:Family="Swiss" ss:Size="20" ss:Color="#000000"/>
                    <Interior ss:Color="#FFFFFF" ss:Pattern="Solid"/>
                </Style>
                <Style ss:ID="s76">
                    <Alignment ss:Vertical="Bottom"/>
                    <Font ss:FontName="Calibri" x:Family="Swiss" ss:Size="14" ss:Color="#000000"
                     ss:Bold="1" ss:Italic="1"/>
                </Style>
                <Style ss:ID="s78">
                    <Alignment ss:Vertical="Center"/>
                    <Font ss:FontName="Segoe UI" x:Family="Swiss" ss:Size="12" ss:Color="#000000"/>
                    <Interior ss:Color="#EEEEEE" ss:Pattern="Solid"/>
                </Style>
                <Style ss:ID="s79">
                    <Alignment ss:Vertical="Center"/>
                    <Font ss:FontName="Segoe UI" x:Family="Swiss" ss:Size="12" ss:Color="#000000"/>
                    <Interior ss:Color="#D9D9D9" ss:Pattern="Solid"/>
                </Style>
            </Styles>
                <Worksheet ss:Name="TFS Project Access Rights">
                <Table x:FullColumns="1" x:FullRows="1">
                    <Row>
                        <Cell ss:StyleID="s27">
                            <Data ss:Type="String">TFS Git Permissions Report</Data>
                        </Cell>
                        <Cell ss:StyleID="s27">
                            <Data ss:Type="String"><xsl:value-of select="//PermissionsReportByProject/Date"/></Data>
                        </Cell>
                        <Cell ss:StyleID="s27">
                            <Data ss:Type="String">TFS Collection</Data>
                        </Cell>
                        <Cell ss:StyleID="s27">
                            <Data ss:Type="String"><xsl:value-of select="//PermissionsReportByProject/TfsCollectionUrl"/></Data>
                        </Cell>
                    </Row>
                    <xsl:for-each select="//PermissionsReportByProject/TeamProjects/TeamProject">

                        <!-- Git Version Control Permissions-->
                        <xsl:for-each select="GitVersionControlPermissions/VersionControlPermissionsList/GitRepoPermission">
                            <Row>
                                <Cell ss:StyleID="s76">
                                    <Data ss:Type="String"><xsl:value-of select="RepoName"/></Data>
                                </Cell>
                            </Row>

                                    <xsl:for-each select="RepoPermissionsByUser/UserPermissions">
                                        <xsl:sort select="UserName"/>
                                        <Row>
                                            <Cell ss:StyleID="s78">
                                                <Data ss:Type="String"><xsl:if test="IsUser"><xsl:value-of select="UserName"/></xsl:if><xsl:if test="not(IsUser)">***<xsl:value-of select="DisplayName"/>***</xsl:if></Data>
                                            </Cell>
                                            <Cell ss:StyleID="s78">
                                                <Data ss:Type="String"><xsl:if test="IsUser"><xsl:value-of select="DisplayName"/></xsl:if></Data>
                                            </Cell>
                                            <Cell ss:StyleID="s79">
                                                <Data ss:Type="String"><xsl:value-of select="Permissions"/></Data>
                                            </Cell>
                                        </Row>

                                    </xsl:for-each>
                        </xsl:for-each>

                    </xsl:for-each>
                </Table>
            </Worksheet>
        </Workbook>
    </xsl:template>
</xsl:stylesheet>
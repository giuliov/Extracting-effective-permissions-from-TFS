<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" >
    <xsl:output method="html"  encoding="utf-16"/>
    <xsl:template match="/">
        <head>
            <title>TFS Permissions Report</title>
            <style type="text/css">
                body{ text-align: left; width: 95%;  font-family: Calibri, sans-serif; }

                table{ margin-left:60px; border: none;  border-collapse: separate;  width: 90%; }

                tr.title td{ background: white;font-size: 26px;  font-weight: bold; }

                th{ background: #d0d0d0;  font-weight: bold;  font-size: 16pt;  text-align: left; }
                tr{ background: #eeeeee}
                td, th{
                font-family:'Segoe UI'; font-weight:lighter;
                font-size: 12pt;  padding: 2px;  border: none; }
                h1 {
                margin-top:10px;
                font-size:xx-large;
                font-weight:lighter;
                font-family:'Segoe UI';
                }

                .ProjectsHeader {

                }
                span.tab{
                padding: 0 10px; /* Or desired space*/
                }
                tr.info td{}
                tr.warning td{background-color:yellow;color:black}
                tr.error td{background-color:red;color:black}

                a:hover{text-transform:uppercase;color: #9090F0;}
            </style>
        </head>

        <body>
            <table>
                <tr class="title">
                    <td>
                        <img style="float:left; margin-right:0px; margin-bottom:0px" src="ALMRangers_Logo.png" alt="" title="Home" />
                    </td>
                    <td colspan="7">
                        <h1 style="margin-left:60px">TFS Git Permissions Report</h1>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">Date</td>
                    <td colspan="5">
                        <xsl:value-of select="//PermissionsReportByProject/Date"/>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">TFS Collection</td>
                    <td colspan="5">
                        <xsl:value-of select="//PermissionsReportByProject/TfsCollectionUrl"/>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">Team Project</td>
                    <td colspan="5">
                        <xsl:value-of select="//PermissionsReportByProject/TeamProjects/TeamProject/Name"/>
                    </td>
                </tr>


            </table>
            <xsl:for-each select="//PermissionsReportByProject/TeamProjects/TeamProject">

                <!-- Git Version Control Permissions-->
                <xsl:for-each select="GitVersionControlPermissions/VersionControlPermissionsList/GitRepoPermission">
                    <tr>
                        <td >Git Version Control Permissions</td>
                        <td>
                            <b>
                                <i>
                                    <xsl:value-of select="RepoName"/>
                                </i>
                            </b>
                        </td>
                    </tr>
                    <tr>
                        <table>
                            <xsl:for-each select="RepoPermissionsByUser/UserPermissions">
                                <xsl:sort select="UserName"/>
                                <tr>
                                    <td>
                                        <xsl:if test="IsUser">
                                            <xsl:value-of select="UserName"/> (<xsl:value-of select="DisplayName"/>)
                                        </xsl:if>
                                        <xsl:if test="not(IsUser)">
                                            <b>
                                                ***<xsl:value-of select="DisplayName"/>***
                                            </b>
                                        </xsl:if>
                                    </td>
                                    <td>
                                        <xsl:value-of select="Permissions"/>
                                        <!--
                                            <xsl:for-each select="Permissions/Permission">
                                                <table>
                                                    <tr>
                                                        <td>
                                                            <xsl:value-of select="DisplayName"/>
                                                        </td>
                                                        <td>
                                                            <xsl:value-of select="Value"/>
                                                            <span class="tab">
                                                                <xsl:value-of select="GroupMemberInheritance"/>
                                                            </span>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </xsl:for-each>
                                            -->
                                    </td>
                                </tr>
                            </xsl:for-each>
                        </table>
                    </tr>
                </xsl:for-each>

            </xsl:for-each>
        </body>
    </xsl:template>
</xsl:stylesheet>
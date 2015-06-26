// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//   Microsoft Visual Studio ALM Rangers
// </copyright>
// <summary>
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Microsoft.ALMRangers.PermissionsExtractionTool
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using CommandLine;
    using Microsoft.ALMRangers.PermissionsExtractionTool.Properties;
    using TeamFoundation.Client;
    using TeamFoundation.Framework.Client;
    using TeamFoundation.Framework.Common;
    using TeamFoundation.VersionControl.Client;
    using TeamFoundation.WorkItemTracking.Client;
    using XmlTransformation;

    /// <summary>
    /// The program.
    /// </summary>
    internal class PivotByProject : PivotAlgorithm
    {
        /// <summary>
        /// Sends a failure message of the ERROR output and wait for a press on the enter key.
        /// </summary>
        /// <param name="message">message about the failure</param>
        private static void Fail(string message = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.Error.WriteLine(message);
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public override bool Run(Options options, TfsTeamProjectCollection tfs)
        {
            bool someExtractionFail = false;

            // Getting Identity Service
            var ims = tfs.GetService<IIdentityManagementService>();
            var identityManagementService = ims;
            // get workItem store
            var workItemStore = tfs.GetService<WorkItemStore>();

            ////Initiate Report

            try
            {
                // retrieve list of Team Projects in the given collection
                ProjectCollection workItemsProjects = workItemStore.Projects;
                foreach (Project teamProject in workItemsProjects)
                {
                    if (options.Projects != null)
                    {
                        if (!options.Projects.Contains(teamProject.Name))
                        {
                            Console.WriteLine(" ...skipping Team Project: {0}", teamProject.Name);
                            continue;
                        }
                    }

                    // Initiate a new object of Permission Report
                    var permissionsReport = new PermissionsReportByProject
                    {
                        Date = DateTime.Now,
                        TfsCollectionUrl = options.Collection,
                        TeamProjects = new List<TfsTeamProject>()
                    };

                    // Create project security token
                    string projectSecurityToken = "$PROJECT:" + teamProject.Uri.AbsoluteUri;

                    // Project Permissions
                    var server = tfs.GetService<ISecurityService>();
                    TeamFoundation.Git.Client.GitRepositoryService gitRepositoryService = tfs.GetService<TeamFoundation.Git.Client.GitRepositoryService>();

                    Console.WriteLine("==== Extracting Permissions for {0} Team Project ====", teamProject.Name);




                    string collectionName = workItemStore.TeamProjectCollection.Name.Split('\\')[1];
                    var allProjectUsers = GetProjectAllUsers(collectionName, teamProject.Name, options.Users, ims);

                    Console.WriteLine("== Extract Git Version Control Permissions ==");
                    var x = new List<GitRepoPermission>();

                    SecurityNamespace gitVersionControlSecurityNamespace = server.GetSecurityNamespace(Helpers.GetSecurityNamespaceId(PermissionScope.GitSourceControl, server));

                    var gitProjectRepoService = gitRepositoryService.QueryRepositories(teamProject.Name);
                    var result = new List<GitRepoPermission>();
                    foreach (var gitRepo in gitProjectRepoService)
                    {
                        var perms = new GitRepoPermission()
                        {
                            RepoName = gitRepo.Name,
                            RepoPermissionsByUser = new List<UserPermissions>()
                        };
                        var repoIdToken = string.Format("repoV2{0}{1}{2}{3}", gitVersionControlSecurityNamespace.Description.SeparatorValue, gitRepo.ProjectReference.Id, gitVersionControlSecurityNamespace.Description.SeparatorValue, gitRepo.Id);
                        foreach (var user in allProjectUsers)
                        {
                            int mask = gitVersionControlSecurityNamespace.QueryEffectivePermissions(repoIdToken, user.Descriptor);
                            string allowed = ((EnumrationsList.GitPermissions)mask).ToString();
                            perms.RepoPermissionsByUser.Add(new UserPermissions()
                            {
                                IsActive = user.IsActive,
                                IsUser = !user.IsContainer,
                                DisplayName = user.DisplayName,
                                UserName = user.UniqueName,
                                Permissions = allowed
                            });
                        }
                        result.Add(perms);
                        Console.Write(".");
                    }
                    Console.WriteLine("done.");





                    // Version Control Permissions
                    var gitVersionControlPermissions = result;

                    // Set TFS report Data
                    // Create Team Project node in XML file
                    var tfsTeamProject = new TfsTeamProject
                    {
                        Name = teamProject.Name,
                        AreaPermissions = null,
                        BuildPermissions = null,
                        IterationPermissions = null,
                        ProjectLevelPermissions =
                            new ProjectLevelPermissions
                            {
                                ProjectLevelPermissionsList
                                    =
                                    null
                            },
                        GitVersionControlPermissions = new GitVersionControlPermissions
                        {
                            VersionControlPermissionsList = gitVersionControlPermissions
                        },
                        TfvcVersionControlPermissions = new TfvcVersionControlPermissions
                        {
                            VersionControlPermissionsList = null
                        }
                    };
                    permissionsReport.TeamProjects.Add(tfsTeamProject);

                    var fileName = Helpers.GenerateFileName(teamProject.Name, options.OutputFile);

                    // Generate output file
                    FileInfo fi = new FileInfo(fileName);
                    if (!Directory.Exists(fi.DirectoryName))
                    {
                        Console.Write("Creating Output Directory {0}", fi.DirectoryName);
                        Directory.CreateDirectory(fi.DirectoryName);
                    }

                    var fs = new FileStream(fileName, FileMode.Create);
                    var streamWriter = new StreamWriter(fs, Encoding.UTF8);

                    using (streamWriter)
                    {
                        var xmlSerializer = new XmlSerializer(typeof(PermissionsReportByProject));
                        xmlSerializer.Serialize(streamWriter, permissionsReport);
                        streamWriter.Flush();
                    }

                    if (options.Html)
                    {
                        var tranformationFileName = Path.Combine(Path.GetDirectoryName(fileName), "ALMRanger.xsl");
                        File.WriteAllText(tranformationFileName, Resources.ALMRangers_SampleXslt2);
                        var htmlOuput = Path.ChangeExtension(fileName, ".html");

                        var logoFile = Path.Combine(Path.GetDirectoryName(fileName), "ALMRangers_Logo.png");
                        Resources.ALMRangers_Logo.Save(logoFile);
                        XmlTransformationManager.TransformXmlUsingXsl(fileName, tranformationFileName, htmlOuput);
                    }

                }//for

                return true;
            }
            catch (TeamFoundationServiceException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private List<TeamFoundationIdentity> GetProjectAllUsers(string collectionName, string projectName, string[] usersToSkip, IIdentityManagementService identityManagementService)
        {
            var queue = new Queue<TeamFoundationIdentity>();
            var flatUserList = new Dictionary<Guid, TeamFoundationIdentity>();

            TeamFoundationIdentity allProjectUsers = identityManagementService.ReadIdentity(IdentitySearchFactor.General, string.Format("[{0}]\\Project Valid Users", projectName), MembershipQuery.Expanded, ReadIdentityOptions.IncludeReadFromSource);
            queue.Enqueue(allProjectUsers);
            TeamFoundationIdentity collectionAdmins = identityManagementService.ReadIdentity(IdentitySearchFactor.General, string.Format("[{0}]\\Project Collection Administrators", collectionName), MembershipQuery.Expanded, ReadIdentityOptions.IncludeReadFromSource);
            queue.Enqueue(collectionAdmins);
            TeamFoundationIdentity serverAdmins = identityManagementService.ReadIdentity(IdentitySearchFactor.General, "[TEAM FOUNDATION]\\Team Foundation Administrators", MembershipQuery.Expanded, ReadIdentityOptions.IncludeReadFromSource);
            queue.Enqueue(serverAdmins);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (cur.IsContainer)
                {
                    System.Diagnostics.Debug.WriteLine("examining group {0}", cur.DisplayName, 0);
                    //expand
                    foreach (var member in cur.Members)
                    {
                        var memberId = identityManagementService.ReadIdentity(IdentitySearchFactor.Identifier, member.Identifier, MembershipQuery.Expanded, ReadIdentityOptions.IncludeReadFromSource);
                        System.Diagnostics.Debug.WriteLine("  queuing {0}", memberId.DisplayName, 0);
                        queue.Enqueue(memberId);
                    }
                }
                else
                {
                    if (!flatUserList.ContainsKey(cur.TeamFoundationId)
                        && !usersToSkip.Contains(cur.UniqueName, StringComparer.InvariantCultureIgnoreCase))
                    {
                        System.Diagnostics.Debug.WriteLine("Adding {0}", cur.DisplayName, 0);
                        flatUserList.Add(cur.TeamFoundationId, cur);
                    }
                }
            }//while

            return flatUserList.Values.ToList();
        }

        /// <summary>
        /// Returns the security group of the user
        /// </summary>
        /// <param name="tpc">Team project collection</param>
        /// <param name="teamProjectUri">Team Project Uri</param>
        /// <param name="userIdentity">User identity</param>
        /// <returns>The groups where the user is present.</returns>
        private static List<string> GetUserGroups(TfsTeamProjectCollection tpc, string teamProjectUri, TeamFoundationIdentity userIdentity)
        {
            Console.WriteLine("  -- Getting UserGroups...");
            List<string> groups = new List<string>();

            // Get the group security service.
            var gss = tpc.GetService<Microsoft.TeamFoundation.Server.IGroupSecurityService2>();

            Microsoft.TeamFoundation.Server.Identity[] appGroups = gss.ListApplicationGroups(teamProjectUri);

            foreach (Microsoft.TeamFoundation.Server.Identity group in appGroups)
            {
                Microsoft.TeamFoundation.Server.Identity[] groupMembers = gss.ReadIdentities(Microsoft.TeamFoundation.Server.SearchFactor.Sid, new[] { group.Sid }, Microsoft.TeamFoundation.Server.QueryMembership.Expanded);
                foreach (Microsoft.TeamFoundation.Server.Identity member in groupMembers)
                {
                    if (member.Members != null)
                    {
                        groups.AddRange(from memberSid in member.Members where memberSid == userIdentity.Descriptor.Identifier select @group.Sid);
                    }
                }
            }

            return groups;
        }
    }
}

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AreaPermission.cs" company="Microsoft Corporation">
//   Microsoft Visual Studio ALM Rangers
// </copyright>
// <summary>
//   The area permission.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.ALMRangers.PermissionsExtractionTool
{
    using System.Collections.Generic;

    /// <summary>
    /// The Git permission.
    /// </summary>
    public class GitRepoPermission
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the repository name.
        /// </summary>
        public string RepoName { get; set; }

        /// <summary>
        /// Gets or sets the repository permissions.
        /// </summary>
        public List<Permission> RepoPermissions { get; set; }

        /// <summary>
        /// Gets or sets the repository permissions.
        /// </summary>
        public List<UserPermissions> RepoPermissionsByUser { get; set; }

        #endregion
    }
}
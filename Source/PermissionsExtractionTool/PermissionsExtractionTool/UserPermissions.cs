using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.ALMRangers.PermissionsExtractionTool
{
    public class UserPermissions
    {
        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public bool IsUser { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the user Display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the repository permissions.
        /// </summary>
        //public List<Permission> Permissions { get; set; }
        public string Permissions { get; set; }
    }
}

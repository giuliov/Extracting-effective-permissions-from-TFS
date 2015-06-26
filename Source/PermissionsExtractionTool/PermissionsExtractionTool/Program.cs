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


    internal abstract class PivotAlgorithm
    {
        public abstract bool Run(Options options, TfsTeamProjectCollection tfs);
    }

    /// <summary>
    /// The program.
    /// </summary>
    internal static class Program
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

        /// <summary>
        /// Console entry point
        /// </summary>
        /// <param name="args">command line arguments</param>
        /// <returns>0 if all extractions are successful.</returns>
        private static int Main(string[] args)
        {
            var options = new Options();

            var parser = new Parser(settings =>
            {
                settings.CaseSensitive = false;
                settings.HelpWriter = Console.Error;
                settings.ParsingCulture = CultureInfo.InvariantCulture;
            });

            var result = parser.ParseArguments(args, options);

            if (!result)
            {
                Fail();
                return -1;
            }
            // ad-hoc check as 1.9 has no decent command management
            if (!options.ByUser && !options.ByProject)
            {
                Fail();
                return -1;
            }

            TfsTeamProjectCollection tfs =
                TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(options.Collection));
            try
            {
                tfs.EnsureAuthenticated();
            }
            catch (Exception)
            {
                Fail("Connection to TFS failed");
                return -1;
            }

            bool someExtractionFail = false;

            PivotAlgorithm report = null;
            if (options.ByUser)
            {
                report = new PivotByUser();
            }
            if (options.ByProject)
            {
                report = new PivotByProject();
            }
            someExtractionFail = report.Run(options, tfs);

            if (someExtractionFail)
            {
                Fail("An error occurred during the extraction");
                return -1;
            }

            return 0;
        }
    }
}

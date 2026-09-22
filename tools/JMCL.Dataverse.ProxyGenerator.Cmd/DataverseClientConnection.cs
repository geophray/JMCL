// Modernized from XrmToolingConnection.cs (credit: Scott Durrow, GitHub/scottdurrow/SparkleXrm),
// which built a Microsoft.Xrm.Tooling.Connector.CrmServiceClient over ADAL. This
// rewrite targets Microsoft.PowerPlatform.Dataverse.Client.ServiceClient (MSAL)
// instead -- see fork-plan.md 11.1a/D15. The connection-string grammar
// (AuthType=OAuth;Url=...;AppId=...;RedirectUri=...;LoginPrompt=...;TokenCacheStorePath=...)
// and the saved-connection UX are unchanged; only the underlying client type and
// auth library changed.

using Microsoft.PowerPlatform.Dataverse.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace JMCL.Dataverse.ProxyGenerator.Cmd
{
    public class DataverseClientConnection
    {
        /// <summary>
        /// Connection configurations - no credentials or tokens are stored here.
        /// </summary>
        public static readonly string ConnectionsFilePath = Path.Combine(
             Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "jmcl-proxygen"),
             "Connections.json");

        /// <summary>
        /// Token cache path, so the user is not prompted to log in on every run
        /// where a cached, still-valid token exists.
        /// </summary>
        public static readonly string TokenPath = Path.Combine(
             Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "jmcl-proxygen"),
             "TokenCache");

        public string ConnectionString { get; internal set; }
        private List<SavedConnection> _savedConnections = new List<SavedConnection>();

        public DataverseClientConnection()
        {
            LoadSettings();
        }

        public ServiceClient Connect()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Office365 Login");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Note:  pass an explicit connection string via -c for on-premises, AD, or a non-interactive login");
            Console.ForegroundColor = ConsoleColor.Gray;

            // Microsoft's published public-client sample app registration, the same one
            // documented for ServiceClient's own interactive-login samples. It has no
            // client secret and works across tenants; pass -c with your own AppId in the
            // connection string if a project needs its own app registration instead.
            var connectionStringRoot = $@"AuthType=OAuth;
                      AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;
                      RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;
                      TokenCacheStorePath={DataverseClientConnection.TokenPath}";

            // Show list of saved connections. Credentials are never stored here -- only
            // the environment URL and display metadata -- the ServiceClient token cache
            // above is what actually avoids re-prompting for a login.
            var configNumber = -1;
            if (_savedConnections.Count > 0)
            {
                var i = 1;
                Console.WriteLine("(0) Add New Server Configuration");
                foreach (SavedConnection connection in _savedConnections)
                {
                    Console.WriteLine($"({i}) {connection.EnvironmentUrl},  {connection.DisplayName},  {connection.UserName}");
                    i++;
                }

                Console.Write($"\nSpecify the saved server configuration number (0-{_savedConnections.Count}) [{_savedConnections.Count}] : ");
                var input = Console.ReadLine();
                Console.WriteLine();
                if (input == string.Empty)
                {
                    input = _savedConnections.Count.ToString();
                }

                if (!int.TryParse(input, out configNumber))
                {
                    configNumber = -1;
                }
            }

            SavedConnection selectedConnection;
            var loginPrompt = "Auto";
            var newConnection = configNumber <= 0;
            if (newConnection)
            {
                selectedConnection = new SavedConnection();
                var envUrl = ReadLine("Environment/Organization Url (e.g. org123.crm.dynamics.com)", regex: @"^(?<!http)([^\s:\/]+)(\.crm[0-9]*\.dynamics\.com[\/]?)$");
                envUrl = envUrl.TrimEnd('/');
                selectedConnection.EnvironmentUrl = envUrl;
                // If new connection - set LoginPrompt=Always
                loginPrompt = "Always";
            }
            else
            {
                selectedConnection = _savedConnections[configNumber - 1];
                // Move the saved connection to the end so it's default next time
                _savedConnections.Remove(selectedConnection);
                _savedConnections.Add(selectedConnection);
                SaveSettings();
                connectionStringRoot += $";UserName={selectedConnection.UserName}";
            }

            var connectionString = $@"{connectionStringRoot};Url=https://{selectedConnection.EnvironmentUrl};LoginPrompt={loginPrompt}";
            var client = new ServiceClient(connectionString);
            if (client.IsReady)
            {
                if (newConnection)
                {
                    selectedConnection.EnvironmentUrl = client.ConnectedOrgUriActual.Host;
                    selectedConnection.UserName = client.OAuthUserId;
                    selectedConnection.DisplayName = client.ConnectedOrgFriendlyName;
                    connectionString += $";UserName={selectedConnection.UserName}";
                    // Is this replacing an existing connection?
                    _savedConnections.RemoveAll(c => c.EnvironmentUrl.Equals(selectedConnection.EnvironmentUrl, StringComparison.InvariantCultureIgnoreCase) &&
                        c.UserName.Equals(selectedConnection.UserName, StringComparison.InvariantCultureIgnoreCase));
                    _savedConnections.Add(selectedConnection);
                    SaveSettings();
                }
                this.ConnectionString = connectionString;
                return client;
            }
            else
            {
                throw new Exception($"Cannot connect: {client.LastError}", client.LastException);
            }
        }

        private void LoadSettings()
        {
            if (File.Exists(DataverseClientConnection.ConnectionsFilePath))
            {
                string configJson = File.ReadAllText(DataverseClientConnection.ConnectionsFilePath);
                _savedConnections = JsonConvert.DeserializeObject<List<SavedConnection>>(configJson);
            }
        }

        private void SaveSettings()
        {
            var directory = Path.GetDirectoryName(DataverseClientConnection.ConnectionsFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string configJson = JsonConvert.SerializeObject(_savedConnections);
            File.WriteAllText(DataverseClientConnection.ConnectionsFilePath, configJson);
        }

        private string ReadLine(string prompt, string regex = null, string defaultValue = null)
        {
            if (defaultValue != null)
            {
                prompt += $"[{defaultValue}]";
            }

            bool isValid = true;
            string returnedValue = defaultValue;
            do
            {
                Console.Write(prompt + ": ");
                string input = Console.ReadLine();
                if (input.Length > 0)
                {
                    if (regex != null && Regex.IsMatch(input, regex))
                    {
                        returnedValue = input;
                        isValid = true;
                    }
                    else
                    {
                        Console.WriteLine("\nInput invalid");
                        isValid = false;
                    }
                }
                else if (defaultValue == null)
                {
                    Console.WriteLine("\nInput Required");
                    isValid = false;
                }
            } while (!isValid);
            Console.Write("\n");
            return returnedValue;
        }
    }
}

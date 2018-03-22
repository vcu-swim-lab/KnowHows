using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Website.Managers;

namespace Website.Manager
{
    public class GitHubUser
    {
        public string TeamID, ChannelID, UserID;

        private GitHubClient _client;
        private string _gitHubAccessToken;
        private List<String> _repositories;
        private List<String> _trackedRepositories;

        public GitHubClient GitHubClient
        {
            get { return _client; }
        }

        public String UUID
        {
            get { return String.Format("{0}.{1}.{2}", TeamID, ChannelID, UserID); }
        }

        public IReadOnlyCollection<String> Repositories
        {
            get { return _repositories.AsReadOnly(); }
        }

        public IReadOnlyCollection<String> TrackedRepositories
        {
            get { return _trackedRepositories.AsReadOnly(); }
        }

        public IReadOnlyCollection<String> UntrackedRepositories
        {
            get { return Repositories.Except(TrackedRepositories).ToList().AsReadOnly(); }
        }

        public string GitHubAccessToken
        {
            get { return _gitHubAccessToken; }
            set
            {
                _gitHubAccessToken = value;
                _client.Credentials = new Credentials(_gitHubAccessToken);
                UpdateRepositoryIndex();
            }
        }

        public GitHubUser(string teamId, string channelId, string userId)
        {
            this.TeamID = teamId;
            this.ChannelID = channelId;
            this.UserID = userId;
            this._repositories = new List<String>();
            this._trackedRepositories = new List<String>();
            this._client = new GitHubClient(new ProductHeaderValue(userId));
        }

        private void UpdateRepositoryIndex()
        {
            _repositories.Clear();
            _trackedRepositories.Clear();
            var repos = _client.Repository.GetAllForCurrent().Result;
            foreach (var repo in repos) _repositories.Add(repo.Name);
        }

        public bool TrackRepository(string repositoryName)
        {
            if (_repositories.Contains(repositoryName) && !_trackedRepositories.Contains(repositoryName)) {
                _trackedRepositories.Add(repositoryName);
                Task.Run( () => SolrManager.Instance.TrackRepository(this, repositoryName));
                return true;
            }
            return false;
        }

        public bool UntrackRepository(string repositoryName)
        {
            if(_trackedRepositories.Contains(repositoryName)) {
                _trackedRepositories.Remove(repositoryName);
                Task.Run(() => SolrManager.Instance.UntrackRepository(this, repositoryName));
                return true;
            }
            return false;
        }
    }

    public class UserManager
    {
        public static UserManager Instance = Load();

        private HashSet<String> pendingUsers = new HashSet<string>();
        private Dictionary<String, GitHubUser> githubUsers = new Dictionary<string, GitHubUser>();

        private static UserManager Load()
        {
            if (File.Exists("./users.json")) {
                return JsonConvert.DeserializeObject<UserManager>(File.ReadAllText("./users.json"));
            }
            else return null;
        }

        private void Save()
        {
            File.WriteAllText("./users.json", JsonConvert.SerializeObject(this));
            Console.WriteLine("Performing save of current user manager at {0}", DateTime.Now);

            // Reoccuring save
            Task.Run( () => {
                Thread.Sleep(1000 * 60 * 5); // every 5 minutes
                Save();
            });
        }

        private UserManager() { Save(); }

        public bool IsGitHubAuthenticated(string uuid)
        {
            return githubUsers.ContainsKey(uuid);
        }

        public void AddPendingGitHubAuth(string uuid)
        {
            if (!githubUsers.ContainsKey(uuid)) pendingUsers.Add(uuid);
        }

        public void AddGitHubAuth(string uuid, string token)
        {
            if (pendingUsers.Contains(uuid)) {
                githubUsers[uuid] = new GitHubUser(GetTeamIDFromUUID(uuid), GetChannelIDFromUUID(uuid), GetUserIDFromUUID(uuid));
                githubUsers[uuid].GitHubAccessToken = token;
                pendingUsers.Remove(uuid);
            }
            else throw new Exception("Tried to add successful github auth for user with no pending state: " + uuid);
        }

        public GitHubUser GetGitHubUser(string uuid)
        {
            return githubUsers[uuid];
        }

        public static String GetTeamIDFromUUID(string uuid) { return uuid.Split(".")[0]; }
        public static String GetChannelIDFromUUID(string uuid) { return uuid.Split(".")[1]; }
        public static String GetUserIDFromUUID(string uuid) { return uuid.Split(".")[2]; }
        public static String GetTeamBasedIDFromUUID(string uuid) { return GetTeamIDFromUUID(uuid) + ".users." + GetUserIDFromUUID(uuid); }
    }
}

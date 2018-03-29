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
        public String TeamID, ChannelID, UserID;

        private String _gitHubAccessToken;

        [JsonProperty(PropertyName = "Repositories")]
        private List<String> _repositories;

        [JsonProperty(PropertyName = "TrackedRepositories")]
        private List<String> _trackedRepositories;

        [JsonIgnore]
        private GitHubClient _client;

        [JsonIgnore]
        public GitHubClient GitHubClient
        {
            get { return _client; }
        }

        [JsonIgnore]
        public String UUID
        {
            get { return String.Format("{0}.{1}.{2}", TeamID, ChannelID, UserID); }
        }

        [JsonIgnore]
        public IReadOnlyCollection<String> Repositories
        {
            get { return _repositories.AsReadOnly(); }
        }

        [JsonIgnore]
        public IReadOnlyCollection<String> TrackedRepositories
        {
            get { return _trackedRepositories.AsReadOnly(); }
        }

        [JsonIgnore]
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

                if (_client == null) {
                    _client = new GitHubClient(new ProductHeaderValue(UserID));
                }

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
        public static UserManager Instance = new UserManager();

        private HashSet<String> pendingUsers = new HashSet<string>();
        private Dictionary<String, GitHubUser> githubUsers;

        private static Dictionary<String, GitHubUser> Load()
        {
            try
            {
                if (File.Exists("./data/users.json"))
                {
                    return JsonConvert.DeserializeObject<Dictionary<String, GitHubUser>>(File.ReadAllText("./data/users.json"));
                }
            }
            catch (Exception ex) { Console.WriteLine(ex); }
            return new Dictionary<String, GitHubUser>();
        }

        private void save()
        {
            File.WriteAllText("./data/users.json", JsonConvert.SerializeObject(githubUsers));
            Console.WriteLine("Performing save of current user manager at {0}", DateTime.Now);

            // Reoccuring save
            Task.Run(() => {
                Thread.Sleep(1000 * 60); // every 60 seconds
                save();
            });
        }

        private UserManager() { githubUsers = Load(); save(); }

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

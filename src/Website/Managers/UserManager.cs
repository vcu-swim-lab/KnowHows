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

        public bool _hasBeenAutoRun;

        [JsonProperty(PropertyName = "Repositories")]
        private List<KeyValuePair<string, string>> _repositories;

        [JsonProperty(PropertyName = "TrackedRepositories")]
        private List<KeyValuePair<string, string>> _trackedRepositories;

        [JsonProperty(PropertyName = "UntrackedRepositories")]
        private List<KeyValuePair<string, string>> _untrackedRepositories;

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
        public IReadOnlyCollection<KeyValuePair<string,string>> Repositories
        {
            get { return _repositories.AsReadOnly(); }
        }

        [JsonIgnore]
        public IReadOnlyCollection<KeyValuePair<string,string>> TrackedRepositories
        {
            get { return _trackedRepositories.AsReadOnly(); }
        }

        [JsonIgnore]
        public IReadOnlyCollection<KeyValuePair<string, string>> UntrackedRepositories
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
            }
        }

        public GitHubUser(string teamId, string channelId, string userId)
        {
            this.TeamID = teamId;
            this.ChannelID = channelId;
            this.UserID = userId;
            this._repositories = new List<KeyValuePair<string,string>>();
            this._trackedRepositories = new List<KeyValuePair<string, string>>();
            this._untrackedRepositories = new List<KeyValuePair<string, string>>();
            this._hasBeenAutoRun = false;
            this._client = new GitHubClient(new ProductHeaderValue(userId));
        }

        private void UpdateRepositoryIndex(string repoName, string repoOwner)
        {
            _repositories.Add(new KeyValuePair<string, string>(repoName, repoOwner));
            _untrackedRepositories.Add(new KeyValuePair<string, string>(repoName, repoOwner));
        }

        public bool AutoTrackRepos()
        {
            var user = _client.User.Current().Result.Login;

            // Owned repositories
            var repositories = _client.Repository.GetAllForCurrent().Result.ToList();
            foreach (var repo in repositories) {
                UpdateRepositoryIndex(repo.FullName, repo.Id.ToString());
            }

            // Contributing repositories with push events
            var allContributingRepos = _client.Activity.Events.GetAllUserPerformedPublic(user).Result;
            var filteredContribRepos = allContributingRepos.Where(x => x.Type == "PushEvent").Select(x => x.Repo.Id).Distinct().ToList();

            foreach (var repoId in filteredContribRepos) {
                try {
                    var repo = _client.Repository.Get(repoId).Result;
                    repositories.Add(repo);
                    UpdateRepositoryIndex(repo.FullName, repo.Id.ToString());
                }
                catch (Exception ex) { 
                    Console.WriteLine(String.Format("Error: could not retrieve contributing repository {0}", repoId));
                    Console.WriteLine(ex.ToString()); 
                }
            }

            // Auto track public repositories
            foreach (var repo in repositories)
            {
                // shouldnt be any tracked at this point
                if (!_trackedRepositories.Select(x => x.Key == repo.FullName).Any() && !repo.Private)
                {
                    Task.Run(() => TrackRepository(repo.FullName, true, repo.Id.ToString()));
                }
            }
            
            return true;
        }

        public bool TrackRepository(string repositoryName, bool isAutoTRacking = false, string curUser = null)
        {
            List<Repository> repos = new List<Repository>();

            if (repositoryName == "*") {
                foreach (var untrackedRepo in UntrackedRepositories) {
                    repos.Add(_client.Repository.Get(long.Parse(UntrackedRepositories.First(x => x.Key == untrackedRepo.Key).Value)).Result);
                }
            }
            else if (!isAutoTRacking && _untrackedRepositories.Exists(x => x.Key == repositoryName))
            {
                var reps = _client.Repository.Get(long.Parse(UntrackedRepositories.First(x => x.Key == repositoryName).Value)).Result;
                repos.Add(reps);
            }
            else if (isAutoTRacking && curUser != null)
            {
                repos.Add(_client.Repository.Get(long.Parse(curUser)).Result);
            }

            // check if repos contains untracked data already
            // TODO: Fix this
            //repos.RemoveAll(r => r.Name == _trackedRepositories.First(x => x.Key == r.Name).Key);

            if (repos.Any())
            {
                _trackedRepositories.AddRange(repos.Select(x => new KeyValuePair<string, string>(x.FullName, x.Id.ToString())));
                _untrackedRepositories.RemoveAll(x => x.Key == repositoryName);
                Task.Run(() => SolrManager.Instance.TrackRepository(this, repos));
                return true;
            }

            return false;
        }

        public bool UntrackRepository(string repositoryName)
        {
            if (repositoryName == "*")
            {
                Task.Run(() => SolrManager.Instance.UntrackRepository(this, _trackedRepositories.Select(x => x.Key).ToList()));
                _untrackedRepositories = _trackedRepositories;
                _trackedRepositories = new List<KeyValuePair<string,string>>();
                return true;
            }
            else if (_trackedRepositories.Exists(x => x.Key == repositoryName))
            {
                _untrackedRepositories.Add(_trackedRepositories.Find(x => x.Key == repositoryName));
                _trackedRepositories.RemoveAll(x => x.Key == repositoryName);
                Task.Run(() => SolrManager.Instance.UntrackRepository(this, new List<string>() { repositoryName }));
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
                else {
                    Directory.CreateDirectory("./data");
                    File.Create("./data/users.json").Close();
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
            Task.Run(() =>
            {
                Thread.Sleep(1000 * 60); // every 60 seconds
                save();
            });
        }

        private UserManager() { githubUsers = Load(); save(); }

        public bool IsGitHubAuthenticated(string uuid)
        {
            return githubUsers.ContainsKey(uuid);
        }

        public bool HasBeenAutoRun(string uuid)
        {
            return githubUsers[uuid]._hasBeenAutoRun;
        }

        public void SetAutoRun(string uuid)
        {
            githubUsers[uuid]._hasBeenAutoRun = true;
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

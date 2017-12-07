using SolrServices;

namespace Website.Managers
{
    public class SolrManager
    {
        public static SolrManager Instance = new SolrManager();

        public SolrService solrService = new SolrService();

        private SolrManager() { }
    }
}

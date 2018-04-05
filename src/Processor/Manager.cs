
namespace Processor
{
    public class Manager
    {
        public string SolrClean()
        {
            // remove all documents: "localhost:8983/solr/new_core/update?commit=true" -H "Content-Type: text/xml" --data-binary "<delete><query>*:*</query></delete>"
            return "todo";
        }
    }
}

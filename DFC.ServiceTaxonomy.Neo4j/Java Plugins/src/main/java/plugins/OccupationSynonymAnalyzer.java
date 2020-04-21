import org.apache.lucene.analysis.Analyzer;
import org.neo4j.graphdb.index.fulltext.AnalyzerProvider;
import org.neo4j.helpers.Service;

@Service.Implementation(AnalyzerProvider.class)
public class OccupationSynonymAnalyzer extends AnalyzerProvider {

    public static final String ANALYZER_NAME = "synonym-occupation";

    public OccupationSynonymAnalyzer() {
        super(ANALYZER_NAME);
    }

    @Override
    public Analyzer createAnalyzer() {
			String synonymFileName = "ncs.occupation_synonyms_file_url";
			
			SynonymHelper synonymHelper = new SynonymHelper();
			return synonymHelper.buildAnalyzer(synonymFileName);
    }

    @Override
    public String description() {
        return "Adds a Synonym Analyzer with Occupations from Neo4J";
    }
}
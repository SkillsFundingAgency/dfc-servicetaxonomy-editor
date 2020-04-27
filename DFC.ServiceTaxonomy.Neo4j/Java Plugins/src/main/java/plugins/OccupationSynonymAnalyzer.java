import org.apache.lucene.analysis.Analyzer;
import org.neo4j.graphdb.schema.AnalyzerProvider;
import org.neo4j.annotations.service.ServiceProvider;

@ServiceProvider
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

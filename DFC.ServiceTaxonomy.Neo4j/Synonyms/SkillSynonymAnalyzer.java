import org.apache.lucene.analysis.Analyzer;
import org.apache.lucene.analysis.core.LowerCaseFilterFactory;
import org.apache.lucene.analysis.custom.CustomAnalyzer;
import org.apache.lucene.analysis.ngram.EdgeNGramFilterFactory;
import org.apache.lucene.analysis.ngram.NGramFilterFactory;
import org.apache.lucene.analysis.standard.StandardFilterFactory;
import org.apache.lucene.analysis.standard.StandardTokenizerFactory;
import org.apache.lucene.analysis.synonym.SynonymFilterFactory;
import org.neo4j.graphdb.index.fulltext.AnalyzerProvider;
import org.neo4j.helpers.Service;

@Service.Implementation(AnalyzerProvider.class)
public class SkillSynonymAnalyzer extends AnalyzerProvider {

    public static final String ANALYZER_NAME = "synonym-skills";

    public SkillSynonymAnalyzer() {
        super(ANALYZER_NAME);
    }

    @Override
    public Analyzer createAnalyzer() {
			String synonymFileName = "ncs.skill_synonyms_file_url";
			
			SynonymHelper synonymHelper = new SynonymHelper();
			return synonymHelper.buildAnalyzer(synonymFileName);
    }

    @Override
    public String description() {
        return "Adds a Synonym Analyzer with Skills from Neo4J";
    }
}
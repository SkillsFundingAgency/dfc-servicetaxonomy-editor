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
import java.io.*;
import java.net.*;
import java.nio.file.Paths;
import java.io.File;
import java.io.IOException;
import org.apache.commons.io.FileUtils;

@Service.Implementation(AnalyzerProvider.class)
public class SynonymAnalyzer extends AnalyzerProvider {

    public static final String ANALYZER_NAME = "synonym-custom";

    public SynonymAnalyzer() {
        super(ANALYZER_NAME);
    }

    @Override
    public Analyzer createAnalyzer() {
        try {
            Analyzer analyzer = CustomAnalyzer.builder(new UrlResourceLoader())
			//Analyzer analyzer = CustomAnalyzer.builder()
                    .withTokenizer(StandardTokenizerFactory.class)
                    .addTokenFilter(StandardFilterFactory.class)
                    .addTokenFilter(SynonymFilterFactory.class, "synonyms", "https://dl.dropboxusercontent.com/s/f08rb52rcgnthso/synonyms2.txt")
                    .addTokenFilter(LowerCaseFilterFactory.class)
                    .build();

            return analyzer;
        } catch (Exception e) {
            throw new RuntimeException("Unable to create analyzer", e);
        }
    }

    @Override
    public String description() {
        return "The default, standard analyzer with a synonyms file. This is an example analyzer for educational purposes.";
    }
}
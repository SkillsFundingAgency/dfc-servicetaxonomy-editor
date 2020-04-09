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
import java.nio.channels.Channels;
import java.nio.channels.ReadableByteChannel;

@Service.Implementation(AnalyzerProvider.class)
public class SynonymAnalyzer extends AnalyzerProvider {

    public static final String ANALYZER_NAME = "synonym-custom";

    public SynonymAnalyzer() {
        super(ANALYZER_NAME);
    }

    @Override
    public Analyzer createAnalyzer() {
        try {
			String currentDir = System.getProperty("user.dir");
			String synFile = "synonyms.txt";
			
			String synonymFileUrl = System.getenv("NCS_SYNONYM_FILE");
			
			if(synonymFileUrl == null)
			{
					synonymFileUrl = "https://dl.dropboxusercontent.com/s/1l2rra1hxhaxktw/synonyms.txt";
			}
			
			System.out.println("Synonym file locaiton:" + synonymFileUrl);
			
			URL url = new URL(synonymFileUrl);
			ReadableByteChannel rbc = Channels.newChannel(url.openStream());
			FileOutputStream fos = new FileOutputStream(currentDir + "/Synonyms/" + synFile);
			fos.getChannel().transferFrom(rbc, 0, Long.MAX_VALUE);
			fos.close();
			rbc.close();
			
            Analyzer analyzer = CustomAnalyzer.builder(Paths.get(currentDir + "/synonyms/"))
                    .withTokenizer(StandardTokenizerFactory.class)
                    .addTokenFilter(StandardFilterFactory.class)
                    .addTokenFilter(SynonymFilterFactory.class, "synonyms", synFile)
                    .addTokenFilter(LowerCaseFilterFactory.class)
                    .build();

            return analyzer;
        } catch (Exception e) {
            throw new RuntimeException("Unable to create analyzer", e);
        }
    }

    @Override
    public String description() {
        return "The default, standard analyzer with a synonyms file. Reads a Synonym text file specified in NCS_SYNONYM_FILE (must be a url).";
    }
}
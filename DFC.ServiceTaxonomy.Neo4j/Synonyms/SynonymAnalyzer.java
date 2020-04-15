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
import java.util.Scanner;

@Service.Implementation(AnalyzerProvider.class)
public class SynonymAnalyzer extends AnalyzerProvider {

    public static final String ANALYZER_NAME = "synonym-custom";

    public SynonymAnalyzer() {
        super(ANALYZER_NAME);
    }

    @Override
    public Analyzer createAnalyzer() {
			String neo4jPath = System.getenv("NEO4J_CONF") + "/neo4j.conf";
			String fileUrl = "";
			
			File file = new File(neo4jPath);

			try {
				Scanner scanner = new Scanner(file);

							 int lineNum = 0;
				while (scanner.hasNextLine()) {
					String line = scanner.nextLine();
					lineNum++;
					
					if(line.indexOf("ncs.synonyms_file_url") != -1)
					{
						fileUrl = line.split("=")[1];
					}
				}
			} catch(FileNotFoundException e) { 
				throw new RuntimeException("ncs.synonyms_file_url not found in Neo4J Configuration", e);
			}
			
			try{
            Analyzer analyzer = CustomAnalyzer.builder(new UrlResourceLoader())
			//Analyzer analyzer = CustomAnalyzer.builder()
                    .withTokenizer(StandardTokenizerFactory.class)
                    .addTokenFilter(StandardFilterFactory.class)
                    .addTokenFilter(SynonymFilterFactory.class, "synonyms", fileUrl)
                    .addTokenFilter(LowerCaseFilterFactory.class)
                    .build();

            return analyzer;
         } catch (Exception ex) {
            System.out.println("Could not add Synonym Analyzer:" + ex);
			try{
				Analyzer analyzer = CustomAnalyzer.builder()
						.withTokenizer(StandardTokenizerFactory.class).build();
				return analyzer;	
			}
			catch (Exception re){
				throw new RuntimeException("Couldn't add standard analyzer", re);
			}
					
         }
    }

    @Override
    public String description() {
        return "The default, standard analyzer with a synonyms file. This is an example analyzer for educational purposes.";
    }
}
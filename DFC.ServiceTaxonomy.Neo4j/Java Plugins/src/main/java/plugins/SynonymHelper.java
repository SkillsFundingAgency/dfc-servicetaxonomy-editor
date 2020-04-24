import org.apache.lucene.analysis.Analyzer;
import org.apache.lucene.analysis.core.LowerCaseFilterFactory;
import org.apache.lucene.analysis.custom.CustomAnalyzer;
import org.apache.lucene.analysis.standard.StandardFilterFactory;
import org.apache.lucene.analysis.standard.StandardTokenizerFactory;
import org.apache.lucene.analysis.synonym.SynonymFilterFactory;
import java.io.*;
import org.apache.commons.io.FileUtils;
import java.util.Scanner;

public class SynonymHelper {

    public SynonymHelper() {
    }

    public Analyzer buildAnalyzer(String settingName) {
			String neo4jPath = System.getenv("NEO4J_CONF") + "/neo4j.conf";
			String fileUrl = "";
			
			File file = new File(neo4jPath);

			try {
				Scanner scanner = new Scanner(file);

				int lineNum = 0;
				while (scanner.hasNextLine()) {
					String line = scanner.nextLine();
					lineNum++;
					
					if(line.indexOf(settingName) != -1)
					{
						fileUrl = line.split("=")[1];
					}
				}
			} catch(FileNotFoundException e) { 
				throw new RuntimeException(settingName + "not found in Neo4J Configuration", e);
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
}

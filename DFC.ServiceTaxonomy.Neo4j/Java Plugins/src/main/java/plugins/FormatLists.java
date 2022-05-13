package ncs.servicetaxonomy.userfunctions;

import java.util.List;
import java.util.ArrayList;
import org.neo4j.procedure.Description;
import org.neo4j.procedure.Name;
import org.neo4j.procedure.UserFunction;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class FormatLists {
    public static final String ulPattern = "<ul>.*</ul>";
    public static final Pattern ul = Pattern.compile(ulPattern);
	
	public static final String liPattern = "<li>(.*?)</li>";
	public static final Pattern li = Pattern.compile(liPattern);

    @UserFunction
    @Description("Formats list objects to UI Markup")
    public String formatLists(
            @Name("input") String input) {
        if (input == null) {
            return null;
        }	
     
		Matcher m = ul.matcher(input);
      
	  while (m.find()) {
		String match = m.group(0);
		
		Matcher ulm = li.matcher(match);
		
		List<String> outputGroups = new ArrayList<>();
		
		while(ulm.find()){
			outputGroups.add(ulm.group(1));
		}
		
		String cleanedString = "[" + String.join(";", outputGroups) + "]";
		
		input = input.replace(match, cleanedString);
	  }
		return input;
    }
}

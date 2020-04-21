package ncs.servicetaxonomy.userfunctions;

import java.util.List;

import org.neo4j.procedure.Description;
import org.neo4j.procedure.Name;
import org.neo4j.procedure.UserFunction;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class FormatLinks {

    @UserFunction
    @Description("Replaces anchor tags with markup UI expects")
    public String formatLinks(
            @Name("input") String input) {
        if (input == null) {
            return null;
        }
		
		String pattern = "(<a href=\"([^\"]+)\">([^<]+)</a>)";
      
		Pattern r = Pattern.compile(pattern);
     
		Matcher m = r.matcher(input);
      
		if (m.find( )) {
			return input.replaceAll(pattern, "[" + m.group(3) +" | " + m.group(2) + "]");
		}
		
		return input;
    }
}
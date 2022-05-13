package ncs.servicetaxonomy.userfunctions;

import java.util.List;

import org.neo4j.procedure.Description;
import org.neo4j.procedure.Name;
import org.neo4j.procedure.UserFunction;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class RemoveHtmlTags {
    public static final String pattern = "<[^>]*>";
    public static final Pattern r = Pattern.compile(pattern);

    @UserFunction
    @Description("Removes HTML tags from the input string")
    public String removeHtmlTags(
            @Name("input") String input) {
        if (input == null) {
            return null;
        }	
     
		Matcher m = r.matcher(input);
      
		if (m.find( )) {
			return input.replaceAll(pattern, "");
		}
		
		return input;
    }
}

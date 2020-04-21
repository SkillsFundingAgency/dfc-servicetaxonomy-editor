namespace GetJobProfiles.Models.Recipe.Fields
{
    public class TextField
    {
        public TextField() => Text = null;
        public TextField(string text) => Text = text;

        public string Text { get; set; }
    }
}

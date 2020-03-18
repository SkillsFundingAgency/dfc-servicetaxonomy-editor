namespace DFC.ServiceTaxonomy.GraphVisualiser.Models
{
    public class ColourScheme
    {
        private readonly string[] _colours;

        public ColourScheme(string[] colours)
        {
            _colours = colours;
        }

        private int _current;

        public string NextColour()
        {
            string colour = _colours[_current++];
            if (_current == _colours.Length)
            {
                _current = 0;
            }

            return colour;
        }
    }
}

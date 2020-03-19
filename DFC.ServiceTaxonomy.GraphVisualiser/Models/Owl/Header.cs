﻿using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl
{
    public partial class Header
    {
        public List<string> Languages { get; set; } = new List<string>();
        public Description Title { get; set; } = new Description();
        public string Iri { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public List<string> Author { get; set; } = new List<string>();
        public Description Description { get; set; } = new Description();
    }
}

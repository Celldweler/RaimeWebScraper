using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;

namespace RaimeWebScraper
{
    public class Trick
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public List<string> Prerequisites { get; set; }= new List<string>();
        public List<string> Progressions { get; set; } = new List<string>();


        public string CategoriesStrList => Categories.Aggregate((current, next) => current + "," + next);
        public string PrerequisitesStrList => 
            Prerequisites == null || Prerequisites.Count == 0 ? "none" 
            : Prerequisites.Aggregate((current, next) => current + "," + next);

        public override string ToString()
        {
            var trickToStringBuilder = new StringBuilder();

            // { "id": "value", "name": "nameValue", "categories": [parentCat, sub], "prerequisites": [prereq1, prereq2] }
            trickToStringBuilder.Append("{ ");
            trickToStringBuilder.Append($"\"{nameof(Id).ToLower()}\": \"{Id}\", ");
            trickToStringBuilder.Append($"\"{nameof(Name).ToLower()}\": \"{Name}\", ");
            trickToStringBuilder.Append($"\"{nameof(Categories).ToLower()}\": [{CategoriesStrList}], ");
            trickToStringBuilder.Append($"\"{nameof(Prerequisites).ToLower()}\": [{PrerequisitesStrList}]");
            trickToStringBuilder.Append(" }");
            
            return trickToStringBuilder.ToString();
        }
    }
}
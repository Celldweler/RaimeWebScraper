using System;
using System.Collections.Generic;

namespace RaimeWebScraper.DataModel
{
    public class TricktionaryModel
    {
        public class Trick
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime Created { get; set; }
            
            // public List<string> Categories { get; set; } = new List<string>();
            // public List<string> Prerequisites { get; set; }= new List<string>();
            // public List<string> Progressions { get; set; } = new List<string>();
            public List<TrickRelationship> Prerequisites { get; set; }
            public List<TrickRelationship> Progressions { get; set; }

            public Difficulty Difficulty { get; set; }
            public string DifficultyId { get; set; }
        }

        public class Category
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            
            public DateTime Created { get; set; }
        }
        
        public class TrickRelationship
        {
            public Trick Prerequisite { get; set; }
            public string PrerequisiteId { get; set; }
            
            public Trick Progression { get; set; }
            public string ProgressionId { get; set; }
        }

        public class TrickCategory
        {
            public Trick Trick { get; set; } 
            public string TrickId { get; set; } 
            
            public Category Category { get; set; } 
            public string CategoryId { get; set; } 
        }
        
        public class Difficulty
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            
            public DateTime Created { get; set; }

            public List<Trick> Tricks { get; set; } = new List<Trick>();
        }
    }
}
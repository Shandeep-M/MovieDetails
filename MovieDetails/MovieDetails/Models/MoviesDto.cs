using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieDetails.Models
{
    public class MoviesDto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }      
        
        [Range(1800,2018,ErrorMessage = "Value should be between 1800 and 2018")]        
        public Nullable<int> ReleasedYear { get; set; }
        public string Plot { get; set; }
        public System.Drawing.Image Poster { get; set; }
        public List<string> Actors { get; set; }
        
        public string Producer { get; set; }

        public SelectList ProducerList { get; set; }

        public List<CheckList> ActorList { get; set; }        
        public string PersonName { get; set; }
        [EnumDataType(typeof(GenderEnum))]
        public GenderEnum Sex { get; set; }
        [DataType(DataType.Date , ErrorMessage = "Value should be either M/F")]
        public DateTime? DOB { get; set; }

        public string Bio { get; set; }

    }
}
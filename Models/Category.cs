using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce_webApi.Models
{
    public class Category
    {       
        public Guid CategoryId {get ; set;}
        public string Name {get; set;} = string.Empty;
        public string? Description {get; set;} = string.Empty;
        public DateTime CreateAdt{get; set;}

        public Category()
        {
            CategoryId = Guid.NewGuid();
            CreateAdt = DateTime.UtcNow;
        }


        };
    }

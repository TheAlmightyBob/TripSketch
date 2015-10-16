using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripSketch.Core.Models
{
    public class DBModel
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
    }
}

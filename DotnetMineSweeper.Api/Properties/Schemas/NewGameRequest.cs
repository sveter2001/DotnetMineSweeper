using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetMineSweeper.Api.Properties.Schemas
{
    public class NewGameRequest
    {
        public int width { get; set; }
        public int height { get; set; }
        public int mines_count { get; set; }
    }
}

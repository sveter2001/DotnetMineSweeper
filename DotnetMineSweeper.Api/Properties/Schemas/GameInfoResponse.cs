using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetMineSweeper.Api.Properties.Schemas
{
    public class GameInfoResponse
    {
        public string game_id { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int mines_count { get; set; }
        public bool completed { get; set; }
        public string[][] field { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetMineSweeper.Api.Properties.Schemas
{
    public class GameTurnRequest
    {
        public string game_id { get; set; }
        public int col { get; set; }
        public int row { get; set; }
    }
}

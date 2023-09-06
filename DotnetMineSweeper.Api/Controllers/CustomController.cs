using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using DotnetMineSweeper.Api.Properties.Schemas;

namespace DotnetMineSweeper.Api.Controllers
{
    [Route("api/Custom")]
    [ApiController]
    public class CustomController : ControllerBase
    {
        //
        public static Dictionary<string, GameInfoResponse> games = new Dictionary<string, GameInfoResponse>();// словарь для хранения текущего состояния игры, ключ:game_id, value: GameInfoResponse
        public static Dictionary<string, string[][]> gamesUnlokedField = new Dictionary<string, string[][]>();// словарь для хранения открытого поля, ключ:game_id, value: string[][]
        //
        [HttpPost("new")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GameInfoResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [Produces("application/json")]
        public IActionResult NewGame([FromBody] NewGameRequest request)
        {
            try
            {
                if(request.width > 30 || request.height > 30 || request.width < 2 || request.height < 2 || request.mines_count >= request.height * request.width || request.mines_count < 1)
                {
                    return Error("Некоретные данные поля или некоректное количество мин");
                }
                Guid guid = Guid.NewGuid();
                string generatedId = guid.ToString();//создание game_id
                GameInfoResponse startResponse = new GameInfoResponse();
                startResponse.game_id = generatedId;
                startResponse.width = request.width;
                startResponse.height = request.height;
                startResponse.mines_count = request.mines_count;

                string[][] field = new string[request.width][];
                for (int i = 0; i < request.height; i++)
                {
                    field[i] = new string[request.width];
                    for (int j = 0; j < request.width; j++)
                    {
                        field[i][j] = " ";
                    }
                }
                startResponse.field = field;
                startResponse.completed = false;
                games.Add(generatedId, startResponse);
                return Ok(startResponse);
            }
            catch 
            {
                return Error("Произошла непредвиденная ошибка");
            }
        }
        [HttpPost("turn")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GameInfoResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [Produces("application/json")]
        public IActionResult Turn([FromBody] GameTurnRequest request)
        {
            try
            {
                if (games[request.game_id].completed)
                {
                    return Error("Игра завершена");
                }
                if(games[request.game_id].field[request.row][request.col]!=" ")
                {
                    return Error("уже открытая ячейка");
                }
                string[][] field = games[request.game_id].field;
                GameInfoResponse turnResponse = new GameInfoResponse();
                if (!gamesUnlokedField.ContainsKey(request.game_id))
                {
                    GenerateOpenField(request.game_id, games[request.game_id].mines_count, request.row, request.col);
                }
                if (gamesUnlokedField[request.game_id][request.row][request.col] != "X")
                {
                    OpenCells(request.game_id, request.row, request.col);
                    turnResponse.field = games[request.game_id].field;
                    turnResponse.completed = Identical(request.game_id);
                    if (turnResponse.completed)
                    {
                        for(int i = 0; i < games[request.game_id].field.Length; i++)
                        {
                            games[request.game_id].field[i] = string.Join("S", games[request.game_id].field[i]).Replace(" ", "M").Split("S");
                        }
                        turnResponse.field = games[request.game_id].field;
                    }
                }
                else
                {
                    turnResponse.field = gamesUnlokedField[request.game_id];
                    turnResponse.completed = true;
                }
                turnResponse.game_id = request.game_id;
                turnResponse.width = games[request.game_id].field[0].Length;
                turnResponse.height = games[request.game_id].field.Length;
                turnResponse.mines_count = games[request.game_id].mines_count;
                games[request.game_id] = turnResponse;
                return Ok(turnResponse);
            }
            catch
            {
                return Error("Произошла непредвиденная ошибка");
            }
        }
        private IActionResult Error(string message)
        {
            ErrorResponse myError = new ErrorResponse();
            myError.error = message;
            return BadRequest(myError);
        }
        private bool Identical(string id) //проверка завершенности игры
        {
            string wholeMatrix = "";
            bool result;

            for(int i = 0; i < games[id].field.Length; i++)
            {
                wholeMatrix += string.Join("", games[id].field[i]);
            }

            result = (wholeMatrix.Split(" ").Length - 1 == games[id].mines_count);

            return result;
        }
        private void OpenCells(string id, int y, int x)
        {
            // Открываем ячейку
            games[id].field[y][x] = gamesUnlokedField[id][y][x];
            if (Convert.ToInt32(gamesUnlokedField[id][y][x]) == 0)
            {
                // Рекурсивно открываем соседние ячейки, если они не являются минами
                if (y - 1 >= 0 && gamesUnlokedField[id][y - 1][x] != "X" && games[id].field[y - 1][x] == " ")
                {
                    OpenCells(id, y - 1, x); // Верхняя соседняя ячейка
                }
                if (y + 1 < games[id].field.Length && gamesUnlokedField[id][y + 1][x] != "X" && games[id].field[y + 1][x] == " ")
                {
                    OpenCells(id, y + 1, x); // Нижняя соседняя ячейка
                }
                if (x - 1 >= 0 && gamesUnlokedField[id][y][x - 1] != "X" && games[id].field[y][x-1] == " ")
                {
                    OpenCells(id, y, x - 1); // Левая соседняя ячейка
                }
                if (x + 1 < games[id].field[0].Length && gamesUnlokedField[id][y][x + 1] != "X" && games[id].field[y][x+1] == " ")
                {
                    OpenCells(id, y, x + 1); // Правая соседняя ячейка
                }

                if (y - 1 >= 0 && x - 1 >= 0 && gamesUnlokedField[id][y - 1][x - 1] != "X" && games[id].field[y - 1][x - 1] == " ")
                {
                    OpenCells(id, y - 1, x - 1); // Верхняя левая
                }
                if (y + 1 < games[id].field.Length && x + 1 < games[id].field[0].Length && gamesUnlokedField[id][y + 1][x + 1] != "X" && games[id].field[y + 1][x + 1] == " ")
                {
                    OpenCells(id, y + 1, x + 1); // Нижняя правая
                }
                if (x - 1 >= 0 && y + 1 < games[id].field.Length && gamesUnlokedField[id][y + 1][x - 1] != "X" && games[id].field[y + 1][x - 1] == " ")
                {
                    OpenCells(id, y + 1, x - 1); // Нижняя левая
                }
                if (x + 1 < games[id].field[0].Length && y - 1 >= 0 && gamesUnlokedField[id][y - 1][x + 1] != "X" && games[id].field[y - 1][x + 1] == " ")
                {
                    OpenCells(id, y - 1, x + 1); // Верхняя правая
                }
            }
            
        }
        private void GenerateOpenField(string id, int mines_count, int y, int x) // генерация поля с минами
        {
            int col;
            int row;
            int cell;
            Random random = new Random();
            string[][] unlokedField = new string[games[id].field.Length][];
            for (int i = 0; i < unlokedField.Length; i++)
            {
                unlokedField[i] = new string[games[id].field[i].Length];
                for (int j = 0; j < unlokedField[i].Length; j++)
                {
                    unlokedField[i][j] = "0";
                }
            }
            for(int i = 0; i < mines_count; i++)
            {
                col = random.Next(0, unlokedField.Length);
                row = random.Next(0, unlokedField[0].Length);
                if ((col != x && row != y) && unlokedField[row][col] != "X")
                {
                    unlokedField[row][col] = "X";
                    for(int j = -1; j < 2; j++)
                    {
                        for(int k = -1; k < 2; k++)
                        {
                            try
                            {
                                if (unlokedField[row + j][col + k] != "X")
                                {
                                    cell = Convert.ToInt32(unlokedField[row + j][col + k]) + 1;
                                    unlokedField[row + j][col + k] = cell.ToString();
                                }
                            }
                            catch { }
                        }
                    }
                }
                else
                {
                    mines_count++;
                }
            }
            gamesUnlokedField.Add(id, unlokedField);
        }
    }
}

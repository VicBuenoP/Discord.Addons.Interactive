using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using SampleApp.Class;

namespace SampleApp.Modules
{
    public class SampleModule : InteractiveBase
    {
        private static List<UserDice> _userDiceList = new List<UserDice>();

        //// DeleteAfterAsync will send a message and asynchronously delete it after the timeout has popped
        //// This method will not block.
        //[Command("delete")]
        //public async Task<RuntimeResult> Test_DeleteAfterAsync()
        //{
        //    await ReplyAndDeleteAsync("this message will delete in 10 seconds", timeout: TimeSpan.FromSeconds(10));
        //    return Ok();
        //}

        //// NextMessageAsync will wait for the next message to come in over the gateway, given certain criteria
        //// By default, this will be limited to messages from the source user in the source channel
        //// This method will block the gateway, so it should be ran in async mode.
        //[Command("next", RunMode = RunMode.Async)]
        //public async Task Test_NextMessageAsync()
        //{
        //    await ReplyAsync("What is 2+2?");
        //    var response = await NextMessageAsync();
        //    if (response != null)
        //        await ReplyAsync($"You replied: {response.Content}");
        //    else
        //        await ReplyAsync("You did not reply before the timeout");
        //}

        //// PagedReplyAsync will send a paginated message to the channel
        //// You can customize the paginator by creating a PaginatedMessage object
        //// You can customize the criteria for the paginator as well, which defaults to restricting to the source user
        //// This method will not block.
        //[Command("paginator")]
        //public async Task Test_Paginator()
        //{
        //    var pages = new[] { "Página 1 :D", "Página 2 >:D", "Ya van 3 >:DD", "JAJAJAJA", ":susurros:" };
        //    await PagedReplyAsync(pages);
        //}

        /// <summary>
        /// Método para tirar iniciativa
        /// </summary>
        /// <returns></returns>
        [Command("iniciativa")]
        public async Task TiraIniciativa([Remainder] int amount = 13)
        {
            var userName = Context.User.Username;
            var rand = new Random();
            var dice = rand.Next(1, 14);

            var str = ($"{userName} -> 1d{amount}: **{dice}**");
            await ReplyAsync(str.ToString());

            _userDiceList.Add(new UserDice
            {
                Name = userName,
                Dice = dice
            });
        }

        /// <summary>
        /// Método para finalizar iniciativa
        /// </summary>
        /// <returns></returns>
        [Command("fin iniciativa")]
        public async Task FinIniciativa()
        {
            if (_userDiceList.Count == 0)
            {
                await ReplyAsync("No hay ninguna tirada de iniciativa.");
            }
            else
            {
                List<UserDice> sortedList = _userDiceList.OrderByDescending(x => x.Dice).ToList();
                _userDiceList = new List<UserDice>();

                var reply = new StringBuilder();
                int index = 1;

                foreach (UserDice user in sortedList)
                {
                    var str = ($"{index}º -> **{user.Name}**: {user.Dice}");
                    reply.AppendLine(str.ToString());
                    index++;
                }

                await ReplyAsync(reply.ToString());
            }
        }

        /// <summary>
        /// Método para una tirada: obteniendo mayor y suma de resultados
        /// </summary>
        /// <returns></returns>
        [Command("roll")]
        public async Task TiradaDados([Remainder] string msg)
        {
            int cantidadDados, numeroTirada, resultado, total = 0, mayorTirada = 0;
            string mayorSTR, respuesta = "Tiradas de *" + Context.User.Username + "* -> ";
            var rand = new Random();

            List<string> listaCadenas = comprobarMensage(msg);

            if(listaCadenas == null)
            {
                await ReplyAsync("Comando erróneo.");
                return;
            }

            for (int i = 0; i < listaCadenas.Count; i++)
            {
                string element = listaCadenas.ElementAt(i);

                if (!element.Contains('+') && !element.Contains('-'))
                {
                    resultado = 0;
                    string[] dado = element.Split('d', 'D');
                    string operador = "+";

                    if (dado.Length > 1)
                    {
                        cantidadDados = Int32.Parse(dado[0]);
                        numeroTirada = Int32.Parse(dado[1]);

                        //Por cada dado, hacemos la tirada correspondiente
                        for (int j = 0; j < cantidadDados; j++)
                        {
                            var random = rand.Next(1, numeroTirada);
                            if (random > mayorTirada) mayorTirada = random;
                            respuesta = respuesta + "[ " + random + " ] ";
                            resultado += random;
                        }
                    }
                    else
                    {
                        resultado = Int32.Parse(dado[0]);

                        if (i != 0 && listaCadenas[i - 1] == "-") operador = "-";

                        respuesta = respuesta + "(" + operador + resultado + ") ";
                    }

                    if (i == 0 || listaCadenas[i - 1] == "+")
                    {
                        total += resultado;
                    }
                    else if (listaCadenas[i - 1] == "-")
                    {
                        total -= resultado;
                    }
                }
            }

            if (mayorTirada == 1) mayorSTR = "PIFIA";
            else mayorSTR = mayorTirada.ToString();

            await ReplyAsync(respuesta + "-> Mayor: **" + mayorSTR + "**; Total: **" + total + "**");
        }

        private List<string> comprobarMensage(string msg)
        {
            List<string> listaCadenas = new List<string>();

            string regex = @"(?<DICE>[0-9]*([dD][0-9]*)?)((?<OPER>[-+])(?<DICE>[0-9]*([dD]([0-9]*))?))*";
            MatchCollection matches = Regex.Matches(msg.Trim(), regex);

            if (matches.Where(x => x.Success == false).Count() > 0)
            {
                return null;
            }

            foreach (Match match in matches)
            {
                string dice = match.Groups["DICE"].Value;
                string oper = match.Groups["OPER"].Value;

                if (!String.IsNullOrEmpty(dice)) listaCadenas.Add(dice);
                if (!String.IsNullOrEmpty(oper)) listaCadenas.Add(oper);
            }

            return listaCadenas;
        }
    }
}

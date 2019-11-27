﻿using Bot.Extensions;
using Bot.GenericTypes;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MainDatabaseControler.DAO;
using MainDatabaseControler.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Bot.Extensions.ErrorExtension;

namespace Bot.Comandos
{
    public class CustomReactions : GenericModule
    {
        public CustomReactions(CommandContext contexto, params object[] args) : base(contexto, args)
        {

        }

        public async Task acr()
        {
            if (!Contexto.IsPrivate)
            {
                SocketGuildUser usuario = Contexto.User as SocketGuildUser;
                IRole cargo = (usuario as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Ajudante de Idol");

                if (usuario.GuildPermissions.ManageGuild || usuario.Roles.Contains(cargo))
                {
                    string[] comando = Comando;
                    string msg = string.Join(" ", comando, 1, (comando.Length - 1));
                    string[] resposta_pergunta = msg.Split('|');

                    if (resposta_pergunta.Length >= 2 && !string.IsNullOrEmpty(resposta_pergunta[0]) && !string.IsNullOrEmpty(resposta_pergunta[1]))
                    {
                        ReacoesCustomizadas cr = new ReacoesCustomizadas(resposta_pergunta[0].Trim(), resposta_pergunta[1].Trim(), new Servidores(Contexto.Guild.Id), Contexto.Guild.Id);
                        Tuple<bool, ReacoesCustomizadas> res = await new ReacoesCustomizadasDAO().CriarAcrAsync(cr);
                        cr = res.Item2;

                        string resposta = "", pergunta = "";

                        if (resposta_pergunta[0].Trim().Length > 1024)
                        {
                            pergunta = $"{resposta_pergunta[0].Trim().Substring(0, 1020)}...";
                        }
                        else
                        {
                            pergunta = resposta_pergunta[0].Trim();
                        }

                        if (resposta_pergunta[1].Trim().Length > 1024)
                        {
                            resposta = $"{resposta_pergunta[0].Trim().Substring(0, 1020)}...";
                        }
                        else
                        {
                            resposta = resposta_pergunta[1].Trim();
                        }
                        await Contexto.Channel.SendMessageAsync(embed: new EmbedBuilder()
                                .WithDescription(await StringCatch.GetStringAsync("acrCriadaOk", "**{0}**, a reação customizada foi criada com sucesso.", Contexto.User.ToString()))
                                .AddField(await StringCatch.GetStringAsync("trigger", "Trigger: "), pergunta)
                                .AddField(await StringCatch.GetStringAsync("resposta", "Reposta: "), resposta)
                                .AddField(await StringCatch.GetStringAsync("codigo", "Codigo: "), cr.Cod)
                                .WithColor(Color.DarkPurple)
                            .Build());
                    }
                    else
                    {
                        await Erro.EnviarErroAsync(await StringCatch.GetStringAsync("acrErro", "para adicionar uma reação customizada você precisa me falar o trigger e a resposta da reação customizada."), new DadosErro(await StringCatch.GetStringAsync("usoAcr", "trigger | resposta"), await StringCatch.GetStringAsync("exemploAcr", "upei | boa corno")));
                    }
                }
                else
                {
                    await Erro.EnviarErroAsync(await StringCatch.GetStringAsync("acrSemPerm", "você não possui a permissão `Gerenciar Servidor` ou o cargo `Ajudante de Idol` para poder adicionar uma Reação Customizada nesse servidor 😕"));
                }
            }
            else
            {
                await Erro.EnviarErroAsync(await StringCatch.GetStringAsync("dm", "esse comando só pode ser usado em servidores."));
            }
        }

        public async Task dcr()
        {
            if (!Contexto.IsPrivate)
            {
                SocketGuildUser usuario = Contexto.User as SocketGuildUser;
                IRole cargo = (usuario as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Ajudante de Idol");

                if (usuario.GuildPermissions.ManageGuild || usuario.Roles.Contains(cargo))
                {
                    string[] comando = Comando;
                    string msg = string.Join(" ", comando, 1, (comando.Length - 1));

                    if (msg != "")
                    {
                        try
                        {
                            ulong codigo = Convert.ToUInt64(msg);
                            ReacoesCustomizadas acr = new ReacoesCustomizadas(codigo);
                            acr.SetServidor(new Servidores(Contexto.Guild.Id));

                            if (await new ReacoesCustomizadasDAO().DeletarAcrAsync(acr))
                            {
                                await Contexto.Channel.SendMessageAsync(embed: new EmbedBuilder()
                                        .WithColor(Color.DarkPurple)
                                        .WithDescription(await StringCatch.GetStringAsync("dcrOk", "**{0}**, a reação customizada com o codigo: `{1}` foi deletada do servidor.", Contexto.User.ToString(), codigo))
                                    .Build());
                            }
                            else
                            {
                                await Erro.EnviarErroAsync(await StringCatch.GetStringAsync("dcrNenhuma", "não foi possivel deletar uma reação customizada com esse código."));
                            }

                        }
                        catch
                        {
                            await Erro.EnviarErroAsync(await StringCatch.GetStringAsync("dcrNumero", "isso não é um numero."));
                        }
                    }
                    else
                    {
                        await Erro.EnviarErroAsync(await StringCatch.GetStringAsync("dcrSemCodigo", "você precisa me falar o código da reação customizada para que eu possa deletar ela."), new DadosErro(await StringCatch.GetStringAsync("usoDcr", "<código>"), await StringCatch.GetStringAsync("exemploDcr", "1")));
                    }
                }
                else
                {
                    await Erro.EnviarErroAsync(await StringCatch.GetStringAsync("dcrSemPerm", "você não possui a permissão `Gerenciar Servidor` ou o cargo `Ajudante de Idol` para poder remover uma Reação Customizada nesse servidor 😕"));
                }
            }
            else
            {
                await Erro.EnviarErroAsync(await StringCatch.GetStringAsync("dcrDm", "esse comando só pode ser usado em servidores."));
            }
        }

        public async Task lcr()
        {
            if (!Contexto.IsPrivate)
            {
                ReacoesCustomizadas acr = new ReacoesCustomizadas();
                acr.SetServidor(new Servidores(Contexto.Guild.Id));
                ReacoesCustomizadasDAO dao = new ReacoesCustomizadasDAO();
                List<ReacoesCustomizadas> listaRetorno = await dao.ListarAcrAsync(acr);

                if (listaRetorno.Count != 0)
                {

                    int[] restricoes = new int[2];
                    restricoes[0] = 0;
                    restricoes[1] = listaRetorno.Count / 10 + ((listaRetorno.Count % 10 > 0) ? 1 : 0);
                    //Declaracao da memoria extra que esse comando requer
                    DumpComandos.Add(restricoes); //id 00 
                    DumpComandos.Add(listaRetorno); //id 01
                    DumpComandos.Add(1); //id 02 - Armazena a msg
                    DumpComandos.Add(1); //id 03 - Armazena o controlador de reacoes
                    DumpComandos.Add(1); //id 04 - Armazena o tipo de acao (next ou fowarding)

                    await Menu();
                }
                else
                {
                    await Erro.EnviarErroAsync(await StringCatch.GetStringAsync("lcrNenhuma", "o servidor não tem nenhuma reação customizada."));
                }
            }
            else
            {
                await Erro.EnviarErroAsync(await StringCatch.GetStringAsync("dm", "esse comando só pode ser usado em servidores."));
            }
        }

        private Tuple<string, string> CriarPagina(List<ReacoesCustomizadas> listaRetorno, int paginaAtual)
        {
            string respIds = "";
            string respTriggers = "";
            for (int i = paginaAtual * 10; i < listaRetorno.Count && i < ((paginaAtual * 10) + 10); i++)
            {
                ReacoesCustomizadas temp = listaRetorno[i];

                string trigger = "";

                if (temp.Trigger.Length > 25)
                {
                    trigger = $"{temp.Trigger.Substring(0, 25)}...";
                }
                else
                {
                    trigger = temp.Trigger;
                }

                respIds += $"`#{temp.Cod}`\n";
                respTriggers += $"{trigger}\n";
            }

            return Tuple.Create(respIds, respTriggers);
        }

        private async Task Menu()
        {
            int[] restricoes = (int[])DumpComandos[0];
            var retornoStrings = CriarPagina((List<ReacoesCustomizadas>)DumpComandos[1], restricoes[0]);
            IUserMessage msg = null;
            if (retornoStrings.Item1 != "")
            {
                msg = await Contexto.Channel.SendMessageAsync(embed: new EmbedBuilder()
                        .WithTitle(await StringCatch.GetStringAsync("lcrTxt", "Lista das Reações Customizadas:"))
                        .AddField(await StringCatch.GetStringAsync("lcrCods", "Codigos: "), retornoStrings.Item1, true)
                        .AddField(await StringCatch.GetStringAsync("lcrTriggers", "Triggers: "), retornoStrings.Item2, true)
                        .WithFooter($"{restricoes[0] + 1} / {restricoes[1]}")
                        .WithColor(Color.DarkPurple)
                    .Build());

            }

            bool pProximo = false;
            bool pAnterior = false;

            if (restricoes[1] != 1)
            {
                if (restricoes[0] == 0 && restricoes[0] < restricoes[1])
                {
                    pProximo = true;
                }
                else
                {
                    if ((restricoes[0] + 1) != restricoes[1])
                    {
                        pProximo = true;
                        pAnterior = true;
                    }
                    else
                    {
                        pAnterior = true;
                    }
                }
            }

            DumpComandos[2] = msg;
            ReactionControler controler = new ReactionControler();
            DumpComandos[3] = controler;
            if (pAnterior)
            {
                Emoji emoji = new Emoji("⬅");
                await msg.AddReactionAsync(emoji);
                controler.GetReaction(msg, emoji, Contexto.User, new ReturnMethod(AnteriorPagina));
            }
            if (pProximo)
            {
                Emoji emoji = new Emoji("➡");
                await msg.AddReactionAsync(emoji);
                controler.GetReaction(msg, emoji, Contexto.User, new ReturnMethod(ProximaPagina));
            }
        }

        private async Task ProximaPagina()
        {
            DumpComandos[4] = 1;
            await AjustesDeDados();
        }

        private async Task AnteriorPagina()
        {
            DumpComandos[4] = 2;
            await AjustesDeDados();
        }

        private async Task AjustesDeDados()
        {
            int tipo = (int)DumpComandos[4];
            int[] restricoes = (int[])DumpComandos[0];

            if (tipo == 1)
            {
                restricoes[0]++;
            }
            else
            {
                restricoes[0]--;
            }

            DumpComandos[0] = restricoes;
            await ((IUserMessage)DumpComandos[2]).DeleteAsync();
            ((ReactionControler)DumpComandos[3]).DesligarReaction();
            await Menu();
        }

        public async Task TriggerACR(CommandContext context, Servidores servidor)
        {
            ReacoesCustomizadas aCRs = new ReacoesCustomizadas();
            aCRs.SetTrigger(context.Message.Content, servidor);
            Tuple<bool, ReacoesCustomizadas> res = await new ReacoesCustomizadasDAO().ResponderAcrAsync(aCRs);
            aCRs = res.Item2;
            if (aCRs.Resposta != null)
            {
                StringVarsControler varsControler = new StringVarsControler(context);
                new EmbedControl().SendMessage(context.Channel, varsControler.SubstituirVariaveis(aCRs.Resposta));
            }
        }
    }
}

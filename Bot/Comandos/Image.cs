﻿using Bot.Constantes;
using Bot.Extensions;
using Bot.GenericTypes;
using Discord;
using Discord.Commands;
using MainDatabaseControler.DAO;
using MainDatabaseControler.Modelos;
using System;
using System.Linq;
using static MainDatabaseControler.Modelos.Servidores;

namespace Bot.Comandos
{
    public class Image : GenericModule
    {
        public Image(CommandContext contexto, string prefixo, string[] comando) : base(contexto, prefixo, comando)
        {

        }

        public void cat()
        {
            Links links = new Links();
            new ImageExtensions().getImg(Contexto, StringCatch.GetString("catTxt", "Meow"), links.cat);
        }

        public void dog()
        {
            Links links = new Links();
            new ImageExtensions().getImg(Contexto, img: links.dog);
        }

        public void magikavatar()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(Color.DarkPurple);

            string[] comando = (string[])args[1];
            string msg = string.Join(" ", comando, 1, (comando.Length - 1));

            if (!Contexto.IsPrivate)
            {
                Tuple<IUser, string> getUser = new Extensions.UserExtensions().GetUser(Contexto.Guild.GetUsersAsync().GetAwaiter().GetResult(), msg);
                IUser user = null;

                if (getUser.Item1 != null)
                {
                    user = getUser.Item1;
                }
                else
                {
                    if (msg == "")
                    {
                        user = Contexto.User;
                    }
                    else
                    {
                        embed.WithColor(Color.Red);
                        embed.WithDescription("");
                        embed.WithTitle(StringCatch.GetString("magikavatarPessoa", "Eu não encontrei essa pessoa no servidor"));
                        embed.AddField(StringCatch.GetString("usoCmd", "Uso do Comando: "), StringCatch.GetString("usoMagikavatar", "`{0}magikavatar <pessoa>`", (string)args[0]));
                        embed.AddField(StringCatch.GetString("exemploCmd", "Exemplo: "), StringCatch.GetString("exemploMagikavatar", "`{0}magikavatar @KingCerverus#2490`", (string)args[0]));
                    }
                }

                if (user != null)
                {
                    embed.WithDescription(StringCatch.GetString("magikavatarAguarde", "**{0}** estou fazendo magica com o avatar por-favor aguarde", Contexto.User.ToString()));
                    embed.WithImageUrl(StringCatch.GetString(" agikavatarAguardeImg", "https://i.imgur.com/EEKIQTv.gif"));
                    IUserMessage userMsg = Contexto.Channel.SendMessageAsync(embed: embed.Build()).GetAwaiter().GetResult();

                    string avatarUrl = user.GetAvatarUrl(0, 2048) ?? user.GetDefaultAvatarUrl();

                    try
                    {
                        string magikReturn = new HttpExtensions().GetSite($"https://nekobot.xyz/api/imagegen?type=magik&image={avatarUrl}&intensity=10", "message");

                        embed.WithImageUrl(magikReturn);
                        embed.WithDescription("");
                        userMsg.DeleteAsync();
                    }
                    catch
                    {
                        userMsg.DeleteAsync();
                        embed.WithColor(Color.Red);
                        embed.WithDescription(StringCatch.GetString("magikavatarErro", "**{0}** infelizmente a diretora mari roubou a minha magia", Contexto.User.ToString()));
                        embed.WithImageUrl(null);
                    }
                }
            }
            else
            {
                embed.WithColor(Color.Red);
                embed.WithDescription(StringCatch.GetString("magikavatarDm", "Eu so posso pegar o avatar de outras pessoas em um servidor"));
            }

            Contexto.Channel.SendMessageAsync(embed: embed.Build());

        }

        public void magik()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(Color.DarkPurple);

            string[] comando = (string[])args[1];
            string msg = string.Join(" ", comando, 1, (comando.Length - 1));
            string imgUrl = "";

            if (msg != "")
            {
                imgUrl = msg;
            }
            else if (Contexto.Message.Attachments.Count != 0)
            {
                imgUrl = Contexto.Message.Attachments.First().Url;
            }

            if (imgUrl != "")
            {
                embed.WithDescription(StringCatch.GetString("magikAguarde", "**{0}** estou fazendo magica com a imagem por-favor aguarde", Contexto.User.ToString()));
                embed.WithImageUrl(StringCatch.GetString("magikAguardeImg", "https://i.imgur.com/EEKIQTv.gif"));
                IUserMessage userMsg = Contexto.Channel.SendMessageAsync(embed: embed.Build()).GetAwaiter().GetResult();

                if (new HttpExtensions().PegarTamanhoArquivo(imgUrl, out long tamanho) && tamanho < 102400)
                {
                    try
                    {
                        string retorno = new HttpExtensions().GetSite($"https://nekobot.xyz/api/imagegen?type=magik&image={imgUrl}&intensity=10", "message");
                        userMsg.DeleteAsync();
                        embed.WithDescription("");
                        embed.WithImageUrl(retorno);
                    }
                    catch
                    {
                        userMsg.DeleteAsync();
                        embed.WithColor(Color.Red);
                        embed.WithDescription(StringCatch.GetString("mgikErro", "**{0}** infelizmente a diretora mari roubou a minha magia", Contexto.User.ToString()));
                        embed.WithImageUrl(null);
                    }
                }
                else
                {
                    userMsg.DeleteAsync();
                    embed.WithColor(Color.Red);
                    embed.WithDescription(StringCatch.GetString("mgiktamanho", "**{0}** sua imagem é muito poderosa para mim, por favor envie imagens até 100 kb 😥", Contexto.User.ToString()));
                    embed.WithImageUrl(null);
                }
            }
            else
            {
                embed.WithTitle(StringCatch.GetString("magikSemImg", "Você precisa me falar qual imagem você quer que eu faça magica"));
                embed.AddField(StringCatch.GetString("usoCmd", "Uso do Comando:"), StringCatch.GetString("usoMagik", "`{0}magik <imagem>`", (string)args[0]));
                embed.AddField(StringCatch.GetString("exemploCmd", "Exemplo: "), StringCatch.GetString("exemploMagik", "`{0}magik https://i.imgur.com/cZDlYXr.png`", (string)args[0]));
                embed.WithColor(Color.Red);
            }
            Contexto.Channel.SendMessageAsync(embed: embed.Build());
        }

        public void loli()
        {
            ulong id = 0;
            if (!Contexto.IsPrivate)
            {
                id = Contexto.Guild.Id;
            }

            Servidores servidor = new Servidores(id, ((string)args[0]).ToCharArray());

            bool sucesso = new ServidoresDAO().GetPermissoes(ref servidor);
            if (!Contexto.IsPrivate && sucesso && servidor.Permissoes == PermissoesServidores.ServidorPika || servidor.Permissoes == PermissoesServidores.LolisEdition || new AdmsExtensions().GetAdm(new Usuarios(Contexto.User.Id)).Item1)
            {
                Links links = new Links();
                new ImageExtensions().getImg(Contexto, img: links.loli);
            }
            else
            {
                new Ajuda(Contexto, args).MessageEventExceptions(new NullReferenceException(), servidor);
            }

        }
    }
}

﻿using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace LossyBotRewrite
{
    [Group("profile")]
    public class ProfilesModule : ModuleBase<SocketCommandContext>
    {
        private readonly string[] elementNames = { "fc", "sz", "tc", "rm", "cb", "main", "other" };

        [Command]
        public async Task ProfileView() //profile without user tagged, grabs own profile
        {
            CheckProfile(Context.User.Id.ToString()); //check the profile and see if it exists

            await ReplyAsync(Context.User.Mention, false, GetProfile(Context.User).Build()); //reply with an embedbuilder returned from the getProfile method
        }

        [Command]
        public async Task ProfileView(SocketGuildUser user) //profile with user tagged, grabs their profile
        {
            CheckProfile(user.Id.ToString()); //check their profile and see if it exists

            await ReplyAsync(Context.User.Mention, false, GetProfile(user).Build()); //reply with an embedbuilder returned from the getProfile method
        }

        [Command("edit")]
        public async Task ProfileEdit(string field, [Remainder] string value)
        {
            CheckProfile(Context.User.Id.ToString());

            if (value.Length > 130)
            {
                await ReplyAsync(Context.User.Mention + " Input cannot exceed 130 characters!");
                return;
            }
            try
            {
                XDocument doc = XDocument.Load(Globals.path + "profiles.xml");

                doc.Root.Descendants().Where(x => x.Attribute("id").Value == Context.User.Id.ToString()).First().Element("field").SetValue(value);

                doc.Save(Globals.path + "profiles.xml");

                await ReplyAsync(Context.User.Mention, false, GetProfile(Context.User).Build());
            }
            catch (Exception) //throws when a node isn't found (or any other error)
            {
                await ReplyAsync(Context.User.Mention + " Invalid edit!");
            }
        }

        private void CheckProfile(string userId)
        {
            XDocument doc = XDocument.Load(Globals.path + "profiles.xml");
            
            var elements = from p in doc.Root.Descendants("profile") where (p.Attribute("id").Value == userId) select p;
            if(elements.Any()) return;
            
            //Make a new profile
            XElement userElem = new XElement("profile", new XAttribute("id", userId), from el in elementNames select new XElement(el, "N/A")); //Last part creates new XElements for every element in elementNames
            doc.Root.Add(userElem);
            
            doc.Save(Globals.path + "profiles.xml");
        }


        
        private EmbedBuilder GetProfile(SocketUser user)
        {
            EmbedBuilder builder = new EmbedBuilder();
            XDocument doc = XDocument.Load(Globals.path + "profiles.xml");
            
            //straightforward
            builder.WithThumbnailUrl(user.GetAvatarUrl());
            builder.WithColor(Color.Magenta);
            
            string master = "";
            foreach (string i in elementNames)
            {
                master += $"**{i.ToUpper()}**: "; //add the element name to the master string
                master += doc.Root.Elements("profile").Where(x => x.Attribute("id").Value == user.Id.ToString()).First().Descendants(i).First().Value;
                master += "\n"; //add a new line
            }

            builder.AddField(":bookmark: " + user.Username + "'s profile", master);

            return builder;
        }


        
    }
}
#region References
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Discord;
using System.Collections.Specialized;
using System.IO;
#endregion References

#region NameSpace
namespace discord
{
    #region ShiroCord_Class
    class ShiroCord
    {
        #region variables //The public variables for the prorgram
        public static Dictionary<ulong, List<string>> messages = new Dictionary<ulong, List<string>>(); //The msgs the user has sent
        public static Stopwatch uptime = new Stopwatch(); //Uptime of the bot          
        private static string pastekey = string.Empty; //The pastebin userKey          
        public static int time = -20; //The amount of time before can paste again      
        private static DiscordClient bot; //The bot instance object                    
        #endregion variables 

        #region MainEntry //The main() method
        /// <summary>
        /// The main entry point for the program.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            uptime.Start(); //Start the clock
            Console.WriteLine("Started Shiro bot"); //Notify it has started

            /*Send a request to pastebin to sign into the bot account and save the UserKey ID*/
            using (MemoryStream m = new MemoryStream(new WebClient().UploadValues("http://pastebin.com/api/api_login.php", new NameValueCollection() {  { "api_dev_key", "devkey" },
                { "api_user_name", "pastebinusername"}, { "api_user_password", "pastebinpassword" } }))) { using (StreamReader r = new StreamReader(m)) { pastekey = r.ReadToEnd(); Console.WriteLine("Signed into pastebin."); } }

            /*New instance of the discord bot*/
            bot = new DiscordClient();

            /*Connect to the key that shirobot uses*/
            bot.Connect("bottoken");

            /*Add the respond method to the eventhandler*/
            bot.MessageReceived += Bot_MessageReceived;
            bot.UserJoined += Bot_joined;              
            /*-------------------------------------*/

            bot.Wait(); //Wait for further input
        }
        #endregion MainEntry

        #region OnJoinedServer //When someone joins the server
        /// <summary>
        /// When someone joins a server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Bot_joined(object sender, UserEventArgs e)
        {
            e.Server.DefaultChannel.SendMessage(e.User.Mention + " Welcome to " + e.Server.Name + ", the server admin is: " + e.Server.Owner.Name);
        }
        #endregion OnJoinedServer

        #region returnAllNames //Return the name of every user in an array
        //Return all the names of the users.
        private string[] returnAllUsersNames(MessageEventArgs e) { return e.Server.Users.Select(x => x.Name).ToArray(); }
        #endregion returnAllNames

        #region FindCertainUser //Return user with the name parameter
        private static User findUser(User[] users, string name)
        {
            try /*Try and execute the code below*/
            {
                User d = users[0]; /*Set it as the first user in the list given*/
                /*For every user in the array, if their name is the same as the target one, save it*/
                users.ToList().ForEach(x => { if (x.Name.ToLower().Contains(name.ToLower())) d = x; });
                /*If their name does not match it, return it as null*/
                if (d.Name.ToLower() != name.ToLower()) { return null; }
                else return d; /*Else return the new user name*/
            }
            catch(Exception ex) { Console.WriteLine(ex); }
            return null;
        }
        #endregion FindCertainUser

        #region getRoleInfo //Get the roles of a person
        private static string getRoleInfo(User u)
        {
            try /*Try and execute the code below*/
            {
                string info = string.Empty; /*Create new info variable*/
                u.Roles.ToList().ForEach(x => /*For every role the user has*/
                {
                    /*Add the name and color of the role in hex to the info variable*/
                    info += "`[" + x.Name + "]`" + " Color: #" + returnSixSlotHex(x.Color) + "\n";
                });
                return info; /*Return the information*/
            } catch(Exception ex) { Console.WriteLine(ex); } /*Write any errors to the console.*/
            return null; /*Return null if errors happen*/
        }
        #endregion getRoleInfo

        #region returnSixSlotHex //Convert color to #000000 Format
        private static string returnSixSlotHex(Color c)
        {
            /*Return the color code in a 6 character hex format*/
            return c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
        #endregion returnSixSlotHex

        #region uploadToPasteBin //Upload text to pastebin
        private static string uploadToPastebin(string input, string language = "bash")
        {
            try /*Try execute the code below*/
            {
                using (System.Net.WebClient client = new WebClient()) /*Implement idisposable*/
                {
                    /*Send a request to pastebin to upload values of the text document and try and return the link*/
                    MemoryStream streamer = new MemoryStream(client.UploadValues("http://pastebin.com/api/api_post.php", new NameValueCollection()
                    { 
                        /*The developer key of the pastebin*/                  /*The name of the pastebin*/
                        { "api_dev_key", "devkey" }, { "api_paste_name", "ShiroBot_Haul" + new Random(9999).Next() },
                        /*The text of the pastebin*/                            /*Set as unlisted*/           /*The account key*/
                        { "api_paste_code", input }, { "api_option", "paste" }, { "api_paste_private", "1" }, { "api_user_key", pastekey },
                        { "api_paste_format", language }
                    }));
                    return new StreamReader(streamer).ReadToEnd(); /*Read response*/
                }
            } catch (Exception ex) { Console.WriteLine(ex); } /*Catch any errors*/
            return null;
        }
        #endregion uploadToPasteBin

        #region OnMessageRecieved //When someone sends a message
        /// <summary>
        /// When someone sends a message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Bot_MessageReceived(object sender, MessageEventArgs e)
        {
            #region BugFixes
            if (e.User.Name.Contains("ShiroBot")) return; //Ignore itself to stop loops
            #endregion BugFixes

            #region saveMessage
            try //Try and add values
            {
                if (!e.Message.Text.StartsWith("!")) //If it isnt a command save it
                {
                    if (!messages.ContainsKey(e.User.Id)) messages.Add(e.User.Id, new List<string>()); //Add their user if it dosent exist
                    messages[e.User.Id].Add("[" + e.Message.Timestamp + "] " + e.User.Name + ": " + e.Message.Text + "\n"); //Save message
                    Console.WriteLine("Log has been added for ID: " + e.User.Id); //Notify the log has been added
                }
            } catch (Exception ex) { Console.WriteLine(ex); } //Write any errors that occur
            #endregion saveMessage

            #region Iterations_And_Commands
            try //Try and iterate
            {
                #region inputStoring
                /*Add a space so it wont break the split*/
                string input = e.Message.Text + " ";
                /*Store all lowercase letters*/
                List<string> words = Regex.Replace(input.ToLower(), @"\s+", " ").Split(new char[] { ' ' }).ToList();
                #endregion inputStoring  

                #region Non_Commands
                /*Say hi to anyone if they said it*/
                if ((words[0].Length < 3 && words[0].StartsWith("hi")) || words[0].StartsWith("hello") && !Regex.Match(string.Concat(words.ToArray()), @"say |to|\@").Success) { e.Channel.SendMessage(e.User.Mention + " hi"); }

                /*Urban dictionary*/
                if (e.Message.Text.ToLower().Contains("shiro") && e.Message.Text.ToLower().Contains("what is"))
                {
                    string tosend = "http://www.urbandictionary.com/define.php?term=" + Regex.Replace(Regex.Split(e.Message.Text.ToLower(), @"what is an |what is a |what is ")[1], @"\s+", "+");
                    e.Channel.SendMessage(tosend);
                }

                /*And they dont stop coming*/
                if (Regex.Replace(e.Message.Text, @"\'|\`", "").ToLower().Contains("and they dont stop coming")) { e.Channel.SendMessage("and they dont stop coming :ok_hand: :joy:"); }
                #endregion Non_Commands

                #region switch_commands
                string output = string.Empty; //A new variable for posting to pastebin
                switch(e.Message.Text.Split(new char[] { ' ' })[0].ToLower()) //The command
                {

                    case "!help": //The help command
                        if (e.Message.Text.ToLower().Contains("extensions"))
                        {
                            /*Display the extension help*/
                            e.Channel.SendMessage("```When an extension is used, the bots response will be affected by it.\n" +
                                "For example; !roles topastebin() will upload the bots response to pastebin.```");
                        }
                        else
                            /*Display the regular help*/
                             e.Channel.SendMessage("```!8ball {Message}\n!imgfy {Message}\n!showmessages @user\n" +
                                 "!info @user\n!botservers\n!roles @user\n!help extensions\n\nExtensions:\n----> topastebin()```");
                        break; //Close switch

                    case "!8ball": //Random chance responses
                        /*Send a message with a random response from a set of responses*/
                        output = ":notepad_spiral: => " + e.Message.Text.Substring(7) + "\n:8ball: => " + new string[] //responses
                        {
                            "yes", "no", "i dont know D: :sob:", "dunno ask mau", "im not sure but you could always kill yourself :smiley: :gun:",
                            "probably", "YES!", "...yeah no"
                        }[new Random().Next(0, 7)]; /*A random index from 0 to 7*/
                        e.Channel.SendMessage(output); //Send value
                        break; //Close switch

                    case "!imgfy": //For googling something
                        if (!Regex.Match(e.Message.Text.Substring(7), @"[^\w\d ]").Success) //If they are not alphanumeric or numeric or whitespace
                        {
                            /*Send the link*/
                            output = "http://lmgtfy.com/?q=" + Regex.Replace(e.Message.Text.Substring(7), @"\s+", "+");
                            e.Channel.SendMessage(output);
                        }
                        else e.Channel.SendMessage("Do you think im stupid :sob:"); //return a derp message
                        break;

                    case "!showmessages": //Show any past logs
                        try //Try and display
                        {
                            /*Get the user messages that they want to pull*/
                            User databaseuser = findUser(e.Server.Users.Select(x => x).ToArray(), e.Message.Text.Split(new char[] { ' ' })[1].Substring(1));
                            output = string.Concat(messages[databaseuser.Id].ToArray()); //Add to the pastebin link
                            if (messages.ContainsKey(databaseuser.Id)) //If they sent a message
                            {
                                /*Pull the value from the dictionary from their username*/
                                e.Channel.SendMessage("```" + output + "```"); //Concat the database
                            }
                            else e.Channel.SendMessage("That user has not sent a message."); //Error
                        } catch (Exception ex) { Console.WriteLine(ex); } //Show any messages
                        break; //Stop switch statement

                    case "!info": //Shows information about a user
                        /*If they didnt mention a user*/
                        if (!e.Message.Text.Contains("@")) { e.Channel.SendMessage("I dont understand, please mention them with @."); return; }
                        /*Find the user they targetted*/
                        User u = findUser(e.Server.Users.Select(x => x).ToArray(), e.Message.Text.Split(new char[] { ' ' })[1].Substring(1));
                        /*If it returned null, say they dont exist*/
                        if (u == null) { e.Channel.SendMessage("That user does not exist."); return; }
                        string roles = string.Empty; /*Make a new string for the roles*/
                        /*For every role that the user has, add the name and a space*/
                        roles = string.Concat(e.User.Roles.ToList().Select(x => x.Name + ", ").ToArray());
                        roles = roles.Remove(roles.Length - 2);
                        /*Display more information about the user here*/
                        /*-----------------------------------[INFORMATION]--------------------------------------------*/
                        output = e.User.Mention + "```Name:" + u.Name + "\n" + "ID: " + u.Id + "\n" +
                            "Joined At: " + u.JoinedAt + "\n" +"State: " + u.Status.Value + "\n" +
                            "Known Roles: " + roles + "\n" + "Is a bot: " + u.IsBot.ToString() + 
                            "\nIsAdmin: " + u.GetPermissions(e.Channel).ManagePermissions + "```\n"  + u.AvatarUrl;
                        /*--------------------------------------------------------------------------------------------*/
                        e.Channel.SendMessage(output); //Send the message
                        break; //Close switch statement

                    case "!botservers": //Gets the servers the bot is in.
                        string servernames = string.Empty; /*A string variable for the names*/
                        /*For every server the bot is in, list the name and the owner in the servername variable*/
                        bot.Servers.ToList().ForEach(x => servernames += "`[" + x.Name + "]` Owned By: `(" + x.Owner + ")`\n");
                        output = "The servers i'm in are:\n\n" + servernames; //Add it to the output for the pastebin.
                        e.Channel.SendMessage(output); //Send the message.
                        break; //Close the switch statement

                    case "!roles": //Gets the roles of a user
                        output = "Roles: \n\n" + getRoleInfo(e.User); //Add role info to the output for pastebin
                        e.Channel.SendMessage(output); //Send the roles of the person in the chat
                        break; //Close the switch statement
                }
                #endregion switch_commands

                #region emotereply
                if (Regex.Match(Regex.Replace(e.Message.Text, @"\s+|\`", ""), @"┻━┻|┻┻").Success)
                {
                    e.Channel.SendMessage(e.User.Mention + "(╯°□°）╯︵(\\ .o.)\\");
                    e.Channel.SendMessage("┬──┬ ノ( ゜-゜ノ)");
                }
                #endregion emotereply

                #region topaste
                /*If they want to upload the bots text to pastebin*/
                if (e.Message.Text.ToLower().Contains("topastebin()"))
                {
                    /*Send the text to pastebin and also return the link to it*/
                    e.Channel.SendMessage(uploadToPastebin(Regex.Replace(output, @"`", "")));
                }
                #endregion topaste
            }
            catch (Exception ex) { Console.WriteLine(ex); } //Catch any errors that happen
            #endregion Iterations_And_Commands
        }
        #endregion OnMessageRecieved
    }
    #endregion ShiroCord_Class
}
#endregion NameSpace

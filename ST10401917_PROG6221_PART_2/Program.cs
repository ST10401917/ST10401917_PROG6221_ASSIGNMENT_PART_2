using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Speech.Synthesis;
using System.Threading;

namespace ST10401917_PROG6221_PART_2
{
    class Program
    {
        static List<string> chatHistory = new List<string>();
        static SpeechSynthesizer synth = new SpeechSynthesizer
        {
            Volume = 100,
            Rate = 0
        };
        static Random random = new Random();

        // Memory variables
        static string favoriteTopic = null;

        static string lastTopic = "";
        static void Main(string[] args)
        {
            // Play greeting audio
            PlayGreetingAudio("welcomeAudio.wav");

            // Set up the console window
            Console.Title = "Cybersecurity Awareness Chatbot"; // console names
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(new string('=', Console.WindowWidth)); // Top border

            // ASCII ART
            Console.WriteLine(@" 
_________        ___.                         .__            __ ___.           __   
\_   ___ \___.__.\_ |__   ___________    ____ |  |__ _____ _/  |\_ |__   _____/  |_ 
/    \  \<   |  | | __ \_/ __ \_  __ \ _/ ___\|  |  \\__  \\   __\ __ \ /  _ \   __\
\     \___\___  | | \_\ \  ___/|  | \/ \  \___|   Y  \/ __ \|  | | \_\ (  <_> )  |  
 \______  / ____| |___  /\___  >__|     \___  >___|  (____  /__| |___  /\____/|__|  
        \/\/          \/     \/             \/     \/     \/         \/             

                                                              [::::::]
                                                             |  o  o  |
                                                             |   __   |   
                                                             |  \__/  |
                                                              \______/
                                                             /|______|\
                                                            /_||____||_\
                                                              ||    ||    
                                                            ==||====||== 

");

            Console.WriteLine(new string('=', Console.WindowWidth)); // Bottom border

            TypingEffect("Hello! Welcome to the Cybersecurity Awareness Bot!\n");

            // Ask for the user's name
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("What's your name? ");

            Console.ForegroundColor = ConsoleColor.White;
            string userName = Console.ReadLine();

            LoadingEffect();
            Console.ForegroundColor = ConsoleColor.Magenta;
            RespondWithSpeech($"Hi, {userName}! I'm here to help you stay safe online.\n");

            // Asks the user favorite cybersecurity topic to remember
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("What cybersecurity topic are you most interested in? ");
            Console.ForegroundColor = ConsoleColor.White;
            favoriteTopic = Console.ReadLine()?.Trim().ToLower();

            if (!string.IsNullOrEmpty(favoriteTopic))
            {
                LoadingEffect();
                RespondWithSpeech($"Great! I'll remember that you're interested in {favoriteTopic}. It's a crucial part of staying safe online.");
            }
            else
            {
                RespondWithSpeech("Don't have one? No problem! We will explore more cybersecuirty together");
                favoriteTopic = null; // reset if empty
            }

            // Display a security tip of the day
            DisplayTipOfTheDay();

            // Display available topics the user can ask about
            Console.WriteLine(new string('-', 50));
            Console.WriteLine("You can ask about:");
            Console.WriteLine("Type 'explain' to explain what is cybersecurity"); // this is explaining the commands the user can type
            Console.WriteLine("Type 'types' to list the types of cyber attacks");
            Console.WriteLine("Type 'signs' to explains signs that you have been hacked");
            Console.WriteLine("Type 'protect' on how to protect yourself from cyber attacks");


            Console.WriteLine(" - Your favorite topic: " + (favoriteTopic ?? "none yet"));
            Console.WriteLine(" - General questions like \"How are you?\", \"What's your purpose?\", " +
            "and \"What can I ask you about?\"");
            Console.WriteLine(" - Or type 'exit' to quit.");
            Console.WriteLine(new string('-', 50));

            // while loop allows the chatbot to run continuously until the user types "exit
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"\n{userName}: ");
                string userInput = Console.ReadLine()?.ToLower().Trim();

                if (string.IsNullOrEmpty(userInput)) // if the prompt from the user is empty or null
                {
                    LoadingEffect();
                    Console.ForegroundColor = ConsoleColor.Red;
                    RespondWithSpeech("Please enter a valid question."); // if the user does not write anything
                    continue;
                }

                if (userInput == "exit") // if the user wants to exit the application
                {
                    LoadingEffect();
                    Console.ForegroundColor = ConsoleColor.Green;
                    RespondWithSpeech("Stay safe and think before you click! Goodbye!");
                    break;
                }

                HandleUserQuery(userInput, userName);
            }

            // Save chat history to a text file when exiting
            SaveChatHistory();
        }




    }
}

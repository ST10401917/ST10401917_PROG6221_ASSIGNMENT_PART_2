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
        static SpeechSynthesizer synth = new SpeechSynthesizer // this is for the voice chat 
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


            // memory and recall feature
            if (!string.IsNullOrEmpty(favoriteTopic))
            {
                LoadingEffect();
                RespondWithSpeech($"Great! I'll remember that you're interested in {favoriteTopic}. It's a crucial part of staying safe online.");
            }
            else
            {
                RespondWithSpeech("Don't have one? No problem! We will explore more cybersecurity topic together");
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


        static void HandleUserQuery(string input, string userName) // If a valid command is typed, the bot outputs the appropriate response.
        {
            string responseText = "";
            bool foundResponse = false;


            chatHistory.Add($"{userName}: {input}");


            // single responses for quick matches
            Dictionary<string, string> responses = new Dictionary<string, string>
            {
                { "password", "Create strong passwords by using a mix of letters, numbers, and symbols. Avoid using personal info!" },


                {"explain", "In today's fully digital age, cyber attacks are more common than ever. So that is where cybersecurity comes to play in. " +
                 "Protecting systems, networks, and applications from online assaults is called cybersecurity. " +
                 "Usually cyber criminals will target sensitive information to gain access to your personal property " +
                 "and start alterating infomaton on it, or destruction; or seeking financial from the vicitum via ransom, these are cyberattacks "},


                { "how are you", " Hey I am doing good. Hope you doing good as well :) How can I assist you with cybersecurity today?" },
                { "what is your purpose", "I am a friendly chatbot and I am here to help you learn more about cybersecurity :)" },
                { "what can i ask you about", "You can ask me about password safety, phishing, protecting personal data, and general cybersecurity guidance." },
                { "help", "You can ask about passwords, phishing, how to protection yourself, or explain what is cycbersecuirty" }
            };

            // Keyword groups for multi-keyword detection
            Dictionary<string, List<string>> keywordGroups = new Dictionary<string, List<string>>()
            {
                { "password", new List<string> { "password", "strong password", "secure password" } },

                { "explain", new List<string> { "tell", "what" } },
                { "signs", new List<string> { "hacked", "sign" } },
                { "types", new List<string> { "type", "attack", "different" } },
                { "protect", new List<string> { "safe", "protection" } },

                { "phishing", new List<string> { "phishing", "phishing emails" } },
            };


            List<string> passwordTips = new List<string>()
            {
                "A strong password is your first line of defense. Make sure it’s long—at least 12 characters—and includes a mix of uppercase and lowercase letters, numbers, and special characters.",
                "Using the same password on multiple accounts is risky. If one account is compromised, all others could be too. Always use unique passwords.",
                "It’s tough to remember many complex passwords. That’s why using a password manager can help—it's secure and remembers your passwords for you.",
                "Avoid sharing your passwords with anyone, and never store them in plain text or on sticky notes. Stay safe by keeping them private and secure.",
                "If you notice suspicious activity on your account or get login alerts from unknown devices, change your password immediately and enable two-factor authentication.",
                "It's a good habit to update your passwords every few months, especially for important accounts like email, banking, and social media.",
                "Consider using a passphrase instead of a single word. Something like 'Sunshine$Eats!99Blueberries' is both strong and easier to remember.",
                "No worries if your current passwords aren’t perfect. You can start improving them one step at a time—every bit of effort adds up to better security!"
            };



            // Random phishing tips for varied response
            List<string> phishingTips = new List<string>()
            {
                "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
                "Don't click on links from unknown senders; hover over links to check their actual destination.",
                "Look for poor grammar and spelling mistakes in emails, which are common signs of phishing attempts.",
                "If an email urges immediate action or threats, double-check with the company directly.",
                "Keep your software and anti-virus updated to protect against phishing exploits."
            };

            // Random signs tips for varied response
            List<string> signsTip = new List<string>()
            {
                "Your files on your pc have changed without your knowledge or have been erased",
                "You notice that your passwords have changed unexpectedly",
                "Unknown software shows up or starts installation unexpectedly on your pc. When you are not using the computer, it is frequently connecting to the internet.",
                "Your online searches are being redirected." ,
                "Without your interaction, additional browser windows may show up or closes. Unseen anti-virus software scans randomly pop up." ,
                "Your web browser gets more toolbars" ,
                "Money vanishes from your bank account. You get bills or see payments made for transactions you didn't make"

            };


            // Random protection tips for varied response
            List<string> protectTypes = new List<string>()
            {
                "Use strong and unique passwords. Create long passwords with a mix of letters, numbers, and symbols. Avoid reusing the same password across sites to prevent widespread breaches.",
                "Phishing scams. Phishing emails often create urgency or fear to trick you. Watch out for messages asking you to verify accounts or avoid penalties—always double-check with the source.",
                "Safe Browsing. Keep antivirus software updated, avoid suspicious links, and look for secure websites with 'https' and a padlock icon. Use two-factor authentication when possible."
            };

            // different types of random attacks 
            List<string> typesTip = new List<string>()
            {
                "DoS and DDoS attacks – These attacks overload a server or your network with traffic, causing it to crash or become unavailable.",
                "Phishing attacks – Fake messages or websites tricks you into giving away personal or financial information.",
                "Ransomware – A type of malware that locks your files and demands payment to restore access.",
                "Password attacks – Cybercriminals try to steal or guess your passwords to access accounts.",
                "Trojan horses – Malicious software disguised as a normal file or app, which can secretly damage or control your system."
            };



            // the if statements will check what the user has entered 
            string lowerInput = input.ToLower();

            if (lowerInput.Contains("phishing"))
            {
                responseText = phishingTips[random.Next(phishingTips.Count)]; // the random count will display the random responses
                foundResponse = true;
            }

            else if (lowerInput.Contains("password"))
            {
                responseText = passwordTips[random.Next(passwordTips.Count)];
                foundResponse = true;
            }

            // Codetesting tips (random)
            else if (lowerInput.Contains("types"))
            {
                responseText = typesTip[random.Next(typesTip.Count)];
                foundResponse = true;
            }

            else if (lowerInput.Contains("signs"))
            {
                responseText = signsTip[random.Next(signsTip.Count)];
                foundResponse = true;
            }

            else if (lowerInput.Contains("protect"))
            {
                responseText = protectTypes[random.Next(protectTypes.Count)];
                foundResponse = true;
            }


            // if the user wants to find out about a topic. It checks the user responses 

            else if (lowerInput.Contains("more") || lowerInput.Contains("explain") || lowerInput.Contains("why") || lowerInput.Contains("what do you mean"))
            {
                switch (lastTopic)
                {
                    case "phishing": // if the user types phishing, the chat will display more phishing facts 
                        responseText = phishingTips[random.Next(phishingTips.Count)];
                        foundResponse = true;
                        break;
                    case "signs":
                        responseText = signsTip[random.Next(signsTip.Count)];
                        foundResponse = true;
                        break;
                    case "types":
                        responseText = typesTip[random.Next(typesTip.Count)];
                        foundResponse = true;
                        break;
                    case "protect":
                        responseText = protectTypes[random.Next(protectTypes.Count)];
                        foundResponse = true;
                        break;
                    default:
                        responseText = "Could you please clarify what you'd like to know more about?";
                        foundResponse = true;
                        break;
                }
            }





            // Direct match from responses
            else
            {
                foreach (var entry in responses)
                {
                    if (lowerInput.Contains(entry.Key))
                    {
                        // Special handling if it's codetesting
                        if (entry.Key == "types")
                        {
                            responseText = typesTip[random.Next(typesTip.Count)];
                        }
                        else
                        {
                            responseText = entry.Value;
                        }

                        foundResponse = true;
                        break;
                    }
                }

                // Synonym check
                if (!foundResponse)
                {
                    foreach (var group in keywordGroups)
                    {
                        foreach (var keyword in group.Value)
                        {
                            if (lowerInput.Contains(keyword))
                            {
                                if (group.Key == "types")
                                {
                                    responseText = typesTip[random.Next(typesTip.Count)];
                                }
                                else
                                {
                                    responseText = responses[group.Key];
                                }

                                foundResponse = true;
                                break;
                            }
                        }
                        if (foundResponse) break;
                    }
                }
            }



            // the sentiment detection system
            List<string> SentimentworriedKeywords = new List<string> { "worried", "scared", "nervous", "anxious", "concerned", "afraid" };
            List<string> SentimentcuriousKeywords = new List<string> { "curious", "interested", "wondering", "want to know", "unsure" };
            List<string> SentimentfrustratedKeywords = new List<string> { "frustrated", "angry", "upset", "irritated", "annoyed" };




            // Direct emotion response (handle when input is just a feeling)
            if (SentimentworriedKeywords.Any(input.Contains))
            {
                responseText = "It's completely understandable to feel that way. Scammers can be very convincing.";
                RespondWithSpeech(responseText);
                return;
            }
            else if (SentimentcuriousKeywords.Any(input.Contains))
            {
                responseText = "I'm glad you're curious! Cybersecurity is a great topic to learn about. What would you like to know more about?";
                RespondWithSpeech(responseText);
                return;
            }
            else if (SentimentfrustratedKeywords.Any(input.Contains))
            {
                responseText = "I hear you. It can be frustrating sometimes. Let's break things down or try a different approach together.";
                RespondWithSpeech(responseText);
                return;
            }

            // memory and recall feature
            if (foundResponse && favoriteTopic != null && lowerInput.Contains(favoriteTopic.ToLower()))
            {
                responseText += $" Since you're interested in {favoriteTopic}, remember to stay updated on the latest best practices.";
            }




            // If no valid match
            if (!foundResponse)
            {
                responseText = "Sorry, I didn't understand. Type 'help' to see what you can ask.";
                if (favoriteTopic != null)
                {
                    responseText += $" By the way, since you're interested in {favoriteTopic}, I recommend exploring that topic more!";
                }
            }

            // SINGLE FINAL RESPONSE
            RespondWithSpeech(responseText);


        }







        static void PlayGreetingAudio(string filePath) // method for the audio
        {
            try
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath); // get the path of the code

                if (File.Exists(fullPath)) // checks to see if the path for the audio exists
                {
                    SoundPlayer player = new SoundPlayer(fullPath);
                    player.PlaySync(); // plays the sound
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: The file '{filePath}' was not found."); // if the file is not found
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error playing audio: {ex.Message}");
            }
        }


        static void DisplayTipOfTheDay()  // displaying the tip of the day
        {
            string[] tips = new string[]
            {
            "More general cyber attacks: Amazon now daily monitors hundreds of millions more possible cyber hazards. In only six or seven months, it went from 100 million to 750 million threats each day.",
            "More DDoS attacks: in contrast to last year, Distributed Denial of Service (DDoS) assaults grew by 46% in the first half of 2024. Tech and gaming took the worst blows.",
            "More attacks of ransomware: These days big ransomware assaults are much more frequent. In 2011, Only Five major assaults annually were reported. But every day in 2024, 20 to 25 big ransomware assaults occurred which is super scary."
            };

            int tipIndex = random.Next(tips.Length);
            LoadingEffect();
            Console.ForegroundColor = ConsoleColor.Green;
            RespondWithSpeech($"Security Tip of the Day: {tips[tipIndex]}");
        }

        static void TypingEffect(string message, int delay = 20) // the typing effect for the chat
        {
            foreach (char c in message)
            {
                Console.Write(c);
                Thread.Sleep(delay);
            }
        }

        static void LoadingEffect()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("ChatBot");
            for (int i = 0; i < 2; i++)
            {
                Thread.Sleep(150);
                Console.Write(".");
            }
            Console.WriteLine();
        }




        static void SaveChatHistory() // saves the last chat history to a text file
        {
            string path = "chat_history.txt";
            File.WriteAllLines(path, chatHistory);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"Chat history saved to {path}");
        }

        static void RespondWithSpeech(string response)
        {
            LoadingEffect();
            Console.ForegroundColor = ConsoleColor.Green;
            TypingEffect($"ChatBot: {response}\n");

            try
            {
                synth.Speak(response); // Reliable speech output
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"TTS Error: {ex.Message}");
            }

            chatHistory.Add($"ChatBot: {response}");
        }


    }
}

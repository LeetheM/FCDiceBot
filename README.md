# FCDiceBot
A chatbot for F-List chat with support for running games in chatrooms.

Includes decks of cards and better rolling options.

Made in VS 2013 C#
Libraries: Netwonsoft.Json, websocket-sharp

In order to connect, the bot assumes you'll have a file to read account data from saved as C:\BotData\DiceBot\account_settings.txt (see FCDiceBot/FChatDicebot/FChatDicebot/SavedData/AccountSettings.cs for the object format, should be saved in JSON). The code could be modified to manually create this object without loading it from file in BotMain.cs > LoadAccountSettings().


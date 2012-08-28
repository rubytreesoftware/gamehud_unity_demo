Welcome to GAMEhud!  We provide video game analytics and monitoring for your live games.

Our Unity API wrapper allows you to easily track your game using the GAMEhud service at https://www.mygamehud.com.  Our wrapper provides the following features:
- Automatically handles registration of new machines.
- Automatically handles the starting and stopping of game sessions.
- Provides an Event Queue to batch events locally and then send them on a time and/or event basis.
- Sends events asynchronously so as not to interrupt game play. 
- Provides a number of overload methods to easily log events.

Here are the steps to help you get started tracking your game:

1) Register for a FREE account at https://www.mygamehud.com
2) Once you have logged in, add your game to the Games List
3) Visit the Settings page for your game and copy down your Game API Key.  You will need this to communicate with our API.
4) Import the gamehud_unity_wrapper asset package into your Unity game.  Found at https://www.mygamehud.com/unity_api_support.
5) Drag the "GAMEhud Manager" prefab from the "GAMEhud Assets" directory to the first scene of your game.
6) Adjust the public variables on the GAMEhud prefab using the inspector.  You need to set the Game API Key you got from step 3.  Make any other configuration changes you want in the inspector.
7) Fire up your game!  The wrapper automatically registers your machine and starts and stops play sessions.
8) You can send Unity logging information (warnings, errors, exceptions) by configuring the "GAMEhud Manager" using the inspector.
9) Custom events can be sent by inserting GameHudEventQueue.Log() in your game scripts.  Please check the GameHudEventQueue script for the various overloads.
10) Check your account at https://www.mygamehud.com for your logged statistics.

If you want to check out a very basic unity game demo showing integration, checkout our repository on Github at https://github.com/rubytreesoftware/gamehud_unity_demo.

For more detail about our API, check out https://www.mygamehud.com/api_support.

If you encounter any issues or have questions, please don't hesitate to contact us using the Support link on the left side of any web page at https://www.mygamehud.com.  Or, email us at support@rubytreesoftware.com. 

Lastly, please feel free to check out our blog and post a comment at http://blog.mygamehud.com.

Sincerely,
Creston and Chris (The GAMEhud Team)
# RAScraping
This program scrapes the RetroAchievements site to find updates in tracked users/games.

Simply create a JSON file with a list attribute "Usernames", storing the usernames of the accounts that you want to track.

When you run the program, it will prompt you for an email address to send updates to the tracked data to and from, or you can save this data in a local plaintext JSON file.

Each time the program is run, it visits the RetroAchievements page for every game that has tracked data saved in the /data/games/ directory, and compares the data stored in that game's JSON file to the current data on that game's site page. It updates the local file with any changes, and notes those changes in the email that it sends out to you upon completion of scraping every stored page.

<p align="center">
  <img src="/images/main_data.png" width="350" />
  <br />
  <em>An example of the main_data.json file.</em>
</p>

<br />

<p align="center">
  <img src="/images/files.png" width="350" />
  <br />
  <em>An example of the layout of the /data/games/ directory after tracking multiple users.</em>
</p>

<br />

<p align="center">
  <img src="/images/game_data_example.png" height="350" />
  <br />
  <em>An example of one of the json files tracking data for a specific game.</em>
</p>

<br />

<p align="center">
  <img src="/images/email_example.png" width="350" />
  <br />
  <em>An example of the final resulting email sent out after tracking several updates across various site pages.</em>
</p>

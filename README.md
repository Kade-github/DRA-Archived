### / ! \ WARNING THIS IS IN BETA THERE IS BUGS! / ! \
## Known Bugs
- Nukes don't work.
- Force MTF/CI Respawns don't work.

# Desktop Remote Admin (AKA DRA)
## What is it?
This is a plugin that is based on multiple control panels for other servers, but this time its on your desktop, as a desktop app!

## How do i use it?
Well first, download the ZIP from releases, then put the .dll in your sm_plugins folder. Then you add this to your scp:sl config:
`dra_password: password`, change password to what ever you want. Remeber that! Its how you access your panel.
Then extract the UI, then run the server, and the UI. Get your server IP then your servers DRA Password Make sure you set your port with `dra_port: portNumber` and put that into the portbox, **and make sure its portforwared.** And click connect. You should have access to everything now!

## Screenshots
![DRA](https://i.imgur.com/VdKslYw.png "Main UI")

## Config
```
Key             Default Value
dra_password: <none you need to set it or the plugin wont work> - The password to be used when logging in.
dra_status: true - Disable or Enable the plugin!
dra_port: 7790 - The port used to login!
dra_logs: false - Logs level, true = a bunch of logs. false = little logs.
```

## Contributing
You can do it, i'll look thru it. And we can get to it! (rhyme inteded)

### T O D O  L I S T
- Fixing the bugs
- ~~Adding remeber me functionailty~~
- ~~Optimizing the code a little more.~~
- ~~Adding more config options.~~

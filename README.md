# Sharing session states between Asp.Net web apps (on .Net Framework) and Asp.Net Core apps

## Sessions are stored on REDIS

### How to use

- Install Fanex.SessionStateProvider.Redis into your Asp.Net web apps (on .Net Framework), configure the custom session state provider accordingly. Refer the Fanex.SessionStateProvider.Redis.Example for details of configuration
- Install Fanex.SessionStateProvider.AspNetCore.Redis into your Asp.Net Core apps, configure the services and pipelines accordingly. Refer Fanex.SessionStateProvider.AspNetCore.Redis.Example's Startup class for details of configuration
- All apps have to point to the same REDIS cluster
- Using cookieless or sharing cookies between subdomains to ensure the sessionId could be shared between apps

### Advantages

- All legacy apps and Asp.Net Core apps take the fairly good performance, depending on how the REDIS cluster is managed

### Disadvantages

- Need to test around the legacy apps to make sure there is no other exceptions

## Sessions are stored on Asp.Net Session State Server

- Host Fanex.SessionStateProvider.StateServerApi on a Windows machine, connect it to the Asp.Net Session State servers which are being used by other legacy apps
- Install Fanex.SessionStateProvider.AspNetCore.StateServer into your Asp.Net Core apps, configure the services and pipelines accordingly. Refer Fanex.SessionStateProvider.AspNetCore.StateServer.Example's Startup class for details of configuration
- Leave the Asp.Net web apps (on .Net framework) alone
- Using cookieless or sharing cookies between subdomains to ensure the sessionId could be shared between apps

### Advantages

- Legacy apps stay untouched

### Disadvantages

- Only work with primary types, string, and some .Net built in types, because of the way Asp.Net SessionStateModule serializes data
- The performance of Asp.Net Core apps will not really good, as the session accesses have to go through an intermediary layer

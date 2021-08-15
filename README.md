# websocket-manager
A .NET Core manager for WebSocket

## Overview 
In a cloud environment, multiple instances of an application are deployed. Scalability of the app is the responsibility of cloud and not the application itself. Number of instances can be changed at runtime without the knowledge of the app.
There are cases that we need some sort of visibilities to the instances. We also need to communicate to the instances. Here is a couple of examples:

## Configuration Change Event
Imagine we want to have a dynamic way of updating configuration. This means when we update a configuration repository, all instances of the application should pick the updated one without new deployment or restarting the app.

![image](https://user-images.githubusercontent.com/4038609/129484715-d05d46fa-3099-4221-b0f6-e3509b13879c.png)

 
Think of a manager service that pushes a new message any time a change occurred in the configuration repository. 
Manager service have no knowledge of instances. Instead, it sees the application as a single entity in the cloud. 
There is a decent solution from SpringCloud out there which uses GitLab hooks and RabbitMq to deliver changes to app instances. This solution required a separate application to deliver those messages.

## Memory Cache Force Refresh Command
A similar scenario is when we want to have a way to tell all instances to reset their memory cache. 
![image](https://user-images.githubusercontent.com/4038609/129484720-901c52eb-3250-435a-9676-14a4a5973fe8.png)

In this case there would be an admin tool (or some other automated service) to send the cache force refresh command.
Same problem here blocks the admin tool to see the instances. It can only connect randomly to one instance only. 
SpringCloud solution can be used here too. 

## WebSocket Solution
WebSocket connections can be created between application instances and the manager app (or admin tool). 
![image](https://user-images.githubusercontent.com/4038609/129484729-0e99ce57-ed09-4886-b578-703dfc76880e.png)
![image](https://user-images.githubusercontent.com/4038609/129484734-4d743980-8783-414d-a148-5209b7516833.png)

In this solution, a piece of code will be added to the app itself and the external service (WebSocket Server). Each of the instances will create a bi-directional WebSocket connection to WebSocket Server. 
The connections are constantly monitored. Therefore, when a connection dropped, the other end is aware of it and tries to reconnect. If the server goes down, all clients retry to connect every few seconds. As soon as the server is back, they will reconnect. If a client goes down, it will be removed from the server connection list. Then it knows not to communicate with that server anymore. When a client comes back, or a new client is added, a new connection will be added to the server connection list.

## Advantages
-	WebSocket is Bidirectional
-	It is really fast
-	Doesn’t need any third-party tool like RMQ
-	We can repurpose the solution for many use cases (including configuration, cache force refresh, …)

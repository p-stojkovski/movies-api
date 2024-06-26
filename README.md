# From Zero to Hero: REST APIs in .NET

### What is REST?
- REpresentational State Transfer
- REST is an architectual style for building distributed hypermedia systems.

### Constrains of REST
1. Uniform interface
 - Clean defined interface between the client and the server.
 - Simplify and decouple the architecture allowing each system to evolve independently.
 - Idenfification of resources
     - Clear way to identify the resource(entity) between the client and the server)
 - Manipulation of resources through representations
     - The resources should have uniform representation in the server responses and the client should be able to use those representations to modify the state in the server.
 - Self descriptive messages
     - Each resource representation should carry all the information it needs for the message to be processed.
 - Hypermedia as the engine of application state
     - The client needs to have only the inital url and then the server through the responses will tell the client how to call different things.
2. Stateless
3. Cacheable
4. Client server
5. Layered system
6. Code on demand (optional)

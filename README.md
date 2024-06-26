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
 - Extremly important
 - Mandates when a client sends a request to the server that message contains everything there is to know by the server to process that request. The server cannot rely on existing state that might have gotten in a previuos request.
 - If there is any state that needs to be kept by the client, it needs to be kept on the client.
3. Cacheable
 - The server should imlicitly or explicitly let the client know if it can cache that response or not and for how long. However it is up to the client to bypass that if the client wants to.
4. Client-server
 - Agreed on contracts
5. Layered system
 - Client cannot know if is directly connected to the end server or some load balancer.
6. Code on demand (optional)
 - Server can send code to the client to run.

### Resource naming and routing
- GET /movie**s**
- GET /movies/id
- GET /movies/id/ratings
- GET /ratings/me
- POST/PUT/DELETE /movies/id/ratings

### HTTP Verbs are meaningful
When we want to specify intent about an action we want to make we are going to use HTTP Verbs to describe that!
- POST - Create (create order, add item in basket, create customer...) 
- GET - Retrieve (give me custmer, customers...)
- PUT - Complete update
- PATCH - Partial update
- DELETE - Delete (delete customer, remove item from basket)

### Using response codes to indicate status
- POST
   - Single resource (/items/id): N/A
   - Collection resource (/items): 201 (Location header) -> link to newly created resource, 202
- GET
   - Single resource (/items/id): 200, 404
   - Collection resource (/items): 200
- PUT
   - Single resource (/items/id): 200, 204, 404
   - Collection resource (/items): 405
- DELETE
   - Single resource (/items/id): 200, 404
   - Collection resource (/items): 405

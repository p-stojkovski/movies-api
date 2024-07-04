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

### Flexible response body options
Json
{
  "Name": "Petar"
}

Accept: application/xml
<xml>

### Understanding Idempotency
- No mather how many time you process a specific request, the result will always be the same on the server.
- POST - Not Idempotent
- GET - Idempotent
- PUT - Idempotent
- DELETE - Idempotent
- HEAD -Idempotent
- OPTIONS - Idempotent
- TRACE - Idempotent

### HATEOAS (Hypermedia as the Engine of Application State)
{
  "departmentId": 10,
  "departmentName": "Administrator",
  "locationId": 1700,
  "managerId": 200,
  "links": [
    {
      "href": "10/employees",
      "rel": "employees",
      "type": "GET"
    }
  ]
}

{
  "account": {
    "account_number": 12345,
    "balance": {
      "currency": "usd",
      "value": 100.00
    }
  },
  "links": [
    {
      "deposits": "/accounts/12345/deposits",
      "withdrawals": "/accounts/12345/withdrawals",
      "transfers": "/accounts/12345/transfers",
      "close-requests": "/accounts/12345/close-requests",
    }
  ]
}

### The different types of errors
There are two categories: 
1. Error
- When the client is sending invalid data -> 400
2. Fault
- There is somethig bad with the server -> 500, the request was valid, but something on the server happened that could not be processed.

### Authentication and Authorization in REST APIs
- Authentication -> Process of verifying of who the user is.
- Authorization -> Process of verifying of what the user can do.
- Issuer -> Who the token is generated from.
- Audience -> Who the token is indented for.
- `{
    "userId": "82a241fb-e454-439c-b0a9-ff31bfad79cb",
    "email": "petar@test.com",
    "customClaims": {
        "admin": true,
        "trusted_member": true
    }
  }`

### Why partial updates (PATCH) are not used?
- Complex to build and process the patch request.
- You need to have a path to the item
- Simpler to Get the item, change it and use PUT.

![image](https://github.com/p-stojkovski/movies-api/assets/3589356/b947f9de-f1b2-4a47-b8db-e5de74a19b79)

### What is Swagger?
- Is a way for describing language agnostic REST apis and it is all about specification.
- It allows both humans and computers to understand the capabilities of apis in terms of what it can or cannot do and how the contracts look like using standardized format.

Goals:
- Minimize the time needed to document an api
- Easier to integrate with api without need to be connected with the team implementing the api

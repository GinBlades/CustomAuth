# ASP.NET Core MVC Custom Authentication

I'm not quite sold on Microsoft's Identity Framework yet, and it will be an education experience for me to try setting up
an authentication and authorization process myself. Some things I want to do differently:

* User ID as an integer
* Using [flag enums](http://stackoverflow.com/questions/8447/what-does-the-flags-enum-attribute-mean-in-c) to manage roles.

Some things that I want to keep from Identity Framework

* Attribute authorization for controllers and actions.
* Allow at least Google+, Facebook and Twitter as Oauth providers.

I don't really expect to make this project extensible. My goal is just to learn more about the authentication and authorization process
and then apply this to my future applications.

## Packages

### User Secrets

User secrets will be used to store database configuration.

    ConnectionStrings:DefaultConnection
    User ID=username;Password=secretpassword;Host=localhost;Port=5432;Database=dbname;Pooling=true;


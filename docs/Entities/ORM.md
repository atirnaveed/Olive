# Olive ORM (Object Relational Mapping)

Olive entities are basic C# objects. You can therefore use them with any ORM technology, such as *Entity Framework*. Though, they won't invoke the lifecycle events, and you will need to be careful in how you write your business logic related to object lifecycle.

Olive provides an ORM framework that is specifically designed for easier use, higher flexibility and better performance than Entity Framework or almost any other ORM technology. For example, in executing queries, it's on average twice as performant as Entity Framework.

## IDatabase
The `IDatabase` concept provides you with a facade, or an entry point, for all data operation requirements. Its default implementation class, named `Database`, will be sufficient for almost all applications. Though, you will be able to provide your own implementation if required.

It provides an extremely simple API to do all common data querying and manipulation scenarios including querying datasets, saving, deleting, bulk inserts, etc. It is designed to work with Olive based entities, i.e. your custom business classes that implement `IEntity`.

All of its operations are `async`, which ensures the highest throughput and performance in modern applications.

For example, to insert a record in the database, you will use a simple command such as:
```csharp
await Database.Save(new Customer { Name = "My customer" });
```

### How to access IDatabase?
To gain an instance of `IDatabase`:
- In an ASP.NET `Controller` classes, simply use the `Database` property (inherited from base classes).
- In your custom Olive based entity classes, you can use the `Database` property (inherited from base classes).
- In custom classes (such as Services) use the standard ASP.NET dependency injection.

## Database.Get<T>(id)
The `Get()` method returns a single object from the database. You will specify the entity type as its generic argument, and provide the object ID as the method's parameter. For example:
```csharp
var myCustomer = await Database.Get<Customer>(myId);
```
The type of the `id` parameter should match the entity type. For example, if your type inherits from `GuidEntity` the ID value should be a valid `Guid`. You can however, provide an `object` parameter (i.e. untyped) as longs as the content is a valid Guid. Alternatively, you can use another overload that takes a `string` object. It will automatically convert the string version of the Guid value before querying the database.
  
### GetOrDefault<T>(id)
With the `Get()` method, an `ArgumentNullException` will be thrown if you pass an empty argument such as `null`, `string.Empty` or `Guid.Empty`. If you send an invalid ID or when no record is found with the ID and type that you specify, an `ArgumentException` will be thrown.

In such cases, if you would rather receive a `null` object back instead of an exception, use `Database.GetOrDefault<T>(...)` instead.

### Polymorphic
The `Get()` and `GetOrDefault()` methods support polymorphism and handle inheritence correctly. For example imagine you have a type named `CorporateCustomer` that inherits from `Customer`. What happens when you specify the generic type argument of the as the base class (i.e. `Customer`) while passing in the ID of an actual `CorporateCustomer` record? Well, Olive will handle it automatically and return the correct object to you:
```csharp
Customer myCustomer = await Database.Get<Customer>(idOfACorporateCustomer);
Assert.IsTrue(myCustomer is CorporateCustomer);
```

This works with interfaces as well. This means that if you have 2 different types, both implementing the same interface, when you invoke `Get<ISomething>()`, the right object will be returned, with the right type. Internally, the system may send multiple database querying to all matching tables to retrieve the record for you.

## Database.FirstOrDefault<T>({criteria})
If you have an object's ID, then with the `Get()` method you can obtain the object. But what if, rather than its ID, you have another value or condition, with which to retrieve an object? Well, that's where the `FirstOrDefault()` method comes in handy.
  
Just like the `Get()` method, the `FirstOrDefault()` method allows you to retrieve a single object from the database, rather than a set of objects. Its argument is, however, a predicate, or a lambda function that returns a `boolean`. This method returns the first matched record of the provided Entity Type from the database. Or, if no record is found, it returns `null`.

```csharp
var myCustomer = await Database.FirstOrDefault<Customer>(c => c.Name == myName && c.Category == someCategory);
```
> The framework will automatically convert the provided lambda expression into the equivalent SQL command. It recognised many common patterns, from direct value comparisons to string functions such as `Contains()`, `StartsWith()`, etc. If your expression is not understood, or convertible to the equivalent SQL, then an exception will be thrown.

It also provides other overloads for more advanced querying scenarios. For example when you have dynamic conditions (rather than statically typed lambda expressions), or when you want to provide a sorting expression.

Just like `Get()`, the `FirstOrDefault()` method also support polymorphic queries.

### IDataProvider


The Olive ORM framework provides the following essential components.


### IDatabase

### IDatabaseQuery<T>
  



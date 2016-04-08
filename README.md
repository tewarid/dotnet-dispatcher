Simple asynchronous message dispatcher.
```csharp
// Register a handler for "foo" that accepts messages of type string
MessageDispatcher<string, string>.RegisterHandler("foo", delegate (string message)
{
    // ...
});

// Dispatch a message
MessageDispatcher<string, string>.Dispatch("foo", "bar");
```

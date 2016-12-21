# Simple in-process asynchronous message dispatcher

Dispatch a message thus

```csharp
MessageDispatcher<string, string>.Dispatch("foo", "bar");
```

Register a handler to receive a message thus

```csharp
// Register a handler for "foo" that accepts messages of type string
MessageDispatcher<string, string>.RegisterHandler("foo", delegate (string message)
{
    // ...
});
```

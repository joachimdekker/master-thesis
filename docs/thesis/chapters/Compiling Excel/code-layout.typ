= Code Layout Model

The code layout model is the model that provides structural guidance in emitting correct C\# code and ease final transformation on the code. The model can be compared with the Roslyn Compiler model, but is simplified in many instances. For example, many of the tokens are implicit in the code layout model that have to be declared in Roslyn. 

```cs
record Class(string Name, Property[] Members, Method[] Methods);

record Method(string Name, Statement[] Body)

record Statement();
  -> record Declaration(string Variable, Type type, Expression Declaration);
  -> record Return(Expression Declaration);

record Expression();
  -> record List()
  -> record FunctionCall(string Name, Expression[] args)
  -> record Constant(Type type, object Value)
  -> record Property(Type type, object Self, string name)
```
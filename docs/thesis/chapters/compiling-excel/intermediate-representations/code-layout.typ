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

The Roslyn Compiler Model allows for the complete modification of the syntax of the language. However, this can also result in invalid C\# syntax. For instance, C\# has a `var` type that automatically infers the type at compile time, like the `auto` type in C++. This 'type' can only be used as a direct or simple type, not as part of a complex or generic type, i.e. `var i = 0;` is valid code, but `List<var> = [1,2,3];` is not.

This versatility of the Roslyn Compiler Model can therefore be seen as a downside an threat to validity of our domain model. If the Roslyn Compiler Model is used as an Intermediary Representation, invalid states can be formed, and as such invalid code can be generated. Hence, the Code Layout Model serves as a type-checked, simplified model.

In the upcoming subsections, we discuss the most important elements. Since most of the elements are well-known elements of programming languages, we will not elaborate too much.

== Declarations
The Code Layout model must contain a top level aggregate that houses all the logic of the Excel program. The `Project` entity is this aggregate. Formally, the Project is the complete collection of generated code. It contains the generated types of the repositories and other generated types.

The actual generated code is housed in the `Class`. A Class is a collection of methods and properties. A class can also be a type with properties. The constructor of the class is generated from the properties. [Currently, the constructor of the class is based on the non-computed properties. There is no other way to instantiate the class.] Properties can be computed or not. When properties are not computed, they can be read from and written to. There are no public fields, since they can also be properties and that is the idiomatic way of writing C\# code.

All declarations are named and unique.

== Statements
The IR supports most of the standard statements like variable assignments and variable declarations. 

It is also possible to use expressions as statements in C\#. Roslyn calls this node the `ExpressionStatement`. This name would suggest that every expression can be an statement. However, this is not the case. Only Function Calls and in-/decrements can be used in an `ExpressionStatment`. We don't allow this flexibility to generate invalid syntax. Hence, we don't allow the `ExpressionStatement` on it's own and we have a `FunctionCallStatement` that only contains a `FunctionCall` expression.

== Expressions
Expressions are entities that can be evaluated to a value. In C\#, there are a lot of entities. We have abstracted the most commonly used entities in this intermediary representation. Some entities, like `Constant` and `Variable` are self-contained while other expressions contain other expressions. The self-contained expressions are very basic, and we will not discuss them further.

One of the better abstractions is the `ListExpression`. This expression abstracts a sequence of expressions of the same type. In C\#, there are a lot of types that represent a sequence, most notably the List and the Array, where the List is of variable length and the Array has a fixed length. However, in the context of the compiler, we actually do not really care about the length of the list, as this is mostly fixed at assignment. Therefore, we abstract the List or Array and create an arbitrary `ListExpression` that contains other expressions.

More interesting is the function call. This expression represents a call to a function or member. In general, we distinguish two types of function calls. The first is the general function call of a function in the current scope. This can be member of the current class, or a evocation of a static member such as `Console.WriteLine()`. The other function call is the member call, where we call upon a member of an object. Both function calls are represented by the same object. The function call can contain an expression that should evaluate to an object. If this expression is null, we have a function evocation. 

A different function call is the anonymous function or lambda function. We represent this function with a separate entity. The lambda function is mostly used in functional contexts, such as the LINQ DSL in C\#. [Perhaps some more explanations?]

Closely related to the member function call, the property accessor represents the access to a property of an object. It contains an expression that should be evaluated to an object. This is mostly used within Lambda functions, for instance when using a LINQ expression to select a whole column from a table. In C\# this would be `table.Select(r => r.Column1)` which uses a lambda function.

Finally, it is possible to create an object. This is mostly used when creating the table using internal data. The object creation is based on the constructors of the created type, and contains expressions for the arguments to construct the object.

== Types
In the above sections, we already briefly mentioned that some expressions and classes have certain types. Within the Code Layout representation, we consider types as an important section. The C\# language is a strictly typed language, which means that everything should have a type at compile time. As such, our model requires the typing of every single entity.

Types can be simple or complex. Simple types are types that represent a single value, and are used to construct complex types. Complex types are compositions of simple and complex types. Often, complex types are represented by a class or struct, where methods allow for manipulation of the datatype.

Just like C\#, the model provides both the possiblity for simple and complex types. A special entity is the `ListOf` complex type.  As discussed above, we have the `ListExpression` which models a sequence and is not bound by a List or Array type. Hence, we need to reflect this in our own typing system. As such as ListExpression will always have the ListOf type.
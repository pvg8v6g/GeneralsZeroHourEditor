# Junie Guidelines

**General Stuff**

- always use primary constructors when possible.
- use the `field` keyword wherever possible.
- SetField is a properly constructed method for handling property changes and notifying observers.
- this is a winui3 project and should be treated as such.
- this is STRICT mvvm pattern. do NOT add methods to the view for any reason other than binding the datacontext.
- do not mess with git for any reason other than me telling you to do so.
- the source code for the game engine this is built to mod is https://github.com/electronicarts/CnC_Generals_Zero_Hour.
  this is super important to know because this engine i'm building is to mod this game.
- use region tags for organization.
- Use collection expressions when possible.

**XAML Stuff**

- do not use "margin" unless absolutely necessary. use padding or spacing instead.

# C# Coding Standards: Pattern Matching

## Rule: Prefer Pattern Matching Syntax

Always use modern **C# pattern matching syntax** for type checking, null validation, and conditional expressions. Do not use legacy comparison
operators or manual casting when pattern matching is available.

### 1. Null Checks (Constant Patterns)

Use the **constant pattern** (`is null` and `is not null`) instead of equality operators (`== null`, `!= null`). This ensures safety against
user-overloaded equality operators.

* ❌ **Avoid:** `if (obj != null)`
* ✔️ **Use:** `if (obj is not null)`

### 2. Type Checking and Casting (Declaration Patterns)

Use the **declaration pattern** to test a type and assign it to a variable in a single step. Do not check types using `GetType()` or cast separately
using the `as` operator.

* ❌ **Avoid:**
  ```csharp
  var text = obj as string;
  if (text != null) { /* use text */ }
  ```
* ✔️ **Use:**
  ```csharp
  if (obj is string text) { /* use text */ }
  ```

### 3. Negations and Combinations (Logical Patterns)

Use **logical patterns** (`not`, `and`, `or`) to combine or negate conditional checks instead of traditional logical operators (`!`, `&&`, `||`)
within pattern expressions.

* ❌ **Avoid:** `if (!(obj is string))`
* ✔️ **Use:** `if (obj is not string)`

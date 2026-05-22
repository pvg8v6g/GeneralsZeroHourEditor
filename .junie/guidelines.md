# Junie Guidelines

**General Stuff**
- always use primary constructors when possible.
- use the `field` keyword for databinding.
- SetField is a properly constructed method for handling property changes and notifying observers.
- this is a winui3 project and should be treated as such.
- this is STRICT mvvm pattern. do NOT add methods to the view for any reason other than binding the datacontext.
- do not mess with git for any reason other than me telling you to do so.
- the source code for the game engine this is built to mod is https://github.com/electronicarts/CnC_Generals_Zero_Hour.
  this is super important to know because this engine i'm building is to mod this game.

**XAML Stuff**
- do not use "margin" unless absolutely necessary. use padding or spacing instead.

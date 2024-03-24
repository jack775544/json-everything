* Missing Tests
  * Literal
  * LooseEquals
  * LooseNotEquals
  * Merge
  * Missing
  * MissingSome
  * Reduce
* Behaviour Differences
  * Loose Equals and Strict Equals are the same
    * This is since type casting is going to be necessary. For example, we might want to pass a DateTime in as an
      argument for a logic operation. But JSON has no way of expressing a DateTime except for a string. Therefore, to
      make this operation valid, we have to parse the string as a DateTime and then compare the date times.
  * Merge not implemented
  * Missing not implemented
  * MissingSome not implemented
  * Reduce not implemented
  * Object literals not supported, only arrays, numbers, strings, bools and null
  * Log method does nothing
  * Substring works like C# substring method and less loose than normal JsonLogic one
  * Array literals must have attribute of all the same type, no mixed types

* Differences from [reference](https://jsonlogic.com/operations.html)
  * In C# for the add rule, null is treated as 0. In the reference a null will make the result null
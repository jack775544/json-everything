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
  * Merge not implemented
  * Missing not implemented
  * MissingSome not implemented
  * Reduce not implemented
  * Object literals not supported, only arrays, numbers, strings, bools and null
  * Log method does nothing
  * Substring works like C# substring method and less loose than normal JsonLogic one
  * Array literals must have attribute of all the same type, no mixed types
# GeorgeCloney

Useful extension methods for deep-cloning an object.

There are 3 extension methods available:
* Serialize(this object source) returns uses the BinaryFormatter to return a MemoryStream containing the serialized object.
* Deserialize<T>(this Stream stream) uses BinaryFormatter to recreate an instance of T from the stream. 
* DeepClone(this object source) will return a copy of the object with the same values but not the same instance. 
The clone is created either using serialization, if the source is serializable, or reflection if it is not. When using 
reflection all properties are recursed so that it really is a "deep" clone. 

<pre>
	class Test
	{
		public string Value { get; set; }
		public Test Nested { get; set; }
	}
	
	var A = new Test { Value = "something", Nested = new Test { Value = "else" } };
	var B = A.DeepClone();
	
	// A and B are equal but not the same
	Assert.AreEqual(A.Value, B.Value);
	Assert.AreEqual(A.Nested.Value, B.Nested.Value);
	
	// Modifying B does not change A
	B.Nested.Value = "modified";
	Assert.AreNotEqual(A.Nested.Value, B.Nested.Value);
</pre>
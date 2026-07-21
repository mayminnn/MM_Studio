using System;

namespace Test_Cases.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    // [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TestTagsAttribute : Attribute
    {
        public string[] Tags { get; }

        public TestTagsAttribute(params string[] tags)
        {
            Tags = tags;
        }
    }
}
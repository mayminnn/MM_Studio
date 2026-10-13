using System;

namespace Test_Cases.Attributes
{
    // TestLink IDs covered by this test class.
    // The class name stays the primary key; list every covered ID here (including the class's own).
    [AttributeUsage(AttributeTargets.Class)]
    public class TestLinkIdsAttribute : Attribute
    {
        public string[] Ids { get; }

        public TestLinkIdsAttribute(params string[] ids)
        {
            Ids = ids;
        }
    }
}

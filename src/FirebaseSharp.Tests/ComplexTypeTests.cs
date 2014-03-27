using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace FirebaseSharp.Tests
{
    public class Person
    {
        public Person()
        {
            Children = new List<Person>();
        }

        public string Name { get; set; }

        public int Age { get; set; }

        public Address Address { get; set; }

        public Person Spouse { get; set; }
        public List<Person> Children { get; private set; }

        public override bool Equals(object obj)
        {
            Person p = obj as Person;

            if (p == null)
            {
                return false;
            }

            return Compare(this, p);
        }

        public static bool Compare(Person x, Person y)
        {
            if (x == null || y == null)
            {
                return x == null && y == null;
            }

            if (x.Name == y.Name &&
                x.Age == y.Age)
            {
                if (Address.Compare(x.Address, y.Address) &&
                    Compare(x.Spouse, y.Spouse) &&
                    x.Children.Count == y.Children.Count)
                {
                    return x.Children.All(child => y.Children.Any(c => c.Equals(child)));
                }
            }

            return false;
        }
    }

    public class Address
    {
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }

        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public override bool Equals(object obj)
        {
            Address p = obj as Address;

            if (p == null)
            {
                return false;
            }

            return Compare(this, p);
        }

        public static bool Compare(Address x, Address y)
        {
            if (x == null || y == null)
            {
                return x == null && y == null;
            }

            return
                x.StreetAddress1 == y.StreetAddress1 &&
                x.StreetAddress2 == y.StreetAddress2 &&
                x.City == y.City &&
                x.State == y.State &&
                x.Zip == y.Zip;
        }
    }

    [TestClass]
    public class ComplexTypeTests
    {
        [TestMethod]
        public void ComplexTypes()
        {
            Person me = new Person
            {
                Name = "Me Horvick",
                Age = 37,
                Address = new Address
                {
                    StreetAddress1 = "123 Generic Suburban St.",
                    City = "Raleigh",
                    State = "NC",
                    Zip = "12345",
                },
            };

            Person wife = new Person
            {
                Name = "Spouse Horvick",
                Age = 38,
                Address = me.Address,
            };


            Person kid1 = new Person
            {
                Name = "Kid1 Horvick",
                Age = 17,
                Address = me.Address,
            };

            Person kid2 = new Person
            {
                Name = "Kid2 Horvick",
                Age = 14,
                Address = me.Address,
            };

            me.Children.Add(kid1);
            me.Children.Add(kid2);

            wife.Children.Add(kid1);
            wife.Children.Add(kid2);

            string blob = JsonConvert.SerializeObject(me);

            Portable.Firebase fb = new Portable.Firebase(TestConfig.RootUrl);

            fb.Put("test/people/me", blob);

            string gotBack = fb.Get("test/people/me");

            Person meAgain = JsonConvert.DeserializeObject<Person>(gotBack);

            Assert.IsTrue(me.Equals(meAgain));

        }
    }
}

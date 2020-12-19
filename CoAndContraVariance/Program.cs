using System;
using System.Collections.Generic;

namespace CoAndContraVariance
{
    class Employee
    {
        public string Name { get; init; }
    }

    class Manager : Employee
    { }

    class Boss : Manager
    { }

    interface IRoomEntrance<in T>
    {
        void ComeIn(T person);
    }

    interface IRoomEnumerable<out T>
    {
        IEnumerable<T> EnumeratePeopleInside();
    }

    class PublicRoom : IRoomEntrance<Employee>, IRoomEnumerable<Employee>
    {
        public void ComeIn(Employee person)
        {
            Console.WriteLine($"{person.GetType().Name} {person.Name} came in the public room");
        }

        public IEnumerable<Employee> EnumeratePeopleInside()
        {
            yield return new Manager { Name = "Roma" }; // Every manager is an employee, so there are different employees in the public room.
            yield return new Employee { Name = "Vasya" };
            yield return new Boss { Name = "Boss" }; // Every boss is an employee as well, so there are different employees in the public room.
        }
    }

    class ManagerRoom : IRoomEntrance<Manager>, IRoomEnumerable<Manager>
    {
        public void ComeIn(Manager person)
        {
            Console.WriteLine($"{person.GetType().Name} {person.Name} came in the manager room");
        }

        public IEnumerable<Manager> EnumeratePeopleInside()
        {
            yield return new Manager { Name = "Roma" };
            // yield return new Employee { Name = "Vasya" }; // but not every employee is a manager. There are only managers in the manager room.
            yield return new Boss { Name = "Boss" }; // Every boss is a manager as well, so there can be the boss in the manager room.
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var publicRoom = new PublicRoom();
            var managerRoom = new ManagerRoom();

            var employee = new Employee { Name = "E" };
            var manager = new Manager { Name = "M" };
            var boss = new Boss { Name = "B" };


            publicRoom.ComeIn(employee);
            publicRoom.ComeIn(manager); // manager can entry into the public room
            publicRoom.ComeIn(boss); // boss can entry into the public room

            // managerRoom.ComeIn(employee);
            managerRoom.ComeIn(manager);
            managerRoom.ComeIn(boss);
        }
    }
}

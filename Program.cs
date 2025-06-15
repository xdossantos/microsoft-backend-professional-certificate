using System;

namespace MyApp
{
    public interface IAnimal
    {
        void makeSound();
    }

    public class Animal : IAnimal
    {
        public virtual void makeSound()
        {
            Console.WriteLine("Animal makes a sound");
        }
    }

    public class Dog : Animal
    {
        public override void makeSound()
        {
            Console.WriteLine("Dog barks");
        }
    }

    public class Cat : Animal
    {
        public override void makeSound()
        {
            Console.WriteLine("Cat meows");
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            IAnimal myDog = new Dog();
            IAnimal myCat = new Cat();

            myDog.makeSound(); // Output: Dog barks
            myCat.makeSound(); // Output: Cat meows

            Animal genericAnimal = new Animal();
            genericAnimal.makeSound(); // Output: Animal makes a sound
        }
    }
}

namespace ConsoleApp1;

public class Manager2
{
    public Manager2() { }

    public void Print()
    {
        var mesage = "Hello, World!";
        Console.WriteLine(mesage);


        Console.WriteLine("Bye, World!");

    }


    private void PrintRepeated(int repeat, string text)
    {
        if (repeat < 0) throw new ArgumentException("repeat must be greater than 0");

    }

    private void ExitApp(int exit) => Environment.Exit(exit);
}

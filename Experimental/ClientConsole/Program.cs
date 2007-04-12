namespace ClientConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // このようにサーバとクライアントのプロジェクトを分ける必要はない。
            // しかし同じアセンブリから２つのプロセスを起動するやり方はデバッグしづらいので分けた。
            EventSystem.Program.Main(args);
        }
    }
}
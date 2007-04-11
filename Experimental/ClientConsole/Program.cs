using EventSystem;

namespace ClientConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // サーバとクライアントのプロジェクトを分ける必要はない。
            // しかし同じアセンブリから２つのプロセスを起動するやり方はデバッグしづらいので分けた。
            IServerHost host = new ServerHost();
            host.Run(args);
        }
    }
}
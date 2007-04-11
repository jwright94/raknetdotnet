using EventSystem;

namespace ClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // サーバとクライアントのプロジェクトを分ける必要はなかった。
            // しかし同じアセンブリで２つのプロセスを起動するのは、デバッグしづらいので分けた。            
            ServerHost.Run(args);
        }
    }
}

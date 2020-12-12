using System;
using System.IO;
using System.Threading.Tasks;

namespace DisposingAndFinalizing
{
    class Program
    {
        static async Task Main(string[] args)
        {
            {
                using var gfs = new MyGoodFileStream();
                gfs.Open("file.txt");
                using var bfs = new MyBadFileStream();
                bfs.Open("file2.txt");
                new MyGoodFileStream().Open("file3.txt");
                new MyBadFileStream().Open("file4.txt");
            }

            GC.Collect();
            await Task.Delay(TimeSpan.FromSeconds(5));

            GC.Collect();
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    class MyGoodFileStream : IDisposable
    {
        private FileStream _file;

        public void Open(string path)
        {
            _file = new FileStream(path, FileMode.Open);
        }

        public void Close()
        {
            Console.WriteLine("MyGoodFileStream is closing...");
            _file?.Dispose();
        }

        private void Dispose(bool disposing)
        {
            Console.WriteLine("MyGoodFileStream is disposing...");
            if (disposing)
            {
                Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MyGoodFileStream()
        {
            Dispose(false);
        }
    }

    class MyBadFileStream : IDisposable
    {
        private FileStream _file;

        public void Open(string path)
        {
            _file = new FileStream(path, FileMode.Open);
        }

        public void Close()
        {
            Console.WriteLine("MyBadFileStream is closing...");
            _file?.Dispose();
        }

        private void Dispose(bool disposing)
        {
            Console.WriteLine("MyBadFileStream is disposing...");
            if (disposing)
            {
                Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MyBadFileStream()
        {
            Console.WriteLine("Good bye file resource");
        }
    }
}

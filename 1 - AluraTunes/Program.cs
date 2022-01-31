using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace _1___AluraTunes
{
    class Program
    {
        static void Main(string[] args)
        {
            //_1_LinqToObjects();
            //_2_LinqToXML();
            //_exercicio_1();

            Console.ReadKey();
        }

        private static void _1_LinqToObjects()
        {
            var generos = new List<Genero>
            {
                new Genero { Id = 1, Nome = "Rock" },
                new Genero { Id = 2, Nome = "Reggae" },
                new Genero { Id = 3, Nome = "Rock Progressivo" },
                new Genero { Id = 4, Nome = "Punk Rock" },
                new Genero { Id = 5, Nome = "Clássica" }
            };

            var musicas = new List<Musica>
            {
                new Musica { Id = 1, Nome = "Sweet Child O'mine", GeneroId = 1 },
                new Musica { Id = 2, Nome = "I Shoft the Sheriff" , GeneroId = 2 },
                new Musica { Id = 3, Nome = "Danúbio Azul", GeneroId = 5 }
            };

            var query = from g in generos
                        where g.Nome.Contains("Rock")
                        select g;

            foreach (var genero in query)
            {
                Console.WriteLine("{0}\t{1}", genero.Id, genero.Nome);
            }


            Console.WriteLine();

            var musicaQuery = from m in musicas
                              join g in generos on m.GeneroId equals g.Id
                              select new { m, g };

            foreach (var musica in musicaQuery)
            {
                Console.WriteLine("{0}\t{1}\t{2}", musica.m.Id, musica.m.Nome, musica.g.Nome);
            }

            Console.WriteLine();

            //Crie uma consulta para listar os nomes das músicas cujo gênero tenha o nome "Reggae".

            var queryReggae = from m in musicas
                              join g in generos on m.GeneroId equals g.Id
                              where g.Nome.Equals("Reggae")
                              select m;

            foreach (var item in queryReggae)
            {
                Console.WriteLine(item.Nome);
            }
        }

        private static void _2_LinqToXML()
        {
            XElement root = XElement.Load(@"Data\AluraTunes.xml");

            var queryXML = from g in root.Element("Generos").Elements("Genero")
                           select g;

            foreach (var genero in queryXML)
                Console.WriteLine("{0}\t{1}", genero.Element("GeneroId").Value, genero.Element("Nome").Value);

            Console.WriteLine();

            var query = from g in root.Element("Generos").Elements("Genero")
                        join m in root.Element("Musicas").Elements("Musica")
                            on g.Element("GeneroId").Value equals m.Element("GeneroId").Value
                           select new
                           {
                               Musica = m.Element("Nome").Value,
                               Genero = g.Element("Nome").Value
                           };

            foreach (var musicaEGenero in query)
            {
                Console.WriteLine("{0}\t{1}", musicaEGenero.Musica, musicaEGenero.Genero);
            }
        }

        private static void _exercicio_1()
        {
            XElement root = XElement.Load(@"Data\Exercicio_1.xml");

            var query = from m in root.Element("Musicas").Elements("Musica")
                        join g in root.Element("Generos").Elements("Genero")
                        on m.Element("GeneroId").Value equals g.Element("GeneroId").Value
                           select new
                           {
                               Id = m.Element("MusicaId").Value,
                               Nome = m.Element("Nome").Value,
                               Genero = g.Element("Nome").Value
                           };

            foreach (var item in query)
            {
                Console.WriteLine("{0}\t{1}\t{2}", item.Id, item.Nome, item.Genero);
            }
        }
    }

    class Genero
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }

    class Musica
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int GeneroId { get; set; }
    }
}

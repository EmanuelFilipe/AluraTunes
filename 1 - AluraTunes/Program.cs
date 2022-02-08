using _1___AluraTunes.Data;
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
            //_3_LinqToEntities();
            //_4_AssociandoEfiltrandoEntidades();
            //_5_OrdenandoConsultas();
            //_6_CountTotal();
            _7_FuncoesMatematicas();

            
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

        private static void _3_LinqToEntities()
        {
            using (var context = new AluraTunesEntities())
            {
                var query = from g in context.Generos
                            select g;

                foreach (var item in query)
                    Console.WriteLine("{0}\t{1}", item.GeneroId, item.Nome);

                var faixaEgenero = from g in context.Generos
                                   join f in context.Faixas
                                   on g.GeneroId equals f.GeneroId
                                   select new { g, f };

                faixaEgenero = faixaEgenero.Take(10);

                context.Database.Log = Console.WriteLine;

                Console.WriteLine();
                foreach (var item in faixaEgenero)
                    Console.WriteLine("{0}\t{1}", item.f.Nome, item.g.Nome);
            }
        }

        private static void _4_AssociandoEfiltrandoEntidades()
        {
            string textoBusca = "Led";

            using (var context = new AluraTunesEntities())
            {
                var query = from a in context.Artistas
                            join alb in context.Albums
                                on a.ArtistaId equals alb.ArtistaId
                            where a.Nome.Contains(textoBusca)
                            select new
                            {
                                NomeArtista = a.Nome,
                                NomeAlbum = alb.Titulo
                            };

                //context.Database.Log = Console.WriteLine;

                foreach (var item in query)
                    Console.WriteLine("{0}\t{1}", item.NomeArtista, item.NomeAlbum);

                // mesma query de cima, mas sem o join
                Console.WriteLine();

                var query2 = from alb in context.Albums
                             where alb.Artista.Nome.Contains(textoBusca)
                             select new
                             {
                                 NomeArtista = alb.Artista.Nome,
                                 NomeAlbum = alb.Titulo
                             };

                //context.Database.Log = Console.WriteLine;

                foreach (var item in query)
                    Console.WriteLine("{0}\t{1}", item.NomeArtista, item.NomeAlbum);

                Console.WriteLine();

                string buscaArtista = "Led Zepplin";
                string buscaAlbum = "Graffiti";

                GetFaixas(context, buscaArtista, buscaAlbum);

            }


        }

        private static void _5_OrdenandoConsultas()
        {
            string buscaArtista = "Led Zepplin";
            string buscaAlbum = "Graffiti";

            using (var context = new AluraTunesEntities())
            {
                var query = from f in context.Faixas
                            where f.Album.Artista.Nome.Contains(buscaArtista)
                            && (!string.IsNullOrEmpty(buscaAlbum) ? f.Album.Titulo.Contains(buscaAlbum) 
                            : true)
                            orderby f.Album.Titulo, f.Nome
                            select f;

                foreach (var item in query)
                    Console.WriteLine("{0}\t{1}", item.Album.Titulo, item.Nome);
            }
        }

        private static void _6_CountTotal()
        {
            using (var context = new AluraTunesEntities())
            {
                //var query = from f in context.Faixas
                //            where f.Album.Artista.Nome == "Led Zeppelin"
                //            select f;

                //var quantidade = context.Faixas.Where(f => f.Album.Artista.Nome == "Led Zeppelin").Count();

                //foreach (var item in query)
                //    Console.WriteLine("{0}\t{1}", item.Album.Titulo.PadRight(40), item.Nome);

                //Console.WriteLine("{0}", quantidade);

                var query1 = from inf in context.ItemNotaFiscals
                             where inf.Faixa.Album.Artista.Nome == "Led Zeppelin"
                             group inf by inf.Faixa.Album into agrupado
                             let vendasPorAlbum = agrupado.Sum(a => a.Quantidade * a.PrecoUnitario)
                             orderby vendasPorAlbum descending
                             select new
                             {
                                 TituloDoAlbum = agrupado.Key.Titulo,
                                 TotalPorAlbum = vendasPorAlbum
                             };

                context.Database.Log = Console.WriteLine;

                foreach (var agrupado in query1)
                    Console.WriteLine("{0}\t{1}", 
                        agrupado.TituloDoAlbum.PadRight(40), agrupado.TotalPorAlbum);

            }
        }

        private static void _7_FuncoesMatematicas()
        {
            using (var context = new AluraTunesEntities())
            {
                var maiorVenda = context.NotaFiscals.Max(nf => nf.Total);
                var menorVenda = context.NotaFiscals.Min(nf => nf.Total);
                var mediaVenda = context.NotaFiscals.Average(nf => nf.Total);

                Console.WriteLine("{0}\t{1}\t{2}", maiorVenda, menorVenda, mediaVenda);
                Console.WriteLine();

                var vendas = (from nf in context.NotaFiscals
                             group nf by 1 into agrupado
                             select new
                             {
                                 maiorVenda = agrupado.Max(nf => nf.Total),
                                 menorVenda = agrupado.Min(nf => nf.Total),
                                 mediaVenda = agrupado.Average(nf => nf.Total)
                             }).Single();

                Console.WriteLine("{0}\t{1}\t{2}", vendas.maiorVenda, vendas.menorVenda, vendas.mediaVenda);

                Console.ReadKey();
            }
        }

        private static void GetFaixas(AluraTunesEntities context, string buscaArtista, string buscaAlbum)
        {
            var query = from f in context.Faixas
                         where f.Album.Artista.Nome.Contains(buscaArtista)
                         select f;

            if (!string.IsNullOrEmpty(buscaAlbum))
            {
                query = query.Where(q => q.Album.Titulo.Contains(buscaAlbum));
            }

            foreach (var item in query)
                Console.WriteLine("{0}\t{1}", item.Album.Titulo, item.Nome);
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

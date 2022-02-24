using _1___AluraTunes.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace _1___AluraTunes
{
    class Program
    {
        private const string Imagens = "Imagens";

        static void Main(string[] args)
        {
            //_1_CriandoRelatoriosComPaginacao();
            //_2_SubQueries();
            //_3_ProdutoMaisVendido();
            //_4_SelfJoin();
            //_5_();
            _6_LinqParalelo();

            Console.ReadKey();
        }


        private static void _1_CriandoRelatoriosComPaginacao()
        {
            const int TAMANHO_PAGINA = 10;

            using (var context = new AluraTunesEntities())
            {
                int numeroNotasFiscais = context.NotaFiscals.Count();
                var numeroPaginas = Math.Ceiling((decimal)numeroNotasFiscais / TAMANHO_PAGINA);

                for (int p = 1; p <= numeroPaginas; p++)
                {
                    ImprimirPagina(TAMANHO_PAGINA, context, p);
                }

            }
        }

        private static void _2_SubQueries()
        {
            using (var context = new AluraTunesEntities())
            {
                var queryMedia = context.NotaFiscals.Average(n => n.Total);

                var query = from nf in context.NotaFiscals
                            where nf.Total > queryMedia
                            orderby nf.Total descending
                            select new
                            {
                                Numero = nf.NotaFiscalId,
                                Data = nf.DataNotaFiscal,
                                Cliente = nf.Cliente.PrimeiroNome + " " + nf.Cliente.Sobrenome,
                                Total = nf.Total
                            };

                foreach (var item in query)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}", item.Numero, item.Data, item.Cliente, item.Total);
                }

                Console.WriteLine();
                Console.WriteLine("a média é: {0}", queryMedia);

            }
        }

        private static void _3_ProdutoMaisVendido()
        {
            using (var context = new AluraTunesEntities())
            {
                var query = from f in context.Faixas
                            where f.ItemNotaFiscals.Count() > 0
                            let totalDeVendas = f.ItemNotaFiscals.Sum(inf => inf.Quantidade * inf.PrecoUnitario)
                            orderby totalDeVendas descending
                            select new
                            {
                                f.FaixaId,
                                f.Nome,
                                Total = totalDeVendas
                            };

                var produtoMaisVendido = query.First();
                Console.WriteLine("{0}\t{1}\t{2}", produtoMaisVendido.FaixaId, produtoMaisVendido.Nome, produtoMaisVendido.Total);

                Console.WriteLine();

                var queryCliente = from inf in context.ItemNotaFiscals
                                   where inf.FaixaId == produtoMaisVendido.FaixaId
                                   select new
                                   {
                                       NomeCliente = inf.NotaFiscal.Cliente.PrimeiroNome + " " + inf.NotaFiscal.Cliente.Sobrenome
                                   };

                foreach (var item in queryCliente)
                {
                    Console.WriteLine(item.NomeCliente);
                }

            }
        }

        private static void _4_SelfJoin()
        {
            var nomeDaMusica = "Smells Like Teen Spirit";

            using (var context = new AluraTunesEntities())
            {
                var faixaIds = context.Faixas.Where(f => f.Nome == nomeDaMusica).Select(f => f.FaixaId);

                var query = from comprouItem in context.ItemNotaFiscals
                            join comprouTambem in context.ItemNotaFiscals
                                on comprouItem.NotaFiscalId equals comprouTambem.NotaFiscalId
                            where faixaIds.Contains(comprouItem.FaixaId) &&
                                comprouItem.FaixaId != comprouTambem.FaixaId
                            orderby comprouTambem.NotaFiscalId, comprouTambem.Faixa.Nome
                            select comprouTambem;

                foreach (var item in query)
                {
                    Console.WriteLine("{0}\t{1}", item.NotaFiscalId, item.Faixa.Nome);
                }
            }
        }

        private static void _5_()
        {
            using (var context = new AluraTunesEntities())
            {
                var mesAniversario = 1;

                while (mesAniversario <= 12)
                {
                    Console.WriteLine("Mês: {0}", mesAniversario);

                    var query = (from f in context.Funcionarios
                                where f.DataNascimento.Value.Month == mesAniversario
                                orderby f.DataNascimento.Value.Month, f.DataNascimento.Value.Day
                                select f).ToList();

                    mesAniversario += 1;

                    foreach(var f in query)
                        Console.WriteLine("{0:dd/MM}\t{1} {2}", f.DataNascimento, f.PrimeiroNome, f.Sobrenome);

                    Console.WriteLine();
                }
            }
        }

        private static void _6_LinqParalelo()
        {
            var barcodWriter = new BarcodeWriter();
            barcodWriter.Format = BarcodeFormat.QR_CODE;
            barcodWriter.Options = new ZXing.Common.EncodingOptions
            {
                Width = 100,
                Height = 100
            };

            //barcodWriter.Write("Meu Teste").Save("QRCode.jpg", ImageFormat.Jpeg);

            if (!Directory.Exists(Imagens))
                Directory.CreateDirectory(Imagens);

            using (var context = new AluraTunesEntities())
            {
                var queryFaixas = (from f in context.Faixas
                             select f).ToList();

                Stopwatch stopwatch = Stopwatch.StartNew();

                var queryCodigos = queryFaixas.AsParallel().Select(f => new
                {
                    Arquivo = string.Format("{0}\\{1}.jpg", Imagens, f.FaixaId),
                    Imagem = barcodWriter.Write(string.Format("aluratunes.com/faixa/{0}", f.FaixaId))
                });

                stopwatch.Stop();

                Console.WriteLine("Códigos gerados: {0} em {1} segundos.", queryCodigos.Count(), stopwatch.ElapsedMilliseconds / 1000.0);
                //4,8 - 1,3

                stopwatch = Stopwatch.StartNew();

                //foreach (var item in queryCodigos)
                //    item.Imagem.Save(item.Arquivo, ImageFormat.Jpeg);

                //foreach da instrução 'AsParallel'
                queryCodigos.ForAll(item => item.Imagem.Save(item.Arquivo, ImageFormat.Jpeg));

                stopwatch.Stop();
                Console.WriteLine();
                Console.WriteLine("Códigos salvos: {0} em {1} segundos.", queryCodigos.Count(), stopwatch.ElapsedMilliseconds / 1000.0);
                //4,82 - 2,12
            }
        }

        private static void ImprimirPagina(int TAMANHO_PAGINA, AluraTunesEntities context, int numeroPagina)
        {
            var query = from nf in context.NotaFiscals
                        orderby nf.NotaFiscalId
                        select new
                        {
                            Numero = nf.NotaFiscalId,
                            Data = nf.DataNotaFiscal,
                            Cliente = nf.Cliente.PrimeiroNome + " " + nf.Cliente.Sobrenome,
                            Total = nf.Total
                        };

            int numeroDePulos = (numeroPagina - 1) * TAMANHO_PAGINA;

            query = query.Skip(numeroDePulos);

            query = query.Take(TAMANHO_PAGINA);

            Console.WriteLine();
            Console.WriteLine("Número de Paginas: {0}", numeroPagina);

            foreach (var item in query)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", item.Numero, item.Data, item.Cliente, item.Total);
            }
        }
    }
}
